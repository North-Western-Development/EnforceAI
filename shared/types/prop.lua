Prop = {}
Prop.__index = Prop

Prop.id = 0
Prop.NetworkId = 0
Prop.Spawner = 0

function Prop:new(database, netid)
    if IsDuplicityVersion() then error("Prop can only be created on the client side!") end
    local newObject = setmetatable({}, Prop)

    newObject.NetworkId = netid
    newObject.Spawner = GetPlayerServerId(PlayerId())

    newObject.id = database:GetNext(Datatypes.PROP)

    database:AddOrUpdate(Datatypes.PROP, newObject.id, newObject)

    TriggerServerEvent("EnforceAI::server:DataUpdate", Datatypes.PROP, newObject)

    return newObject
end

function Prop.FromData(data)
    local newObject = setmetatable(data, Prop)

    return newObject
end