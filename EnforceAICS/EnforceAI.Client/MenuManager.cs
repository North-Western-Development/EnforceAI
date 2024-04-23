using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using EnforceAI.Server.Types;
using MenuAPI;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace EnforceAI.Client;

internal static class MenuManager
{
    internal static MenuCheckboxItem DutyCheck;

    internal static bool MedicalServicesActive = false;
    internal static bool MedicalServicesShouldCancel = false;
    
    private static Vehicle? ambulance;
    private static Ped? paramedic;
    
    internal static Menu CreateMainMenu()
    {
        Menu mainMenu = new Menu("EnforceAI Menu");
        MenuController.AddMenu(mainMenu);

        DutyCheck = new MenuCheckboxItem("Set duty status", "Set your duty status for EnforceAI");
        DutyCheck.LeftIcon = MenuItem.Icon.BRIEFCASE;
        DutyCheck.Checked = EnforceAI.Instance.IsOnDuty;

        mainMenu.AddMenuItem(DutyCheck);

        MenuItem debugMenuItem = new MenuItem("~o~Debug Options~s~", "EnforceAI debug options");
        debugMenuItem.LeftIcon = MenuItem.Icon.WARNING;

        mainMenu.AddMenuItem(debugMenuItem);

        Menu debugMenu = CreateDebugMenu();

        MenuController.AddSubmenu(mainMenu, debugMenu);

        MenuController.BindMenuItem(mainMenu, debugMenu, debugMenuItem);

        mainMenu.OnCheckboxChange += (menu, checkItem, itemIndex, newState) =>
        {
            if (checkItem == DutyCheck)
            {
                EnforceAI.Instance.SetDutyStatus(newState);
            }
        };

        return mainMenu;
    }

    internal static Menu CreateSceneMenu()
    {
        Menu sceneMenu = new Menu("Scene Menu");

        MenuItem sceneProps = new MenuItem("Scene Prop Menu", "Spawn props to control your scene");
        sceneProps.LeftIcon = MenuItem.Icon.INV_PERSON;
        sceneMenu.AddMenuItem(sceneProps);

        MenuItem speedZone = new MenuItem("Speed Zone Menu", "Create speed zones to control traffic");
        speedZone.LeftIcon = MenuItem.Icon.CAR;
        sceneMenu.AddMenuItem(speedZone);

        Menu propMenu = CreatePropMenu();

        MenuController.AddSubmenu(sceneMenu, propMenu);
        MenuController.BindMenuItem(sceneMenu, propMenu, sceneProps);

        Menu speedZoneMenu = CreateSpeedZoneMenu();

        MenuController.AddSubmenu(sceneMenu, speedZoneMenu);
        MenuController.BindMenuItem(sceneMenu, speedZoneMenu, speedZone);

        return sceneMenu;
    }

    internal static Menu CreateDispatchMenu()
    {
        Menu dispatchMenu = new Menu("Dispatch Menu");

        MenuItem requestServiceItem = new MenuItem("Request Service", "Request a service to your location");
        requestServiceItem.LeftIcon = MenuItem.Icon.INFO;
        dispatchMenu.AddMenuItem(requestServiceItem);

        MenuItem radioMenuItem = new MenuItem("Radio Calls", "Access radio calls");
        radioMenuItem.LeftIcon = MenuItem.Icon.INV_PERSON;
        dispatchMenu.AddMenuItem(radioMenuItem);

        MenuItem backupMenuItem = new MenuItem("Request Backup");
        backupMenuItem.Enabled = false;
        backupMenuItem.LeftIcon = MenuItem.Icon.GUN;
        dispatchMenu.AddMenuItem(backupMenuItem);

        Menu serviceRequestMenu = CreateServiceRequestMenu();
        MenuController.AddSubmenu(dispatchMenu, serviceRequestMenu);
        MenuController.BindMenuItem(dispatchMenu, serviceRequestMenu, requestServiceItem);
        
        Menu radioCallsMenu = CreateRadioCallMenu();
        MenuController.AddSubmenu(dispatchMenu, radioCallsMenu);
        MenuController.BindMenuItem(dispatchMenu, radioCallsMenu, radioMenuItem);
        
        Menu backupMenu = CreateBackupMenu();
        MenuController.AddSubmenu(dispatchMenu, backupMenu);
        MenuController.BindMenuItem(dispatchMenu, backupMenu, backupMenuItem);
        
        return dispatchMenu;
    }

