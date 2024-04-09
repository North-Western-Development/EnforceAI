using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Server
{
    public class EnforceAI : BaseScript
    {
        private readonly Dictionary<string, bool> dutyStatus = new Dictionary<string, bool>();
        
        public EnforceAI()
        {
            EventHandlers["onResourceStart"] += new Action<string>(ScriptInitialization);
            EventHandlers["EnforceAI::server:Duty"] += new Action<Player, bool>(PlayerDutyStatus);
            EventHandlers["playerDropped"] += new Action<Player>(([FromSource] Player player) =>
            {
                if(!dutyStatus.ContainsKey(player.Name)) return;
                dutyStatus.Remove(player.Name);
            });

            Tick += PlayerBlips;
        }
        
        private void ScriptInitialization(string resource)
        {
            if (resource != GetCurrentResourceName()) return;
            Print("======================== ENFORCEAI ========================");
            Print("Developed by North Western Development and Contributors");
            Print($"Version: {typeof(EnforceAI).Assembly.GetName().Version}");
        }

        private async Task PlayerBlips()
        {
            await Task.Delay(500);
            Dictionary<string, Vector4> positions = new Dictionary<string, Vector4>(); 
            foreach (Player player in Players)
            {
                if(!dutyStatus.TryGetValue(player.Name, out bool duty)) continue;
                if (duty)
                {
                    Vector4 position = (Vector4)GetEntityCoords(player.Character.Handle);
                    position.W = GetEntityHeading(player.Character.Handle);
                    positions.Add(player.Name, position);
                }
            }

            foreach (KeyValuePair<string, bool> status in dutyStatus)
            {
                if(status.Value)
                {
                    Player player = Players[status.Key];
                    Print(player.Name);
                    player.TriggerEvent("EnforceAI::client:PlayerBlips", positions);
                }
            }
        }

        private void PlayerDutyStatus([FromSource] Player client, bool status)
        {
            dutyStatus[client.Name] = status;
        }
    }
}