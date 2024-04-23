RegisterNetEvent("EnforceAI::server:DataUpdate", function (datatype, data, id)
    if data == nil then
        ServerDatabase:AddOrUpdate(datatype, id, data)
        TriggerClientEvent("EnforceAI::client:DataUpdate", -1, datatype, data, id)
        return
    end
    if datatype == Datatypes.PED then
        data = Ped.FromData(data)
    elseif datatype == Datatypes.VEHICLE then
        return nil -- NOT IMPLEMENTED
    elseif datatype == Datatypes.PROP then
        data = Prop.FromData(data)
    elseif datatype == Datatypes.PLAYER then
        data = Player.FromData(data)
    end
    ServerDatabase:AddOrUpdate(datatype, data.id, data)
    TriggerClientEvent("EnforceAI::client:DataUpdate", -1, datatype, data)
end)