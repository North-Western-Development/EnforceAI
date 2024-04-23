local names
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

Citizen.CreateThread(function()
    if IsDuplicityVersion() then
        names = json.decode(LoadResourceFile(GetCurrentResourceName(), "configs/names.json"))
        names.firstNames.combined = {}

        for k, v in ipairs(names.firstNames.neutral) do
            names.firstNames.female[#names.firstNames.female+1] = v
            names.firstNames.male[#names.firstNames.male+1] = v
            names.firstNames.combined[#names.firstNames.combined+1] = v
        end

        for k, v in ipairs(names.firstNames.male) do
            names.firstNames.combined[#names.firstNames.combined+1] = v
        end

        for k, v in ipairs(names.firstNames.female) do
            names.firstNames.combined[#names.firstNames.combined+1] = v
        end
    end
end)

local middleInitial = {
    "A",
    "B",
    "C",
    "D",
    "E",
    "F",
    "G",
    "H",
    "I",
    "J",
    "K",
    "L",
    "M",
    "N",
    "O",
    "P",
    "Q",
    "R",
    "S",
    "T",
    "U",
    "V",
    "W",
    "X",
    "Y",
    "Z"
}

Ped = {}
Ped.__index = Ped

Ped.id = 0
Ped.netid = 0

Ped.FirstName = "George"
Ped.MiddleInitial = "L"
Ped.LastName = "Stevens"

Ped.Gender = 0

Ped.Age = 0

Ped.DateOfBirth = ""

Ped.Licenses = {
    Drivers = {},
    Weapons = {},
    Hunting = {},
    Pilots = {}
}

function Ped:new(database, netid, gender, model, headcomp, headtex)
    if not IsDuplicityVersion() then error("Ped can only be created on the server side!") end
    local newObject = setmetatable({}, Ped)

    newObject.id = database:GetNext(Datatypes.PED)
    newObject.netid = netid

    if not Configs.AllowTransgender then
        newObject.Gender = gender
    else
        local biasedRollTable = {}

        for k,v in pairs(Genders) do
            local count = 1

            if v == gender then
                count = 30
            end

            if v == 2 and not Configs.AllowOtherGender then
                count = 0
            end

            for i=0, count do
                biasedRollTable[#biasedRollTable+1] = v
            end
        end

        newObject.Gender = biasedRollTable[math.random(1, #biasedRollTable)]
    end

    if newObject.Gender == Genders.MALE then
        newObject.FirstName = names.firstNames.male[math.random(1, #names.firstNames.male)]
    elseif newObject.Gender == Genders.MALE then
        newObject.FirstName = names.firstNames.female[math.random(1, #names.firstNames.female)]
    else
        newObject.FirstName = names.firstNames.combined[math.random(1, #names.firstNames.combined)]
    end

    newObject.MiddleInitial = middleInitial[math.random(1, 26)]
    newObject.LastName = names.lastNames[math.random(1, #names.lastNames)]

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

    MySQL.insert('INSERT INTO `enforceai_peds` (ped_model, gender, firstname, middle, lastname, dob, licenses, head_component, head_texture) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)', {
        model,
        genderConversions.RuntimeToDatabase[newObject.Gender],
        newObject.FirstName,
        newObject.MiddleInitial,
        newObject.LastName,
        newObject.DateOfBirth,
        json.encode(newObject.Licenses),
        headcomp,
        headtex
    })

    return newObject
end


function Ped:FromRaw(database, netid, firstname, middleinitial, lastname, gender, dob, licenses)
    if not IsDuplicityVersion() then error("Ped can only be created on the server side!") end
    local newObject = setmetatable({}, Ped)

    newObject.id = database:GetNext(Datatypes.PED)
    newObject.netid = netid

    newObject.Gender = gender
    newObject.FirstName = firstname
    newObject.MiddleInitial = middleinitial
    newObject.LastName = lastname

    newObject.DateOfBirth = dob
    newObject.Age = Helpers.GetAge(newObject.DateOfBirth)

    newObject.Licenses = json.decode(licenses)

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