    //TODO: ADD ABILITY TO OPEN MENU
    internal static Menu CreateNearbyPedMenu()
    {
        Menu nearbyPedMenu = new Menu("Ped Interaction");

        MenuListItem requestLicenseItem = new MenuListItem("Request License", new List<string>() { "Driver's", "Weapons", "Hunting", "Fishing", "Pilot's" }, 0, "Request the specified license from the ped");
        
        nearbyPedMenu.AddMenuItem(requestLicenseItem);

        return nearbyPedMenu;
    }

    #region Main Menu Sub Menus
    private static Menu CreateDebugMenu()
    {
        Menu debugMenu = new Menu("EnforceAI Debug Menu");

        MenuItem toggleDutyStatus = new MenuItem("Toggle Duty Status", "Toggle your duty status");
        toggleDutyStatus.LeftIcon = MenuItem.Icon.BRIEFCASE;

        debugMenu.AddMenuItem(toggleDutyStatus);

        MenuItem calloutNotification = new MenuItem("Trigger Test Callout Notification", "Triggers a callout notification for testing");
        calloutNotification.LeftIcon = MenuItem.Icon.MISSION_STAR;

        debugMenu.AddMenuItem(calloutNotification);
        
        MenuItem pedDataItem = new MenuItem("Get nearest ped data", "Requests and displays the nearest ped's data from the server");
        pedDataItem.LeftIcon = MenuItem.Icon.INV_DATA;

        debugMenu.AddMenuItem(pedDataItem);

        debugMenu.OnItemSelect += async (menu, selectedItem, itemIndex) =>
        {
            if (selectedItem == toggleDutyStatus)
            {
                EnforceAI.Instance.SetDutyStatus();
            }
            else if (selectedItem == calloutNotification)
            {
                ClientUtilities.Notification("~b~Type:~s~~o~ Officer in Distress (10-99)~s~~n~~g~Location:~s~ ~p~Del Perro Freeway, Los Santos~s~", true, 140, "CHAR_CALL911", true, 0, EnforceAI.Instance.Department + " Dispatch", "Call Received");
            }
            else if (selectedItem == pedDataItem)
            {
                Ped? ped = await ClientUtilities.GetNearestLivingPed(Game.PlayerPed.Position, 25f);
                if (ped == null)
                {
                    ClientUtilities.Tooltip("~r~No ped close enough!~s~");
                    return;
                }
                
                Task<string?> callbackRegistration = EnforceAI.RegisterAwaitableCallback("ReturnPedData:" + ped.NetworkId, 5000);

                int pedId = NetworkGetEntityFromNetworkId(ped.NetworkId);
                int headComponentNumber = GetPedDrawableVariation(pedId, 0);
                int headTextureNumber = GetPedTextureVariation(pedId, 0);
                
                BaseScript.TriggerServerEvent("EnforceAI::server:GetPedData", ped.NetworkId, ped.Gender switch
                {
                    Gender.Male => Common.Enums.Gender.Male,
                    Gender.Female => Common.Enums.Gender.Female,
                    _ => Common.Enums.Gender.Other
                }, headComponentNumber, headTextureNumber);
                string? pedDataResult = await callbackRegistration;

                if (pedDataResult == null)
                {
                    ClientUtilities.Tooltip("~r~A timeout occurred resulting in a timeout!~s~");
                    return;
                }
                
                try
                {
                    PedData data = JsonConvert.DeserializeObject<PedData>(pedDataResult);
                    
                    if (data == null)
                    {
                        ClientUtilities.Notification("~r~" + pedDataResult + "~s~");
                        return;
                    }
                    
                    ClientUtilities.Notification("~b~Name:~s~ " + data.GetName() + "~n~" + "Date of Birth:~s~ " + data.GetDateOfBirthString());
                }
                catch
                {
                    ClientUtilities.Notification("~r~" + pedDataResult + "~s~");
                }
            }
        };

        return debugMenu;
    }
    #endregion

