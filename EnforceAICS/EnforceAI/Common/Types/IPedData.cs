using System;
using System.Collections.Generic;
using CitizenFX.Core;
using EnforceAI.Common.DataHolders;
using Newtonsoft.Json;
using Gender = EnforceAI.Common.Enums.Gender;

namespace EnforceAI.Common.Types;

public interface IPedData
{
    [JsonProperty]
    public string FirstName { get; }
    [JsonProperty]
    public string MiddleInitial { get; }
    [JsonProperty]
    public string LastName { get; }
    [JsonProperty]
    public DateTime DateOfBirth { get; }
    [JsonProperty]
    public Gender Gender { get; }
    [JsonIgnore]
    public Licenses Licenses { get; }
    [JsonIgnore]
    public List<VehicleData> RegisteredVehicles { get; }
    [JsonProperty]
    public int AssignedPed { get; }

    [JsonIgnore]
    public int Age
    {
        get;
    }
}