using EnforceAI.Common.Types;

namespace EnforceAI.Common.DataHolders;

public struct Licenses
{
    public DriversLicense DriversLicense { get; private set; }
    public WeaponsLicense WeaponsLicense { get; private set; }
    public HuntingLicense HuntingLicense { get; private set; }
    public FishingLicense FishingLicense { get; private set; }
    public PilotsLicense PilotsLicense { get; private set; }
}