using CitizenFX.Core;

namespace EnforceAI.Server;

public struct SpeedZone
{
    public float MaxSpeed;
    public float Radius;
    public Vector3 Center;
    public Player Owner;
}