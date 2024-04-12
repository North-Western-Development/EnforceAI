using System;
using EnforceAI.Common.Interfaces;

namespace EnforceAI.Common;

public class DriversLicense : ILicense
{
    public DateTime Expiration { get; }
    public DateTime Issued { get; }
    public LicenseStatus Status { get; }
    
    internal DriversLicense()
    {
        
    }
}