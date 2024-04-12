using System;
using System.Collections.Generic;
using CitizenFX.Core;
using EnforceAI.Common.Interfaces;
using EnforceAI.Server;
using Gender = EnforceAI.Common.Interfaces.Gender;

namespace EnforceAI.Common;

public class PedData
{
    public string FirstName { get; private set; }
    public string MiddleInitial { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public Licenses Licenses { get; private set; }
    public List<VehicleData> RegisteredVehicles { get; private set; }
    public Ped AssignedPed { get; private set; }

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

    public PedData(Names names)
    {
        //TODO: IMPLEMENT RANDOM GENERATION
        Random gen = new Random();
        Gender = (Gender) gen.Next(0, 2);
        FirstName = GenerateName(names, Gender, true);
        MiddleInitial = middleInitialTable[new Random().Next(0, 25)];
        LastName = GenerateName(names);
        DateOfBirth = GenerateDateOfBirth();
        Licenses = new Licenses();
        RegisteredVehicles = new List<VehicleData>();
    }
    
    public void AssociatePed(Ped ped)
    {
        AssignedPed = ped;
    }

    public string GetName()
    {
        return FirstName + " " + MiddleInitial + ". " + LastName;
    }

    public string GetDateOfBirthString()
    {
        return DateOfBirth.ToString("dd MMMM yyyy");
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
        int years = gen.Next(18, 96);
        DateTime initial = DateTime.Now.AddYears(-years);
        initial = initial.AddMonths(-gen.Next(0, 11));
        initial = initial.AddDays(-gen.Next(0, 31));
        return initial;
    }
}