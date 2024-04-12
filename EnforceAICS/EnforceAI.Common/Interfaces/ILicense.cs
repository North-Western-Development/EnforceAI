using System;

namespace EnforceAI.Common.Interfaces;

public interface ILicense
{
    public DateTime Expiration { get; }
    public DateTime Issued { get; }
    public LicenseStatus Status { get; }
}