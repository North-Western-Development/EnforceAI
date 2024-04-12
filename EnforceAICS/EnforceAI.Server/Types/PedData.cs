using System;
using System.Collections.Generic;
using CitizenFX.Core;
using EnforceAI.Common;
using EnforceAI.Common.Enums;
using EnforceAI.Common.DataHolders;
using EnforceAI.Common.Types;

namespace EnforceAI.Server.Types;

public class PedData : IPedData
{
    public string FirstName { get; internal set; }
    public string MiddleInitial { get; internal set; }
    public string LastName { get; internal set; }
    public DateTime DateOfBirth { get; internal set; }
    public Gender Gender { get; internal set; }
    public Licenses Licenses { get; internal set; }
    public List<VehicleData> RegisteredVehicles { get; internal set; }
    public int AssignedPed { get; private set; }
    
    public int Age
    {
        get
        {
            DateTime now = DateTime.Today;
            int age = now.Year - DateOfBirth.Year;
            if (DateOfBirth > now.AddYears(-age)) age--;
            return age;
        }
    }
    
    private static string[] middleInitialTable = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    
    internal PedData(Names names)
    {
        Random gen = new Random();
        Gender = (Gender) gen.Next(0, 2);
        FirstName = GenerateName(names, Gender, true);
        MiddleInitial = middleInitialTable[new Random().Next(0, 25)];
        LastName = GenerateName(names);
        DateOfBirth = GenerateDateOfBirth();
        //TODO: IMPLEMENT RANDOM GENERATION OF LICENSES AND VEHICLES
        Licenses = new Licenses();
        RegisteredVehicles = new List<VehicleData>();
    }

    internal PedData()
    {
        
    }
    
    internal static string GenerateName(Names names, Gender gender = Gender.Other, bool first = false)
    {
        string[] nameArray = (gender == Gender.Other || !first) ? names.LastNames.ToArray() : (gender == Gender.Male) ? names.FirstNames.Male.ToArray() : names.FirstNames.Female.ToArray();

        int number = new Random().Next(0, nameArray.Length - 1);

        return nameArray[number];
    }

    internal static DateTime GenerateDateOfBirth()
    {
        Random gen = new Random();
        int years = gen.Next(18, 82);
        DateTime initial = DateTime.Now.AddYears(-years);
        initial = initial.AddMonths(-gen.Next(0, 11));
        initial = initial.AddDays(-gen.Next(0, 31));
        return initial;
    }
    
    internal void AssociatePed(Ped ped)
    {
        AssignedPed = ped.NetworkId;
    }
    
    public string GetName()
    {
        return FirstName + " " + MiddleInitial + ". " + LastName;
    }

    public string GetDateOfBirthString()
    {
        return DateOfBirth.ToString("dd MMMM yyyy");
    }

    public override string ToString()
    {
        return GetName() + " - " + Gender + " - " + GetDateOfBirthString() + " - " + Age;
    }
}