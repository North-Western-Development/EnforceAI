using System;
using System.Collections.Generic;
using EnforceAI.Common.DataHolders;
using EnforceAI.Common.Interfaces;
using EnforceAI.Common.Types;
using Gender = EnforceAI.Common.Enums.Gender;

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
    
    internal PedData() {}
    
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