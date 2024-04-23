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

RegisterNetEvent("EnforceAI::server::GetPedData", function(data, toCall)
    local source = source
    local ped = ServerDatabase:GetDataForPedNetId(data.netid)

    if ped ~= nil then
        ped = ServerDatabase:Get(Datatypes.PED, ped)
    else
        local sqlData = MySQL.query.await('SELECT * FROM `enforceai_peds` WHERE `head_component` = ? AND `head_texture` = ? AND `ped_model` = ?', {
            data.headcomp,
            data.headtex,
            data.model
        });

        if sqlData then
            for _, v in ipairs(sqlData) do
                if not ServerDatabase:LookupPedData(v.firstname, v.middle, v.lastname, v.dob, v.gender) then
                    ped = v
                end
            end
        end

        if ped ~= nil then
           ped = Ped:FromRaw(ServerDatabase, data.netid, ped.firstname, ped.middle, ped.lastname, ped.gender, ped.dob, ped.licenses)
        else
            ped = Ped:new(ServerDatabase, data.netid, data.gender, data.model, data.headcomp, data.headtex)
        end
    end

    TriggerClientEvent(toCall, source, { ped = ped })
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
