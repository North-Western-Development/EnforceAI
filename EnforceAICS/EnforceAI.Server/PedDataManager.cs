using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using EnforceAI.Common.Enums;
using EnforceAI.Server.Types;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Server;

internal static class PedDataManager
{
    private static readonly Dictionary<int, PedData> dataList = new Dictionary<int, PedData>();
    private static readonly Dictionary<int, PedData> idLookup = new Dictionary<int, PedData>();
    
    internal static async Task<PedData> GetDataForPed(Ped ped, Gender gender, int headComponent, int headTexture)
    {
        if (dataList.TryGetValue(ped.NetworkId, out PedData data)) return data;

        List<dynamic> pedDatas = await DBConnector.Query("SELECT * FROM enforceai_peds WHERE ped_model = @Model AND head_component = @Component AND head_texture = @Texture", new { Model = ped.Model, Component = headComponent, Texture = headTexture });
        
        if(pedDatas != null && pedDatas.Count > 0)
        {
            dynamic selected = null;
            
            foreach (dynamic entry in pedDatas)
            {
                if(entry == null) continue;
                if(idLookup.ContainsKey(entry.id)) continue;
                selected = entry;
                break;
            }

            if(selected != null)
            {
                data = new PedData();
                data.FirstName = selected.firstname;
                data.MiddleInitial = selected.middle;
                data.LastName = selected.lastname;
                data.DateOfBirth = DateTime.Parse(selected.dob);
                data.Gender = selected.gender switch
                {
                    'm' => Gender.Male,
                    'f' => Gender.Female,
                    _ => Gender.Other
                };

                dataList.Add(ped.NetworkId, data);
                idLookup.Add(selected.id, data);
                
                return data;
            }
        }
        
        data = new PedData(Configs.Names, gender);
        dataList.Add(ped.NetworkId, data);
        try
        {
            dynamic temp = await DBConnector.QuerySingle("INSERT INTO enforceai_peds (ped_model, gender, firstname, middle, lastname, dob, licenses, head_component, head_texture) VALUES (@Model, @Gender, @Firstname, @Middle, @Lastname, @Dob, @Licenses, @Component, @Texture) RETURNING id", new { Model = ped.Model, Gender = data.Gender switch
            {
                Gender.Male => 'm',
                Gender.Female => 'f',
                _ => 'x'
            }, Firstname = data.FirstName, Middle = data.MiddleInitial, Lastname = data.LastName,
            Dob = data.DateOfBirth.ToString("M/d/yyyy"), Licenses = "{}", Component = headComponent, Texture = headTexture });
            int id = temp.id;
            idLookup.Add(id, data);
            return data;
        }
        catch (Exception e)
        {
            Print(e);
            data.FirstName = "Error";
            data.LastName = "Error";
            return data;
        }
    }
}