    #region Scene Menu Sub Menus
    private static Menu CreatePropMenu()
    {
        Menu propMenu = new Menu("Scene Prop Menu");
        
        MenuListItem propList = new MenuListItem("Prop", PropManager.Props, 0, "Select prop to spawn");
        propMenu.AddMenuItem(propList);

        MenuCheckboxItem freezeSpawnedProp = new MenuCheckboxItem("Spawn Prop Frozen", "If checked spawned prop will be frozen");
        propMenu.AddMenuItem(freezeSpawnedProp);

        MenuItem deleteNearestProp = new MenuItem("~y~Delete Nearest Prop~s~", "Deletes the prop closest to you regardless of who spawned it");
        deleteNearestProp.LeftIcon = MenuItem.Icon.WARNING;
        propMenu.AddMenuItem(deleteNearestProp);

        MenuItem deleteAllProps = new MenuItem("~r~Delete All Props~s~", "Deletes all props you've spawned");
        deleteAllProps.LeftIcon = MenuItem.Icon.WARNING;
        propMenu.AddMenuItem(deleteAllProps);

        propMenu.OnIndexChange += (menu, item, newItem, index, newIndex) =>
        {
            if (newItem == propList)
            {
                PropManager.DrawGhost = true;
                PropManager.SpawnModel = propList.GetCurrentSelection();
            }
            else
            {
                PropManager.DrawGhost = false;
                PropManager.SpawnModel = "";
            }
        };

        propMenu.OnMenuOpen += menu =>
        {
            if(menu.GetCurrentMenuItem() != propList) return;
            PropManager.DrawGhost = true;
            PropManager.SpawnModel = propList.GetCurrentSelection();
        };
        
        propMenu.OnMenuClose += menu =>
        {
            PropManager.DrawGhost = false;
            PropManager.SpawnModel = "";
        };

        propMenu.OnListItemSelect += (menu, item, index, itemIndex) =>
        {
            if (item == propList)
            {
                PropManager.FinalizePlacement();
            }
        };

        propMenu.OnCheckboxChange += (menu, item, index, state) =>
        {
            if (item == freezeSpawnedProp)
            {
                PropManager.SpawnFrozen = state;
            }
        };

        propMenu.OnListIndexChange += (menu, item, index, selectionIndex, itemIndex) =>
        {
            if (item == propList)
            {
                PropManager.SpawnModel = item.GetCurrentSelection();
            }
        };

        propMenu.OnItemSelect += (menu, item, index) =>
        {
            if (item == deleteNearestProp)
            {
                PropManager.DeleteNearestProp();
            }
            else if (item == deleteAllProps)
            {
                PropManager.DeleteAllProps();
            }
        };
        
        return propMenu;
    }

    private static Menu CreateSpeedZoneMenu()
    {
        Menu speedZoneMenu = new Menu("Speed Zone Menu");

        MenuListItem sizeList = new MenuListItem("Speed Zone Size", new List<string>()
        {
            "5",
            "10",
            "15",
            "20"
        }, 0, "Select radius of speed zone in GTA units");
        speedZoneMenu.AddMenuItem(sizeList);

        MenuListItem speedList = new MenuListItem("Max Speed", new List<string>()
        {
            "0",
            "5",
            "10",
            "15",
            "20",
            "25",
            "30",
            "35",
            "40",
            "45",
            "50"
        }, 0, "Set the max speed for the zone");
        speedZoneMenu.AddMenuItem(speedList);
        
        MenuItem createZone = new MenuItem("Create Speed Zone", "Create a speed zone with the specified max speed and size");
        speedZoneMenu.AddMenuItem(createZone);

        speedZoneMenu.OnItemSelect += (menu, item, index) =>
        {
            if (item == createZone)
            {
                //BaseScript.TriggerServerEvent("EnforceAI::server:CreateSpeedZone", speedList.GetCurrentSelection(), sizeList.GetCurrentSelection());
            }
        };
        
        return speedZoneMenu;
    }
    #endregion

