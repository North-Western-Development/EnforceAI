Ped = {}
Ped.__index = Ped

Ped.id = 0

Ped.FirstName = "George"
Ped.MiddleInitial = "L"
Ped.LastName = "Stevens"

Ped.Age = 0

Ped.DateOfBirth = ""

Ped.Licenses = {
    Drivers = {},
    Weapons = {},
    Hunting = {},
    Pilots = {}
}

function Ped:new(database)
    if not IsDuplicityVersion() then error("Ped can only be created on the server side!") end
    local newObject = setmetatable({}, Ped)

    newObject.id = database:GetNext(Datatypes.PED)

    newObject.DateOfBirth = Helpers.GenerateDate(18)
    newObject.Age = Helpers.GetAge(newObject.DateOfBirth)

    newObject.Licenses = {
        Drivers = License:new(LicenseTypes.DRIVERS, newObject.Age),
        Weapons = License:new(LicenseTypes.WEAPONS, newObject.Age),
        Hunting = License:new(LicenseTypes.HUNTING, newObject.Age),
        Pilots = License:new(LicenseTypes.PILOTS, newObject.Age)
    }

    database:AddOrUpdate(Datatypes.PED, newObject.id, newObject)

    TriggerClientEvent("EnforceAI::client:DataUpdate", -1, Datatypes.PED, newObject)

    return newObject
end

function Ped:GetName()
    return self.FirstName .. " " .. self.MiddleInitial .. ". " .. self.LastName
end

function Ped:Update()
    database:AddOrUpdate(Datatypes.PED, self.id, self)
    if IsDuplicityVersion() then
        TriggerClientEvent("EnforceAI::client:DataUpdate", -1, Datatypes.PED, self)
    else
        TriggerServerEvent("EnforceAI::server:DataUpdate", Datatypes.PED, self)
    end
end

function Ped.FromData(data)
    local newObject = setmetatable(data, Ped)

    return newObject
end