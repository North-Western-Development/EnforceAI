local blips = {}

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

RegisterNetEvent("EnforceAI::client::PlayerBlips", function(blipdata)
    if LocalPlayer == nil or LocalPlayer.DutyStatus == false then return end
    for k, v in pairs(blipdata) do
        local blip
        if blips[k] == nil then
            blip = CreateBlip(v.name, v.coords, v.rotation, 38, 0.5, 2, 399, false)
        else
            blip = blips[k]
            SetBlipCoords(blip, v.coords.x, v.coords.y, v.coords.z)
            SetBlipRotation(blip, v.rotation)
        end
    end
end)