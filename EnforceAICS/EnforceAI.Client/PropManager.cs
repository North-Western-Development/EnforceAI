using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Client;

internal static class PropManager
{
    internal static List<string> Props = new List<string>()
    {
        "prop_roadcone01a",
        "prop_consign_02a",
        "prop_mp_cone_04"
    };
    
    internal static bool DrawGhost;
    internal static bool SpawnFrozen;
    internal static string SpawnModel;

    internal static bool RotateRight;
    internal static bool RotateLeft;
    internal static bool RotateFaster;

    private static bool propSpawned;
    private static string currentPropSpawned;
    private static int currentPropHandle;

    private static readonly List<int> myProps = new List<int>();
    
    private static float currentYaw = 0;
    
    internal static async Task DrawLoop()
    {
        if (DrawGhost)
        {
            if (!propSpawned || currentPropSpawned != SpawnModel)
            {
                if (propSpawned)
                {
                    DeleteObject(ref currentPropHandle);
                    currentPropHandle = 0;
                    propSpawned = false;
                }

                currentPropSpawned = SpawnModel;
                await ClientUtilities.RequestModelClient((uint)GetHashKey(SpawnModel));
                
                Vector3 pos = GetEntityCoords(PlayerPedId(), true);
                Vector3 forward = GetEntityForwardVector(PlayerPedId());

                currentPropHandle = CreateObject(GetHashKey(SpawnModel), pos.X + (forward.X * 1.5f), pos.Y + (forward.Y * 1.5f), pos.Z, true, false, false);
                PlaceObjectOnGroundProperly(currentPropHandle);
                SetEntityCollision(currentPropHandle, false, false);
                SetEntityAlpha(currentPropHandle, 102, 0);
                NetworkSetEntityInvisibleToNetwork(NetworkGetNetworkIdFromEntity(currentPropHandle), true);
                propSpawned = true;
                return;
            }
            Vector3 position = GetEntityCoords(PlayerPedId(), true);
            Vector3 forwardVector = GetEntityForwardVector(PlayerPedId());
            SetEntityCoords(currentPropHandle, position.X + (forwardVector.X * 1.5f), position.Y + (forwardVector.Y * 1.5f), position.Z, false, false, false, false);
            PlaceObjectOnGroundProperly(currentPropHandle);
            Vector3 rot = GetEntityRotation(currentPropHandle, 2);
            if ((RotateRight && RotateLeft) || (!RotateRight && !RotateLeft))
            {
                SetEntityRotation(currentPropHandle, rot.X, rot.Y, currentYaw, 2, false);
                return;
            }
            if (RotateRight)
            {
                if (currentYaw + (0.2f * (RotateFaster ? 5f : 1f)) > 360f)
                    currentYaw = 0;
                currentYaw += (0.2f * (RotateFaster ? 5f : 1f));
                SetEntityRotation(currentPropHandle, rot.X, rot.Y, currentYaw, 2, false);
            }
            else if (RotateLeft)
            {
                if (currentYaw - (0.2f * (RotateFaster ? 5f : 1f)) < 0f)
                    currentYaw = 360;
                currentYaw -= (0.2f * (RotateFaster ? 5f : 1f));
                SetEntityRotation(currentPropHandle, rot.X, rot.Y, currentYaw, 2, false);
            }
        }
        else if (propSpawned)
        {
            DeleteObject(ref currentPropHandle);
            currentPropHandle = 0;
            propSpawned = false;
            await BaseScript.Delay(250);
        }
        else
        {
            await BaseScript.Delay(250);
        }
    }
    
    internal static void FinalizePlacement()
    {
        Vector3 position = GetEntityCoords(PlayerPedId(), true);
        Vector3 forwardVector = GetEntityForwardVector(PlayerPedId());
        SetEntityCoords(currentPropHandle, position.X + (forwardVector.X * 1.5f), position.Y + (forwardVector.Y * 1.5f), position.Z, false, false, false, false);
        PlaceObjectOnGroundProperly(currentPropHandle);
        if (SpawnFrozen)
        {
            FreezeEntityPosition(currentPropHandle, true);
        }
        SetEntityCollision(currentPropHandle, true, true);
        SetEntityAlpha(currentPropHandle, 255, 0);
        NetworkSetEntityInvisibleToNetwork(NetworkGetNetworkIdFromEntity(currentPropHandle), false);
        propSpawned = false;
        BaseScript.TriggerServerEvent("EnforceAI::server:AddPropToList", NetworkGetNetworkIdFromEntity(currentPropHandle));
        myProps.Add(currentPropHandle);
    }

    internal static void CleanUp()
    {
        DrawGhost = false;
        if (propSpawned)
        {
            DeleteObject(ref currentPropHandle);
            currentPropHandle = 0;
            propSpawned = false;
        }
    }

    internal static void DeleteNearestProp()
    {
        Vector3 pos = GetEntityCoords(PlayerPedId(), false);
        float closestDistance = -1.0f;
        int closestHandle = 0;
        foreach (string prop in Props)
        {
            int temp = GetClosestObjectOfType(pos.X, pos.Y, pos.Z, 1.0f, (uint)GetHashKey(prop), false, true, true);
            Print(temp);
            if (temp == 0) continue;
            Vector3 objDist = GetEntityCoords(temp, false);
            float tempDist = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, objDist.X, objDist.Y, objDist.Z, true);
            if (closestDistance == -1.0f)
            {
                closestDistance = tempDist;
                closestHandle = temp;
            }
            else if (closestDistance > tempDist)
            {
                closestDistance = tempDist;
                closestHandle = temp;
            }
        }
        
        if(closestDistance == -1.0f) return;
        
        Print(closestDistance);
        NetworkRequestControlOfEntity(closestHandle);
        SetEntityAsMissionEntity(closestHandle, true, true);
        DeleteObject(ref closestHandle);
    }

    internal static void DeleteAllProps()
    {
        foreach (int prop in myProps)
        {
            int hand = prop;
            NetworkRequestControlOfEntity(prop);
            SetEntityAsMissionEntity(prop, true, true);
            DeleteObject(ref hand);
        }
    }
}