local QBCore = exports["qb-core"]:GetCoreObject()
ServerDatabase = Database:new()

RegisterNetEvent("EnforceAI::server::login", function(data, toCall)
    local playerData = QBCore.Functions.GetPlayer(source).PlayerData
    local player = Player:new(ServerDatabase, playerData.charinfo.firstname .. " " .. playerData.charinfo.lastname)
    TriggerClientEvent(toCall, source, { player = player })
end)

RegisterNetEvent("EnforceAI::server::logout", function(data, toCall)
    ServerDatabase:AddOrUpdate(Datatypes.PLAYER, data.id, nil)
    TriggerClientEvent(toCall, source)
end)

RegisterNetEvent("EnforceAI::server::DeleteEntity", function(netid)
    DeleteEntity(NetworkGetEntityFromNetworkId(netid))
    local databaseId = ServerDatabase:GetPropForNetId(netid)
    if databaseId then
        ServerDatabase:AddOrUpdate(Datatypes.PROP, databaseId, nil)
    end
end)

RegisterNetEvent("EnforceAI::server::DeleteAllForPlayer", function()
    local props = ServerDatabase:GetAllPropsForSource(source)
    for _, v in pairs(props) do
        DeleteEntity(NetworkGetEntityFromNetworkId(v.NetworkId))
        ServerDatabase:AddOrUpdate(Datatypes.PROP, v.id, nil)
    end
end)

AddEventHandler("onResourceStop", function (resource)
    if resource ~= GetCurrentResourceName() then return end

    for _, v in pairs(ServerDatabase.Props) do
        DeleteEntity(NetworkGetEntityFromNetworkId(v.NetworkId))
    end
end)
