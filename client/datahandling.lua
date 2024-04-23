RegisterNetEvent("EnforceAI::client:DataUpdate", function (datatype, data, id)
    if data == nil then
        ClientDatabase:AddOrUpdate(datatype, id, nil)
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
    ClientDatabase:AddOrUpdate(datatype, data.id, data)
end)