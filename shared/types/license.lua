License = {}
License.__index = License

License.IssuanceDate = ""
License.ExpirationDate = ""
License.Status = 0
License.Type = 0
License.Metadata = {}

local randomStatuses = {
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    2,
    2,
    2,
    2,
    3,
    3
}

function License:new(type, age)
    local newObject = setmetatable({}, License)

    newObject.Type = type

    local hasLicense = math.random(0, 100)
    if hasLicense > 90 then
        newObject.Status = LicenseStatues.NONE
        newObject.IssuanceDate = "N/A"
        newObject.ExpirationDate = "N/A"
        return newObject
    end

    newObject.ExpirationDate = Helpers.GenerateDate(-4, age-16, true)
    newObject.IssuanceDate = Helpers.AddYear(newObject.ExpirationDate, -4)

    if Helpers.IsDateInPast(newObject.ExpirationDate) then
        newObject.Status = LicenseStatues.EXPIRED
    else
        local statusNumber = math.random(1, #randomStatuses)
        newObject.Status = randomStatuses[statusNumber]
    end

    return newObject
end