using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core;
using MenuAPI;
using static CitizenFX.Core.Native.API;
using static EnforceAI.Common.Utilities;

namespace EnforceAI.Client
{
    public class EnforceAI : BaseScript
    {
        internal bool IsOnDuty;
        internal string Department = "LSPD";
        internal readonly Dictionary<string, Blip> playerBlips = new Dictionary<string, Blip>();

        private bool hasInitialized;
        
        public EnforceAI()
        {
            EventHandlers["EnforceAI::client:SetDuty"] += new Action<bool?>(SetDutyStatus);
            EventHandlers["EnforceAI::client:PlayerBlips"] += new Action<ExpandoObject>(PlayerPositionList);
            
            ScriptInitialization();
        }
        
        private void ScriptInitialization()
        {
            if (hasInitialized) return;
            hasInitialized = true;
            Print("======================== ENFORCEAI ========================");
            Print("Developed by North Western Development and Contributors");
            Print($"Version: {typeof(EnforceAI).Assembly.GetName().Version}");
            
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;

            Menu mainMenu = MenuManager.CreateMainMenu(this);

            Menu sceneMenu = MenuManager.CreateSceneMenu(this);

            RegisterCommand("EnforceAI::client:OpenMenu", new Action(() =>
            {
                if (MenuController.IsAnyMenuOpen())
                {
                    MenuController.CloseAllMenus();
                }
                else
                {
                    mainMenu.OpenMenu();
                }
            }), false);

            RegisterCommand("EnforceAI::client:OpenSceneMenu", new Action(() =>
            {
                if (MenuController.IsAnyMenuOpen())
                {
                    MenuController.CloseAllMenus();
                }
                else
                {
                    sceneMenu.OpenMenu();
                }
            }), false);
            
            RegisterKeyMapping("EnforceAI::client:OpenSceneMenu", "Open the scene menu", "keyboard", "HOME");
            
            RegisterKeyMapping("EnforceAI::client:OpenMenu", "Open the main menu", "keyboard", "END");
        }

        internal void SetDutyStatus(bool? status = null)
        {
            if (!status.HasValue) status = !IsOnDuty;
            IsOnDuty = status.Value;
            ClientUtilities.Tooltip($"You are now {(IsOnDuty ? "~g~on~s~ duty" : "~r~off~s~ duty")}!");
            TriggerServerEvent("EnforceAI::server:Duty", IsOnDuty);
        }

        private void PlayerPositionList(ExpandoObject positions)
        {
            foreach (KeyValuePair<string, object> position in positions)
            {
                if(Players[position.Key] == Player.Local) continue;
                if (playerBlips.TryGetValue(position.Key, out Blip locationBlip))
                {
                    Vector4 parent = (Vector4) position.Value;
                    locationBlip.Position = (Vector3)parent;
                    locationBlip.Rotation = (int)parent.W;
                }
                else
                {
                    Vector4 parent = (Vector4) position.Value;
                    Vector3 pos = (Vector3) parent;
                    int rot = (int)parent.W;
                    playerBlips[position.Key] = ClientUtilities.CreateBlip(pos, rot, position.Key, 38, 0.5f, 2, 399);
                }
            }
        }
    }
}