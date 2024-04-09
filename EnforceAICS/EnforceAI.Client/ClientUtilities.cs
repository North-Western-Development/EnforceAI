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
    }
}