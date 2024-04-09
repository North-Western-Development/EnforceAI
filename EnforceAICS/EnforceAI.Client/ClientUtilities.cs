using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

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
    }
}