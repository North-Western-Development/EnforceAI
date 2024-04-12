using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using EnforceAI.Common;
using EnforceAI.Server.Types;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Server
{
    public class EnforceAI : BaseScript
    {
        private readonly List<int> props = new List<int>();
        private readonly List<SpeedZone> speedZones = new List<SpeedZone>();
        private readonly Dictionary<string, bool> dutyStatus = new Dictionary<string, bool>();
        private bool hasInitialized;
        
        public EnforceAI()
        {
            EventHandlers["EnforceAI::server:Duty"] += new Action<Player, bool>(PlayerDutyStatus);
            //EventHandlers["EnforceAI::server:CreateSpeedZone"] += new Action<Player, string, string>(CreateSpeedZone);
            EventHandlers["playerDropped"] += new Action<Player>(([FromSource] Player player) =>
            {
                if(!dutyStatus.ContainsKey(player.Name)) return;
                dutyStatus.Remove(player.Name);
            });
            EventHandlers["EnforceAI::server:AddPropToList"] += new Action<int>((netid) =>
            {
                props.Add(netid);
            });
            EventHandlers["onResourceStop"] += new Action<string>((resource) =>
            {
                if(resource != GetCurrentResourceName()) return;

                foreach (int propNetId in props)
                {
                    int prop = NetworkGetEntityFromNetworkId(propNetId);
                    if (DoesEntityExist(prop))
                    {
                        DeleteEntity(prop);
                    }
                }
            });
            EventHandlers["EnforceAI::server:GetPedData"] += new Action<Player, int>(([FromSource] client, netId) =>
            {
                Ped ped = (Ped)Entity.FromNetworkId(netId);
                if (ped == null)
                {
                    TriggerClientEvent(client, "EnforceAI::client:ReturnPedData:" + netId, "NO SUCH NETWORK ID");
                };
                PedData data = new PedData(Configs.Names);
                data.AssociatePed(ped);
                Print("EnforceAI::client:ReturnPedData:" + netId);
                File.WriteAllText(GetResourcePath(GetCurrentResourceName()) + "\\test.txt", JsonConvert.SerializeObject(data));
                PedData test = JsonConvert.DeserializeObject<PedData>(JsonConvert.SerializeObject(data));
                Print(test);
                TriggerClientEvent(client, "EnforceAI::client:ReturnPedData:" + netId, JsonConvert.SerializeObject(data));
            });

            Tick += PlayerBlips;
            
            ScriptInitialization();
        }
        
        private void ScriptInitialization()
        {
            if (hasInitialized) return;
            hasInitialized = true;
            Print("======================== ENFORCEAI ========================");
            Print("Developed by North Western Development and Contributors");
            Print($"Version: {typeof(EnforceAI).Assembly.GetName().Version}");

            Configs.Names = JsonConvert.DeserializeObject<Names>(File.ReadAllText(GetResourcePath(GetCurrentResourceName()) + "\\configs\\names.json"));

            Configs.Names.FirstNames.Male = Configs.Names.FirstNames.Male.Concat(Configs.Names.FirstNames.Neutral).ToList();
            Configs.Names.FirstNames.Female = Configs.Names.FirstNames.Female.Concat(Configs.Names.FirstNames.Neutral).ToList();
            
            PedData data = new PedData(Configs.Names);
            
            Print(data);
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
                    player.TriggerEvent("EnforceAI::client:PlayerBlips", positions);
                }
            }
        }

        private void PlayerDutyStatus([FromSource] Player client, bool status)
        {
            dutyStatus[client.Name] = status;
        }

        /*private void CreateSpeedZone([FromSource] Player client, string maxSpeed, string zoneSize)
        {
            Vector3 position = GetEntityCoords(client.Character.Handle);
            
            speedZones.Add(new SpeedZone()
            {
                Center = position,
                MaxSpeed = float.Parse(maxSpeed) * 2.236936f,
                Radius = float.Parse(zoneSize),
                Owner = client
            });
            
            TriggerClientEvent("EnforceAI::client:NewSpeedZone", speedZones.Last());
        }*/
    }
}