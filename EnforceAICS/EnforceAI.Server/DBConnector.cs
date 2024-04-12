using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using Dapper;
using MySql.Data.MySqlClient;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Server;

internal static class DBConnector
{
    internal static MySqlConnection Connection;

    internal static async void InitializeDBConnector()
    {
        try
        {
            Connection = new MySqlConnection(ConvertConnectionString(GetConvar("mysql_connection_string", "")));
            Connection.Open();
            Print("CONNECTED SUCCESSFULLY: " + Connection.ServerVersion);
        }
        catch(Exception e)
        {
            #if DEBUG
            //Print(e);
            #endif
            Print("CONNECTION FAILED, CHECK CONNECTION STRING");
        }
    }

    private static string ConvertConnectionString(string original)
    {
        string working = original.Substring(0, original.Length - (original.Length - original.IndexOf('?')));
        working = working.Replace("mysql://", "");
        string userAndPass = working.Split('@')[0];
        string user = userAndPass.Split(':')[0];
        string pass = userAndPass.Split(':')[1];
        string serverAndDb = working.Split('@')[1];
        string server = serverAndDb.Split('/')[0];
        string database = serverAndDb.Split('/')[1];
        string connectionString = $"server={server};database={database};uid={user};pwd={pass}";
        return connectionString;
    }

    internal static async Task<dynamic> QuerySingle(string query, object parameters)
    {
        while (Connection.State != ConnectionState.Open)
        {
            await BaseScript.Delay(100);
        }

        try
        {
            return await Connection.QuerySingleAsync(query, parameters);
        }
        catch(Exception e)
        {
            Print(e);
            return null;
        }
    }
    
    internal static async Task<List<dynamic>> Query(string query, object parameters)
    {
        while (Connection.State != ConnectionState.Open)
        {
            await BaseScript.Delay(100);
        }

        try
        {
            return (await Connection.QueryAsync(query, parameters)).ToList();
        }
        catch(Exception e)
        {
            Print(e);
            return null;
        }
    }
    
    internal static async Task<int?> Insert(string query, object parameters)
    {
        while (Connection.State != ConnectionState.Open)
        {
            await BaseScript.Delay(100);
        }

        try
        {
            return await Connection.ExecuteAsync(query, parameters);
        }
        catch(Exception e)
        {
            Print(e);
            return null;
        }
    }
}