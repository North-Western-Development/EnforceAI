Callbacks = {
    Create = function(name, data, toCall, timeout, client)
        local awaitable = promise.new()
        if IsDuplicityVersion() then
            local callbackName = "EnforceAI::server::callbacks::"..name
            Citizen.CreateThread(function ()
                local resolved = false
                local listener = RegisterNetEvent(callbackName, function(data)
                    resolved = true
                    if listener ~= nil then
                        RemoveEventHandler(listener)
                    end
                    awaitable:resolve(data or {})
                end)
                TriggerClientEvent(toCall, client, data, callbackName)
                if timeout then
                    local time = 0
                    while time < timeout and not resovled do
                        time = time + 10

                        Wait(10)
                    end
                    if not resolved then awaitable:reject("timeout") end
                end
            end)
            return awaitable
        else
            local callbackName = "EnforceAI::client::callbacks::"..name
            Citizen.CreateThread(function ()
                local resolved = false
                local listener = RegisterNetEvent(callbackName, function(data)
                    resolved = true
                    if listener ~= nil then
                        RemoveEventHandler(listener)
                    end
                    awaitable:resolve(data or {})
                end)
                TriggerServerEvent(toCall, data, callbackName)
                if timeout then
                    local time = 0
                    while time < timeout and not resovled do
                        time = time + 10

                        Wait(10)
                    end
                    if not resolved then awaitable:reject("timeout") end
                end
            end)
            return awaitable
        end
    end
}