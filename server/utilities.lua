local daysPerMonth = {
    31,
    28,
    31,
    30,
    31,
    30,
    31,
    31,
    30,
    31,
    30,
    31
}

Helpers = {
    GenerateDate = function(minimumTimeSince, maxTimeSince, bias)
        local month = math.random(1, 12)
        local days = math.random(1, daysPerMonth[month])
        local mostRecentYear = os.date("*t").year - (minimumTimeSince + 1)
        maxTimeSince = maxTimeSince or 80
        local oldestYear = os.date("*t").year - (maxTimeSince)
        local year
        if bias then
            local possibleYears = {}

            for i = oldestYear, mostRecentYear do
                if i == os.date("*t").year then
                    for k = 0, 6 do
                        possibleYears[#possibleYears+1] = i
                    end
                elseif i > os.date("*t").year then
                    for k = 0, 16 do
                        possibleYears[#possibleYears+1] = i
                    end
                elseif os.date("*t").year - i > 10 then
                    possibleYears[#possibleYears+1] = i
                else
                    for k = 0, 3 do
                        possibleYears[#possibleYears+1] = i
                    end
                end
            end

            year = possibleYears[math.random(1, #possibleYears)]
        else
            year = math.random(oldestYear, mostRecentYear)
        end
        return month .. "/" .. days .. "/" .. year
    end,
    GetAge = function(dob)
        if type(dob) ~= "string" then
            dob = tostring(dob)
        end

        local datedata = {}

        for str in string.gmatch(dob, "([^/]+)") do
            table.insert(datedata, str)
        end

        local date = {
            year = tonumber(datedata[3]),
            month = tonumber(datedata[1]),
            day = tonumber(datedata[2])
        }

        local offset = 0

        if tonumber(datedata[3]) < 1970 then
            offset = 1970 - tonumber(datedata[3])
            date.year = 1970
        end

        local today = os.date("*t")
        dob = os.time(date)
        dob = os.date("*t", dob)

        local age

        if (dob.day < today.day and dob.month <= today.month) or dob.month < today.month then
            age = today.year - (dob.year + offset)
        else
            age = today.year - (dob.year + offset) - 1
        end

        return age
    end,
    AddYear = function(date, value)
        if type(date) ~= "string" then
            date = tostring(date)
        end

        local datedata = {}

        for str in string.gmatch(date, "([^/]+)") do
            table.insert(datedata, str)
        end

        local date = {
            year = tonumber(datedata[3]),
            month = tonumber(datedata[1]),
            day = tonumber(datedata[2])
        }

        return date.month .. "/" .. date.day .. "/" .. (date.year + value)
    end,
    IsDateInPast = function(date)
        if type(date) ~= "string" then
            date = tostring(date)
        end

        local datedata = {}

        for str in string.gmatch(date, "([^/]+)") do
            table.insert(datedata, str)
        end

        local date = {
            year = tonumber(datedata[3]),
            month = tonumber(datedata[1]),
            day = tonumber(datedata[2])
        }

        local today = os.date("*t")

        if (date.day < today.day and date.month <= today.month and date.year <= today.year) or (date.month < today.month and date.year <= today.year) or date.year < today.year then
            return true
        else
            return false
        end
    end
}