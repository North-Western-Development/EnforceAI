local genderConversions = {
    DatabaseToRuntime = {
        m = 0,
        f = 1,
        x = 2
    },
    RuntimeToDatabase = {
        [0] = 'm',
        [1] = 'f',
        [2] = 'x'
    }
}

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

function Database:GetDataForPedNetId(netid)
    for k, v in pairs(self.Peds) do
        if v.netid == netid then
            return k;
        end
    end

    return nil
end

function Database:LookupPedData(firstname, middleinitial, lastname, dob, gender)
    gender = genderConversions.DatabaseToRuntime[gender]

    for k, v in pairs(self.Peds) do
        if v.FirstName == firstname and v.MiddleInitial == middleinitial and v.LastName == lastname and v.DateOfBirth == dob and v.Gender == gender then
            return v
        end
    end

    return nil
end

function Database:GetPlayerForSource(source)
    for _, v in pairs(self.Players) do
        if v.source == source then
            return v
        end
    end

    return nil
end