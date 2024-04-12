﻿using System;
using EnforceAI.Common.Enums;

namespace EnforceAI.Common.Types;

public class FishingLicense : ILicense
{
    public DateTime Expiration { get; }
    public DateTime Issued { get; }
    public LicenseStatus Status { get; }
    
    internal FishingLicense()
    {
        
    }
}