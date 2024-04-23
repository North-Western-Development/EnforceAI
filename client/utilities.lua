local menuOpen
local propMenu
local debugMenu

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

NearestLivingPed = function(pos, radius, allowPlayer)
    local peds = GetGamePool("CPed")
    local closest = -1
    local ped = 0

    for _, v in pairs(peds) do
        if DoesEntityExist(v) and IsPedHuman(v) and not IsPedDeadOrDying(v, true) and (allowPlayer or not IsPedAPlayer(v)) then
            local dist = #(pos - GetEntityCoords(v, true))
            if dist < radius and (dist < closest or closest == -1) then
                closest = dist
                ped = v
            end
        end
    end

    return ped
end

CreateBlip = function(name, coords, heading, color, scale, displayType, sprite, shortRange)
    local blip = AddBlipForCoord(coords.x, coords.y, coords.z)
    SetBlipSprite(blip, sprite);
    SetBlipDisplay(blip, displayType);
    SetBlipScale(blip, scale);
    SetBlipColour(blip, color);
    SetBlipRotation(blip, heading);
    SetBlipAsShortRange(blip, shortRange or true);
    BeginTextCommandSetBlipName("STRING");
    AddTextComponentString(name);
    EndTextCommandSetBlipName(blip);

    return blip
end

SetupMenus = function(mainmenu, scenemenu, dispatchmenu)
    -- MAIN MENU --
    local dutyCheck = mainmenu:AddCheckbox({ icon = '💼', label = "Set Duty Status", value = false })
    debugMenu = MenuV:CreateMenu("Debug Menu", "", "topright", 0, 0, 255, 'size-100', 'default', 'menuv', 'enforceaidebug', "native")

    local debugMenuButton = mainmenu:AddButton({ icon = '🐞', label = "Debug Menu", value = debugMenu })

    dutyCheck:On('change', function(_, value)
        if LocalPlayer == nil then
            mainmenu:Close()
            return
        end
        LocalPlayer.DutyStatus = value
        LocalPlayer:Update()
    end)

    -- DEBUG MENU --
    local getPedDataButton = debugMenu:AddButton({ icon = '🪪', label = "Get Nearest Ped Data"})

    getPedDataButton:On('select', function()
        Citizen.CreateThread(function ()
            local pos = GetEntityCoords(PlayerPedId())
            local ped = NearestLivingPed(pos, 5)
            if ped == 0 then
                Tooltip("~y~No ped nearby!~s~")
                return
            end

            local pedData = ClientDatabase:GetDataForPedNetId(NetworkGetNetworkIdFromEntity(ped))

            if pedData ~= nil then
                pedData = ClientDatabase:Get(Datatypes.PED, pedData)
                if pedData ~= nil then
                    Notification("~b~Name:~s~ " .. pedData:GetName() .. "~n~~b~Date of Birth:~s~ " .. pedData.DateOfBirth);
                    return
                end
            end
            pedData = Callbacks.Create("getpeddata", { gender = IsPedMale(ped) and Genders.MALE or Genders.FEMALE, model = GetEntityModel(ped), headcomp = GetPedDrawableVariation(ped, 0), headtex = GetPedTextureVariation(ped, 0), netid = NetworkGetNetworkIdFromEntity(ped) }, "EnforceAI::server::GetPedData", 2000)
            local result = Citizen.Await(pedData)
            if result.ped ~= nil then
                pedData = Ped.FromData(result.ped)
            end

            if pedData ~= nil then
                Notification("~b~Name:~s~ " .. pedData:GetName() .. "~n~~b~Date of Birth:~s~ " .. pedData.DateOfBirth);
            else
                Tooltip("~r~An error occured!~s~")
            end
        end)
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

    RegisterKeyMapping("+rotateright", "Rotate Prop Right", "KEYBOARD", "PRIOR")

    RegisterKeyMapping("+rotateleft", "Rotate Prop Left", "KEYBOARD", "NEXT")

    RegisterKeyMapping("+rotatefaster", "Increase Rotation Speed", "KEYBOARD", "LSHIFT")

    RegisterKeyMapping("+rotatedoublefaster", "Double Rotation Speed", "KEYBOARD", "LMENU")

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

    RegisterCommand("+rotateright", function()
        PropManagerState.RotateRight = true
    end, false)

    RegisterCommand("-rotateright", function()
        PropManagerState.RotateRight = false
    end, false)

    RegisterCommand("+rotateleft", function()
        PropManagerState.RotateLeft = true
    end, false)

    RegisterCommand("-rotateleft", function()
        PropManagerState.RotateLeft = false
    end, false)

    RegisterCommand("+rotatefaster", function()
        PropManagerState.RotateFaster = true
    end, false)

    RegisterCommand("-rotatefaster", function()
        PropManagerState.RotateFaster = false
    end, false)

    RegisterCommand("+rotatedoublefaster", function()
        PropManagerState.DoubleFast = true
    end, false)

    RegisterCommand("-rotatedoublefaster", function()
        PropManagerState.DoubleFast = false
    end, false)
end