    #region Dispatch Menu Sub Menus
    internal static Menu CreateServiceRequestMenu()
    {
        Menu serviceRequestMenu = new Menu("Request Service");

        MenuItem callAmbulance = new MenuItem("Request Medical Services", "Request a paramedic to your location");
        serviceRequestMenu.AddMenuItem(callAmbulance);

        serviceRequestMenu.OnItemSelect += async (menu, item, index) =>
        {
            if (item == callAmbulance)
            {
                if (!MedicalServicesActive)
                {
                    MedicalServicesActive = true;
                    item.Text = "~r~Cancel Medical Services~s~";
                    item.Description = "Cancel your paramedic request";
                    
                    int saved = 0;
                    int notSaved = 0;

                    Ped? closestDownedOrInjuredPed = await ClientUtilities.GetNearestDownedOrInjuredPed(Game.PlayerPed.Position, 25f);

                    if (closestDownedOrInjuredPed == null)
                    {
                        ClientUtilities.Tooltip("~r~Failed to find ped nearby.~s~");
                        return;
                    }

                    Vector3 randomLocation = FindRandomPointInSpace(PlayerPedId());
                    Vector3 spawnLocation = Vector3.Zero;
                    float heading = -4443.44f;
                    bool found = GetClosestVehicleNodeWithHeading(randomLocation.X, randomLocation.Y, randomLocation.Z, ref spawnLocation, ref heading, 1, 3.0f, 0);

                    if (!found)
                    {
                        ClientUtilities.Tooltip("~r~Medical services failed to respond and has been canceled, please try again!~s~");
                        return;
                    }

                    ;

                    ambulance = await World.CreateVehicle(new Model("ambulance"), spawnLocation, heading);
                    paramedic = await World.CreatePed(new Model("s_m_m_paramedic_01"), spawnLocation, heading);
                    paramedic.SetIntoVehicle(ambulance, VehicleSeat.Driver);
                    paramedic.BlockPermanentEvents = true;
                    Vector3 roadLocation = Vector3.Zero;
                    found = GetClosestVehicleNode(closestDownedOrInjuredPed.Position.X, closestDownedOrInjuredPed.Position.Y, closestDownedOrInjuredPed.Position.Z, ref roadLocation, 1, 3.0f, 0);
                    if (!found)
                    {
                        ClientUtilities.Tooltip("~r~Medical services failed to respond and has been canceled, please try again!~s~");
                        ambulance.Delete();
                        paramedic.Delete();
                        return;
                    }

                    if (MedicalServicesShouldCancel)
                    {
                        MedicalServicesActive = false;
                        MedicalServicesShouldCancel = false;
                        return;
                    }
                    
                    Vector3 target = Vector3.Zero;
                    found = GetPointOnRoadSide(roadLocation.X, roadLocation.Y, roadLocation.Z, 3, ref target);
                    if (!found)
                    {
                        ClientUtilities.Tooltip("~r~Medical services failed to respond and has been canceled, please try again!~s~");
                        ambulance.Delete();
                        paramedic.Delete();
                        return;
                    }
                    
                    if (MedicalServicesShouldCancel)
                    {
                        MedicalServicesActive = false;
                        MedicalServicesShouldCancel = false;
                        return;
                    }

                    paramedic.Task.DriveTo(ambulance, target, 15f, 50f * 2.236936f, 787326);
                    ambulance.IsSirenActive = true;
                    ClientUtilities.Tooltip("~g~Medical services are on the way!~s~");
                    int timeoutTimer = 0;
                    bool timedOut = false;

                    while (ambulance.Position.DistanceToSquared(target) > 250f)
                    {
                        await BaseScript.Delay(250);
                        timeoutTimer += 250;
                        if (timeoutTimer > 90000)
                        {
                            timedOut = true;
                            ClientUtilities.Tooltip("~r~Medical services failed to respond and has been canceled, please try again!~s~");
                            break;
                        }
                        if (MedicalServicesShouldCancel)
                        {
                            MedicalServicesActive = false;
                            MedicalServicesShouldCancel = false;
                            return;
                        }
                    }

                    if (timedOut)
                    {
                        ambulance.Delete();
                        paramedic.Delete();
                        return;
                    }

                    await BaseScript.Delay(1000);
                    paramedic.Task.ClearAll();
                    ambulance.IsSirenActive = false;
                    paramedic.Task.LeaveVehicle();
                    paramedic.Task.GoTo(closestDownedOrInjuredPed);

                    while (paramedic.Position.DistanceToSquared(closestDownedOrInjuredPed.Position) > 2.4f)
                    {
                        await BaseScript.Delay(15);
                        if (MedicalServicesShouldCancel)
                        {
                            MedicalServicesActive = false;
                            MedicalServicesShouldCancel = false;
                            return;
                        }
                    }

                    paramedic.Task.ClearAll();
                    paramedic.Task.TurnTo(closestDownedOrInjuredPed);
                    await BaseScript.Delay(400);
                    paramedic.Task.StartScenario("CODE_HUMAN_MEDIC_TEND_TO_DEAD", paramedic.Position);
                    Task<List<Ped>> search = ClientUtilities.GetAllNearbyDownedOrInjuredPed(paramedic.Position, 25f, true, closestDownedOrInjuredPed.Handle);
                    await BaseScript.Delay(10000);
                    if (new Random().Next(0, 10) < 5)
                    {
                        await World.CreatePed(closestDownedOrInjuredPed.Model, closestDownedOrInjuredPed.Position, closestDownedOrInjuredPed.Heading);
                        closestDownedOrInjuredPed.Delete();
                        paramedic.Task.ClearAll();
                        saved++;
                    }
                    else
                    {
                        paramedic.Task.ClearAll();
                        notSaved++;
                    }

                    List<Ped> peds = await search;
                    
                    if (MedicalServicesShouldCancel)
                    {
                        MedicalServicesActive = false;
                        MedicalServicesShouldCancel = false;
                        return;
                    }

                    if (peds.Count == 0)
                    {
                        if (saved == 0)
                        {
                            ClientUtilities.Tooltip("~r~Failed to revive ped.~s~");
                        }
                        else
                        {
                            ClientUtilities.Tooltip("~g~Successfully revived ped.~s~");
                        }

                        paramedic.Task.EnterVehicle(ambulance, VehicleSeat.Driver, 10, 0.5f, 1);
                        paramedic.Task.CruiseWithVehicle(ambulance, 30f * 2.236936f, 508);
                        await BaseScript.Delay(5000);
                        paramedic.MarkAsNoLongerNeeded();
                        ambulance.MarkAsNoLongerNeeded();
                        return;
                    }

                    foreach (Ped downedPed in peds)
                    {
                        paramedic.Task.GoTo(downedPed);

                        while (paramedic.Position.DistanceToSquared(downedPed.Position) > 2.4f)
                        {
                            await BaseScript.Delay(15);
                        }

                        paramedic.Task.ClearAll();
                        paramedic.Task.TurnTo(closestDownedOrInjuredPed);
                        paramedic.Task.StartScenario("CODE_HUMAN_MEDIC_TEND_TO_DEAD", paramedic.Position);
                        await BaseScript.Delay(10000);
                        if (downedPed.IsPlayer)
                        {
                            //TODO: ASK SERVER TO HEAL PLAYER
                        }
                        else if (new Random().Next(0, 10) < 5)
                        {
                            await World.CreatePed(closestDownedOrInjuredPed.Model, closestDownedOrInjuredPed.Position, closestDownedOrInjuredPed.Heading);
                            downedPed.Delete();
                            paramedic.Task.ClearAll();
                            saved++;
                        }
                        else
                        {
                            paramedic.Task.ClearAll();
                            notSaved++;
                        }
                        
                        if (MedicalServicesShouldCancel)
                        {
                            MedicalServicesActive = false;
                            MedicalServicesShouldCancel = false;
                            return;
                        }
                    }

                    ClientUtilities.Notification($"~g~Saved:~s~ {saved}~n~~r~Deceased:~s~ {notSaved}", true, 140, "CHAR_MEDIC", false, 0, "San Andreas Medical Service", "After Action Report");
                    paramedic.Task.EnterVehicle(ambulance, VehicleSeat.Driver, 10, 1f, 1);
                    paramedic.Task.CruiseWithVehicle(ambulance, 30f * 2.236936f, 511);
                    await BaseScript.Delay(5000);
                    paramedic.MarkAsNoLongerNeeded();
                    ambulance.MarkAsNoLongerNeeded();
                }
                else
                {
                    MedicalServicesShouldCancel = true;
                    ClientUtilities.Tooltip("~r~Medical Services request canceled.~s~");
                    
                    if (paramedic != null && ambulance != null)
                    {
                        paramedic.BlockPermanentEvents = false;
                        paramedic.Task.CruiseWithVehicle(ambulance, 30f * 2.236936f, 511);
                        ambulance.IsSirenActive = false;
                    }
                    
                    if (paramedic != null) paramedic.MarkAsNoLongerNeeded();
                    if (ambulance != null) ambulance.MarkAsNoLongerNeeded();
                    
                    item.Text = "Request Medical Services";
                    item.Description = "Request a paramedic to your location";
                }
            }
        }; 
        
        return serviceRequestMenu;
    }
    
    internal static Menu CreateRadioCallMenu()
    {
        Menu radioCallMenu = new Menu("Radio Calls");

        return radioCallMenu;
    }
    
    internal static Menu CreateBackupMenu()
    {
        Menu backupMenu = new Menu("Request Backup");

        return backupMenu;
    }
    #endregion
}