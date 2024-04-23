Player = {}
Player.__index = Player

Player.id = 0
Player.source = 0
Player.Name = ""
Player.DutyStatus = false

function Player:new(database, name, source)
    if not IsDuplicityVersion() then error("Player can only be created on the server side!") end
    local newObject = setmetatable({}, Player)

    newObject.Name = name

    newObject.id = database:GetNext(Datatypes.PLAYER)
    newObject.source = source

    database:AddOrUpdate(Datatypes.PLAYER, newObject.id, newObject)

    TriggerClientEvent("EnforceAI::client:DataUpdate", -1, Datatypes.PLAYER, newObject)

    return newObject
end

function Player:Update()
    if IsDuplicityVersion() then
        ServerDatabase:AddOrUpdate(Datatypes.PLAYER, self.id, self)
    else
        ClientDatabase:AddOrUpdate(Datatypes.PLAYER, self.id, self)
    end
    if IsDuplicityVersion() then
        TriggerClientEvent("EnforceAI::client:DataUpdate", -1, Datatypes.PLAYER, self)
    else
        TriggerServerEvent("EnforceAI::server:DataUpdate", Datatypes.PLAYER, self)
    end
end

function Player.FromData(data)
    local newObject = setmetatable(data, Player)

    return newObject
end