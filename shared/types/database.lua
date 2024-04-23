Database = {}
Database.__index = Database

Database.Peds = {}
Database.Vehicles = {}
Database.Props = {}
Database.Players = {}

function Database:new()
    local newObject = setmetatable({}, Database)

    return newObject
end

function Database:GetNext(datatype)
    if not IsDuplicityVersion() and datatype ~= Datatypes.PROP then error("This function is not applicable to clients") end
    if datatype == Datatypes.PED then
        return #self.Peds+1
    elseif datatype == Datatypes.VEHICLE then
        return #self.Vehicles+1
    elseif datatype == Datatypes.PROP then
        return #self.Props+1
    elseif datatype == Datatypes.PLAYER then
        return #self.Players+1
    end
end

function Database:AddOrUpdate(datatype, id, data)
    if datatype == Datatypes.PED then
        self.Peds[id] = data
    elseif datatype == Datatypes.VEHICLE then
        self.Vehicles[id] = data
    elseif datatype == Datatypes.PROP then
        self.Props[id] = data
    elseif datatype == Datatypes.PLAYER then
        self.Players[id] = data
    end
    if data == nil then
        if IsDuplicityVersion() then
            TriggerClientEvent("EnforceAI::client:DataUpdate", -1, datatype, data, id)
        end
    end
end

function Database:Get(datatype, id)
    if datatype == Datatypes.PED then
        return self.Peds[id]
    elseif datatype == Datatypes.VEHICLE then
        return self.Vehicles[id]
    elseif datatype == Datatypes.PROP then
        return self.Props[id]
    elseif datatype == Datatypes.PLAYER then
        return self.Players[id]
    end
end

function Database:GetPropForNetId(netid)
    for k, v in pairs(self.Props) do
        if v.NetworkId == netid then
            return k;
        end
    end

    return nil
end

function Database:GetAllPropsForSource(source)
    local props = {}

    for k, v in pairs(self.Props) do
        if v.Spawner == source then
            props[#props+1] = v
        end
    end

    return props
end