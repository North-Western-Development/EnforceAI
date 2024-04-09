﻿using System;
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
        internal static EnforceAI Instance;
        
        internal bool IsOnDuty;
        internal string Department = "LSPD";
        internal readonly Dictionary<string, Blip> playerBlips = new Dictionary<string, Blip>();

        private bool hasInitialized;
        
        public EnforceAI()
        {
            Instance = this;
            
            EventHandlers["EnforceAI::client:SetDuty"] += new Action<bool?>(SetDutyStatus);
            EventHandlers["EnforceAI::client:PlayerBlips"] += new Action<ExpandoObject>(PlayerPositionList);
            EventHandlers["onResourceStop"] += new Action<string>((resource) =>
            {
                if(resource != GetCurrentResourceName()) return;

                PropManager.CleanUp();
            });

            Tick += PropManager.DrawLoop;
            
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

            Menu mainMenu = MenuManager.CreateMainMenu();

            Menu sceneMenu = MenuManager.CreateSceneMenu();

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
            
            RegisterCommand("+EnforceAI::client:RotateRights", new Action(() =>
            {
                PropManager.RotateRight = true;
            }), false);
            
            RegisterCommand("-EnforceAI::client:RotateRights", new Action(() =>
            {
                PropManager.RotateRight = false;
            }), false);
            
            RegisterCommand("+EnforceAI::client:RotateLefts", new Action(() =>
            {
                PropManager.RotateLeft = true;
            }), false);
            
            RegisterCommand("-EnforceAI::client:RotateLefts", new Action(() =>
            {
                PropManager.RotateLeft = false;
            }), false);
            
            RegisterCommand("+EnforceAI::client:RotateFaster", new Action(() =>
            {
                PropManager.RotateFaster = true;
            }), false);
            
            RegisterCommand("-EnforceAI::client:RotateFaster", new Action(() =>
            {
                PropManager.RotateFaster = false;
            }), false);
            
            RegisterKeyMapping("+EnforceAI::client:RotateFaster", "Rotate props faster", "keyboard", "LSHIFT");
            
            RegisterKeyMapping("+EnforceAI::client:RotateLefts", "Rotate props left", "keyboard", "PRIOR");
            
            RegisterKeyMapping("+EnforceAI::client:RotateRights", "Rotate props right", "keyboard", "NEXT");
        }

        internal void SetDutyStatus(bool? status = null)
        {
            if (!status.HasValue) status = !IsOnDuty;
            IsOnDuty = status.Value;
            ClientUtilities.Tooltip($"You are now {(IsOnDuty ? "~g~on~s~ duty" : "~r~off~s~ duty")}!");
            MenuManager.DutyCheck.Checked = IsOnDuty;
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