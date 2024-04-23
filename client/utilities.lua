local menuOpen
local propMenu

Notification = function(notification, adv, color, textureDict, flash, iconType, sender, subject, saveToBrief)
    color = color or 140
    flash = falsh or false
    BeginTextCommandThefeedPost("STRING")
    AddTextComponentSubstringPlayerName(notification)
    ThefeedNextPostBackgroundColor(color)
    if adv then EndTextCommandThefeedPostMessagetext(textureDict, textureDict, flash, iconType, sender, subject) end
    EndTextCommandThefeedPostTicker(false, saveToBrief)
end

Tooltip = function(tooltip)
    SetTextComponentFormat("STRING");
    AddTextComponentString(tooltip);
    DisplayHelpTextFromStringLabel(0, false, true, -1);
end

SetupMenus = function(mainmenu, scenemenu, dispatchmenu)
    -- MAIN MENU --

    local dutyCheck = mainmenu:AddCheckbox({ icon = '💼', label = "Set Duty Status", value = false })
    dutyCheck:On('change', function(_, value)
        if LocalPlayer == nil then
            mainmenu:Close()
            return
        end
        LocalPlayer.DutyStatus = value
        LocalPlayer:Update()
    end)

    -- SCENE MENU --
    propMenu = MenuV:CreateMenu("Scene Prop Menu", "", "topright", 0, 0, 255, 'size-100', 'default', 'menuv', 'enforceaiprop', "native")
    local PropMenuButton = scenemenu:AddButton({ icon = '', label = "Scene Prop Menu", value = propMenu })

    local props = {}

    for i, v in ipairs(Configs.Props) do
        props[i] = {
            label = v,
            value = v,
            description = ''
        }
    end

    local PropList = propMenu:AddSlider({ icon = '', label = "Scene Prop Menu", value = Configs.Props[1], values = props })
    local FreezeProp = propMenu:AddCheckbox({ icon = '❄️', label = "Spawn Prop Frozen", value = false })
    local DeleteNearestProp = propMenu:AddButton({ icon = '⚠️', label = "Delete Nearest Prop"})
    local DeleteAllPropsButton = propMenu:AddButton({ icon = '⚡', label = "Delete All Props" })

    DeleteNearestProp:On('select', function()
        DoDeleteNearestProp()
    end)

    DeleteAllPropsButton:On('select', function()
        TriggerServerEvent("EnforceAI::server::DeleteAllForPlayer")
    end)

    FreezeProp:On('change', function(_, value)
        PropManagerState.SpawnFrozen = value
    end)

    PropList:On('change', function(_, newvalue)
        PropManagerState.SpawnModel = props[newvalue].value
    end)

    PropList:On('select', function()
        FinalizePropPlacement()
    end)

    mainmenu:On('open', function()
        menuOpen = true
    end)

    mainmenu:On('close', function()
        menuOpen = false
    end)

    scenemenu:On('open', function()
        menuOpen = true
    end)

    scenemenu:On('close', function()
        menuOpen = false
    end)

    propMenu:On('open', function()
        PropManagerState.DrawGhost = true
    end)

    propMenu:On('close', function()
        PropManagerState.DrawGhost = false
    end)

    propMenu:On('switch', function(_, currentitem)
        if currentitem == PropList then
            PropManagerState.DrawGhost = true
        else
            PropManagerState.DrawGhost = false
        end
    end)
end

RegisterKeyBinds = function(mainmenu, scenemenu, dispatchmenu)
    RegisterKeyMapping("enforceaimenu", "Main EnforceAI Menu", "KEYBOARD", "END")

    RegisterKeyMapping("enforceaiscenemenu", "EnforceAI Scene Menu", "KEYBOARD", "HOME")

    RegisterKeyMapping("enforceaidispatchmenu", "EnforceAI Dispatch Menu", "KEYBOARD", "G")

    RegisterCommand("enforceaimenu", function()
        if LocalPlayer == nil then return end
        if menuOpen then
            MenuV:CloseAll()
            menuOpen = false
        else
            mainmenu:Open()
        end
    end, false)

    RegisterCommand("enforceaiscenemenu", function()
        if LocalPlayer == nil then return end
        if LocalPlayer.DutyStatus ~= true then
            Tooltip("~r~You must be on duty to use this menu!~s~")
            return
        end
        if menuOpen then
            MenuV:CloseAll()
            menuOpen = false
        else
            scenemenu:Open()
        end
    end, false)

    RegisterCommand("enforceaidispatchmenu", function()
        if LocalPlayer == nil then return end
        if LocalPlayer.DutyStatus ~= true then
            Tooltip("~r~You must be on duty to use this menu!~s~")
            return
        end
        if menuOpen then
            MenuV:CloseAll()
            menuOpen = false
        else
            dispatchmenu:Open()
        end
    end, false)
end