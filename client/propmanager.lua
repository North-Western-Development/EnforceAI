PropManagerState = {
    DrawGhost = false,
    SpawnFrozen = false,
    SpawnModel = Configs.Props[1],
    RotateLeft = false,
    RotateRight = false,
    RotateFaster = false,
    DoubleFast = false
}

local propSpawned = false
local currentPropSpawned = ""
local currentPropHandle = nil
local currentYaw = 0


Citizen.CreateThread(function ()
    -- TODO: Optimize this to not run all the time
    local sleep = 250
    while true do
        if PropManagerState.DrawGhost then
            sleep = 25
            local pos = GetEntityCoords(PlayerPedId(), true)
            local forward = GetEntityForwardVector(PlayerPedId())
            if not propSpawned or currentPropSpawned ~= PropManagerState.SpawnModel then
                if propSpawned and currentPropHandle then
                    DeleteObject(currentPropHandle)
                    currentPropHandle = 0
                    propSpawned = false
                end

                currentPropSpawned = PropManagerState.SpawnModel
                RequestModel(currentPropSpawned)
                while not HasModelLoaded(currentPropSpawned) do
                    Wait(2)
                end

                currentPropHandle = CreateObject(GetHashKey(currentPropSpawned), pos.x + (forward.x * 1.5), pos.y + (forward.y * 1.5), pos.z, true, false, false)
                PlaceObjectOnGroundProperly(currentPropHandle);
                SetEntityCollision(currentPropHandle, false, false);
                SetEntityAlpha(currentPropHandle, 102, false);
                propSpawned = true
            end
            if currentPropHandle then
                SetEntityCoords(currentPropHandle, pos.x + (forward.x * 1.5), pos.y + (forward.y * 1.5), pos.z, false, false, false, false)
                PlaceObjectOnGroundProperly(currentPropHandle)
                local rot = GetEntityRotation(currentPropHandle, 2);
                if (PropManagerState.RotateLeft and PropManagerState.RotateRight) or (not PropManagerState.RotateLeft and not PropManagerState.RotateRight) then
                    SetEntityRotation(currentPropHandle, rot.x, rot.y, currentYaw, 2, false);
                else
                    local multiplier = 1
                    if PropManagerState.RotateFaster then
                        multiplier = 5
                    end
                    if PropManagerState.DoubleFast then
                        multiplier = multiplier * 2
                    end

                    if PropManagerState.RotateRight then
                        if currentYaw + (0.2 * multiplier) > 360 then
                            currentYaw = 0;
                        end
                        currentYaw = currentYaw + (0.2 * multiplier);
                        SetEntityRotation(currentPropHandle, rot.x, rot.y, currentYaw, 2, false);
                    elseif PropManagerState.RotateLeft then
                        if currentYaw - (0.2 * multiplier) < 0 then
                            currentYaw = 360;
                        end
                        currentYaw = currentYaw - (0.2 * multiplier);
                        SetEntityRotation(currentPropHandle, rot.x, rot.y, currentYaw, 2, false);
                    end
                end
            end
        elseif propSpawned and currentPropHandle then
            sleep = 250
            DeleteObject(currentPropHandle)
            currentPropHandle = 0
            propSpawned = false
        else
            sleep = 250
        end

        Wait(sleep)
    end
end)

FinalizePropPlacement = function()
    local pos = GetEntityCoords(PlayerPedId(), true)
    local forward = GetEntityForwardVector(PlayerPedId())
    if currentPropHandle then
        SetEntityCoords(currentPropHandle, pos.x + (forward.x * 1.5), pos.y + (forward.y * 1.5), pos.z, false, false, false, false)
        PlaceObjectOnGroundProperly(currentPropHandle)
        if PropManagerState.SpawnFrozen then
            FreezeEntityPosition(currentPropHandle, true)
        end
        SetEntityCollision(currentPropHandle, true, true);
        SetEntityAlpha(currentPropHandle, 255, false);
        propSpawned = false
        Prop:new(ClientDatabase, NetworkGetNetworkIdFromEntity(currentPropHandle))
        currentPropHandle = 0
    end
end

DoDeleteNearestProp = function()
    local pos = GetEntityCoords(PlayerPedId(), true)
    local closestDistance = -1;
    local closestHandle = 0;
    for _, v in pairs(Configs.Props) do
        temp = GetClosestObjectOfType(pos.x, pos.y, pos.z, 1.0, GetHashKey(v), false, true, true);
        local objDist = GetEntityCoords(temp, false);
        if temp ~= 0 then
            local distance = GetDistanceBetweenCoords(pos.x, pos.y, pos.z, objDist.x, objDist.y, objDist.z, true)
            if closestDistance == -1 then
                closestDistance = distance
                closestHandle = temp
            elseif closestDistance > distance then
                closestDistance = distance
                closestHandle = temp
            end
        end
    end

    if closestDistance == -1 then return end

    TriggerServerEvent("EnforceAI::server::DeleteEntity", NetworkGetNetworkIdFromEntity(closestHandle))
end

CleanUpPropManager = function ()
    if currentPropHandle then
        DeleteObject(currentPropHandle)
        currentPropHandle = 0
        propSpawned = false
    end
end