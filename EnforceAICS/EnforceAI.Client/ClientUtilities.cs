using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Client
{
    internal static class ClientUtilities
    {
        internal static void Notification(string notifcation, bool adv = false, int color = 140, string textureDict = "", bool flash = false,  int iconType = 0, string sender = "", string subject = "", bool saveToBrief = false)
        {
            BeginTextCommandThefeedPost("STRING");
            AddTextComponentSubstringPlayerName(notifcation);
            ThefeedNextPostBackgroundColor(color);
            if(adv) EndTextCommandThefeedPostMessagetext(textureDict, textureDict, flash, iconType, sender, subject);
            EndTextCommandThefeedPostTicker(false, saveToBrief);
        }

        internal static void Tooltip(string tooltip)
        {
            SetTextComponentFormat("STRING");
            AddTextComponentString(tooltip);
            DisplayHelpTextFromStringLabel(0, false, true, -1);
        }

        internal static Blip CreateBlip(Vector3 pos, int rot, string name, int color, float scale, int displayType, int sprite, bool shortRange = true)
        {
            Blip blip = new Blip(AddBlipForCoord(pos.X, pos.Y, pos.Z));
            SetBlipSprite(blip.Handle, sprite);
            SetBlipDisplay(blip.Handle, displayType);
            SetBlipScale(blip.Handle, scale);
            SetBlipColour(blip.Handle, color);
            SetBlipRotation(blip.Handle, rot);
            SetBlipAsShortRange(blip.Handle, shortRange);
            BeginTextCommandSetBlipName("STRING");
            AddTextComponentString(name);
            EndTextCommandSetBlipName(blip.Handle);

            return blip;
        }

        internal static async Task RequestModelClient(uint model)
        {
            if (!IsModelValid(model)) return;
            RequestModel(model);
            while (!HasModelLoaded(model))
            {
                await BaseScript.Delay(1);
            }
        }

        internal static async Task<Ped?> GetNearestDownedOrInjuredPed(Vector3 coords, float maxDistanceSqrd, bool allowPlayer = false)
        {
            float nearestDistance = float.MaxValue;
            Ped? nearestPed = null;

            foreach (Ped ped in World.GetAllPeds())
            {
                await BaseScript.Delay(0);
                if(ped == null) continue;
                if(!ped.IsHuman) continue;
                if(ped.IsPlayer && !allowPlayer) continue;
                if(ped.HealthFloat > (ped.MaxHealthFloat * 0.75f) && !ped.IsDead) continue;
                float dist = ped.Position.DistanceToSquared2D(coords);
                if(dist > maxDistanceSqrd || (nearestPed != null && dist > nearestDistance)) continue;
                nearestPed = ped;
                nearestDistance = ped.Position.DistanceToSquared2D(coords);
            }
            
            return nearestPed;
        }
        
        internal static async Task<List<Ped>> GetAllNearbyDownedOrInjuredPed(Vector3 coords, float maxDistanceSqrd, bool allowPlayer = false, int? ignoreHandle = null)
        {
            List<Ped> peds = new List<Ped>();
            
            foreach (Ped ped in World.GetAllPeds())
            {
                await BaseScript.Delay(0);
                if(ped == null) continue;
                if(ignoreHandle != null && ped.Handle == ignoreHandle) continue;
                if(!ped.IsHuman) continue;
                if(ped.IsPlayer && !allowPlayer) continue;
                if(ped.Position.DistanceToSquared2D(coords) > maxDistanceSqrd) continue;
                if(ped.HealthFloat > (ped.MaxHealthFloat * 0.75f) && !ped.IsDead) continue;
                peds.Add(ped);
            }
            return peds;
        }
        
        internal static async Task<Ped?> GetNearestLivingPed(Vector3 coords, float maxDistanceSqrd, bool allowPlayer = false)
        {
            float nearestDistance = float.MaxValue;
            Ped? nearestPed = null;
            Ped?[] peds = World.GetAllPeds();
            
            for (int i = 0; i < peds.Length; i++)
            {
                Ped? ped = peds[i];
                await BaseScript.Delay(0);
                if(ped == null) continue;
                if(!ped.IsHuman) continue;
                if(ped.IsPlayer && !allowPlayer) continue;
                if(ped.IsDead) continue;
                float dist = ped.Position.DistanceToSquared2D(coords);
                if(dist > maxDistanceSqrd || (nearestPed != null && dist > nearestDistance)) continue;
                nearestPed = ped;
                nearestDistance = ped.Position.DistanceToSquared2D(coords);
            }
            
            return nearestPed;
        }
    }
}