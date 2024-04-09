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
        internal bool IsOnDuty = false;
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
            
            Menu mainMenu = new Menu("EnforceAI Menu", "The main menu for EnforceAI");
            MenuController.AddMenu(mainMenu);
            
            MenuCheckboxItem dutyCheck = new MenuCheckboxItem("Set duty status", "Set your duty status for EnforceAI");
            dutyCheck.LeftIcon = MenuItem.Icon.BRIEFCASE;
            dutyCheck.Checked = IsOnDuty;

            mainMenu.AddMenuItem(dutyCheck);

            MenuItem debugMenuItem = new MenuItem("~o~Debug Options~s~", "EnforceAI debug options");
            debugMenuItem.LeftIcon = MenuItem.Icon.WARNING;
            
            mainMenu.AddMenuItem(debugMenuItem);

            Menu debugMenu = new Menu("EnforceAI Debug Menu", "Debug Options");

            MenuItem toggleDutyStatus = new MenuItem("Toggle Duty Status", "Toggle your duty status");
            toggleDutyStatus.LeftIcon = MenuItem.Icon.BRIEFCASE;
            
            debugMenu.AddMenuItem(toggleDutyStatus);
            
            MenuItem calloutNotification = new MenuItem("Trigger Test Callout Notification", "Triggers a callout notification for testing");
            calloutNotification.LeftIcon = MenuItem.Icon.MISSION_STAR;
            
            debugMenu.AddMenuItem(calloutNotification);
            
            debugMenu.OnItemSelect += (menu, selectedItem, itemIndex) =>
            {
                if (selectedItem == toggleDutyStatus)
                {
                    SetDutyStatus();
                } else if (selectedItem == calloutNotification)
                {
                    ClientUtilities.Notification("~b~Type:~s~~o~ Officer in Distress (10-99)~s~~n~~g~Location:~s~ ~p~Del Perro Freeway, Los Santos~s~", true, 140, "CHAR_CALL911", true, 0, Department + " Dispatch", "Call Received");
                }
            };
            
            MenuController.AddSubmenu(mainMenu, debugMenu);
            
            MenuController.BindMenuItem(mainMenu, debugMenu, debugMenuItem);

            mainMenu.OnCheckboxChange += (menu, checkItem, itemIndex, newState) =>
            {
                if(checkItem == dutyCheck)
                {
                    SetDutyStatus(newState);
                }
            };

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
            
            RegisterKeyMapping("EnforceAI::client:OpenMenu", "Open the debug menu", "keyboard", "END");
        }

        private void SetDutyStatus(bool? status = null)
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
                    // ReSharper disable once PossibleInvalidCastException
                    locationBlip.Rotation = (int)parent.W;
                }
                else
                {
                    Vector4 parent = (Vector4) position.Value;
                    Vector3 pos = (Vector3) parent;
                    int rot = (int)parent.W;
                    playerBlips[position.Key] = new Blip(AddBlipForCoord(pos.X, pos.Y, pos.Z));
                    SetBlipSprite(playerBlips[position.Key].Handle, 399);
                    SetBlipDisplay(playerBlips[position.Key].Handle, 2);
                    SetBlipScale(playerBlips[position.Key].Handle, 0.5f);
                    SetBlipColour(playerBlips[position.Key].Handle, 38);
                    SetBlipRotation(playerBlips[position.Key].Handle, rot);
                    SetBlipAsShortRange(playerBlips[position.Key].Handle, true);
                    BeginTextCommandSetBlipName("STRING");
                    AddTextComponentString(position.Key);
                    EndTextCommandSetBlipName(playerBlips[position.Key].Handle);
                }
            }
        }
    }
}