using System;

namespace EnforceAI.Common.Enums;

public interface ILicense
{
    public DateTime Expiration { get; }
    public DateTime Issued { get; }
    public LicenseStatus Status { get; }
}