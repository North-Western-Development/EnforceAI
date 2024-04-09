using System.Collections.Generic;
using MenuAPI;

namespace EnforceAI.Client;

internal static class MenuManager
{
    internal static Menu CreateMainMenu(EnforceAI instance)
    {
        Menu mainMenu = new Menu("EnforceAI Menu");
            MenuController.AddMenu(mainMenu);
            
            MenuCheckboxItem dutyCheck = new MenuCheckboxItem("Set duty status", "Set your duty status for EnforceAI");
            dutyCheck.LeftIcon = MenuItem.Icon.BRIEFCASE;
            dutyCheck.Checked = instance.IsOnDuty;

            mainMenu.AddMenuItem(dutyCheck);

            MenuItem debugMenuItem = new MenuItem("~o~Debug Options~s~", "EnforceAI debug options");
            debugMenuItem.LeftIcon = MenuItem.Icon.WARNING;
            
            mainMenu.AddMenuItem(debugMenuItem);

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
                    instance.SetDutyStatus();
                } else if (selectedItem == calloutNotification)
                {
                    ClientUtilities.Notification("~b~Type:~s~~o~ Officer in Distress (10-99)~s~~n~~g~Location:~s~ ~p~Del Perro Freeway, Los Santos~s~", true, 140, "CHAR_CALL911", true, 0, instance.Department + " Dispatch", "Call Received");
                }
            };
            
            MenuController.AddSubmenu(mainMenu, debugMenu);
            
            MenuController.BindMenuItem(mainMenu, debugMenu, debugMenuItem);

            mainMenu.OnCheckboxChange += (menu, checkItem, itemIndex, newState) =>
            {
                if(checkItem == dutyCheck)
                {
                    instance.SetDutyStatus(newState);
                }
            };

            return mainMenu;
    }

    internal static Menu CreateSceneMenu(EnforceAI instance)
    {
        Menu sceneMenu = new Menu("Scene Menu");

        MenuItem sceneProps = new MenuItem("Scene Prop Menu", "Spawn props to control your scene");
        sceneProps.LeftIcon = MenuItem.Icon.INV_PERSON;
        sceneMenu.AddMenuItem(sceneProps);

        MenuItem speedZone = new MenuItem("Speed Zone Menu", "Create speed zones to control traffic");
        speedZone.LeftIcon = MenuItem.Icon.CAR;
        sceneMenu.AddMenuItem(speedZone);

        Menu propMenu = new Menu("Scene Prop Menu");
            
        List<string> props = new List<string>()
        {
            "prop_roadcone01a",
            "prop_consign_02a",
            "prop_mp_cone_04"
        };
        MenuListItem propList = new MenuListItem("Prop", props, 0, "Select prop to spawn");
        propMenu.AddMenuItem(propList);

        MenuCheckboxItem freezeSpawnedProp = new MenuCheckboxItem("Spawn Prop Frozen", "If checked spawned prop will be frozen");
        propMenu.AddMenuItem(freezeSpawnedProp);

        MenuItem deleteNearestProp = new MenuItem("~y~Delete Nearest Prop~s~", "Deletes the prop closest to you regardless of who spawned it");
        deleteNearestProp.LeftIcon = MenuItem.Icon.WARNING;
        propMenu.AddMenuItem(deleteNearestProp);
        MenuItem deleteAllProps = new MenuItem("~r~Delete All Props~s~", "Deletes all props you've spawned");
        deleteAllProps.LeftIcon = MenuItem.Icon.WARNING;
        propMenu.AddMenuItem(deleteAllProps);
            
        MenuController.AddSubmenu(sceneMenu, propMenu);
        MenuController.BindMenuItem(sceneMenu, propMenu, sceneProps);
            
        Menu speedZoneMenu = new Menu("Speed Zone Menu");
            
        MenuController.AddSubmenu(sceneMenu, speedZoneMenu);
        MenuController.BindMenuItem(sceneMenu, speedZoneMenu, speedZone);

        return sceneMenu;
    }

}