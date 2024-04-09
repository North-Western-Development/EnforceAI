using System.Collections.Generic;
using MenuAPI;

namespace EnforceAI.Client;

internal static class MenuManager
{
    internal static MenuCheckboxItem DutyCheck;
    
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

        debugMenu.OnItemSelect += (menu, selectedItem, itemIndex) =>
        {
            if (selectedItem == toggleDutyStatus)
            {
                EnforceAI.Instance.SetDutyStatus();
            }
            else if (selectedItem == calloutNotification)
            {
                ClientUtilities.Notification("~b~Type:~s~~o~ Officer in Distress (10-99)~s~~n~~g~Location:~s~ ~p~Del Perro Freeway, Los Santos~s~", true, 140, "CHAR_CALL911", true, 0, EnforceAI.Instance.Department + " Dispatch", "Call Received");
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

        return speedZoneMenu;
    }

    #endregion
}