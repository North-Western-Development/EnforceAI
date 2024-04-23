local QBCore = exports["qb-core"]:GetCoreObject()
local MainMenu = MenuV:CreateMenu("EnforceAI", "", "topright", 0, 0, 255, 'size-100', 'default', 'menuv', 'enforceai', "native")
local SceneMenu = MenuV:CreateMenu("Scene Menu", "", "topright", 0, 0, 255, 'size-100', 'default', 'menuv', 'enforceaiscene', "native")
local DispatchMenu = MenuV:CreateMenu("Dispatch Menu", "", "topright", 0, 0, 255, 'size-100', 'default', 'menuv', 'enforceaidispatch', "native")

SetupMenus(MainMenu, SceneMenu, DispatchMenu)

RegisterKeyBinds(MainMenu, SceneMenu, DispatchMenu)

ClientDatabase = Database:new()
LocalPlayer = nil

AddEventHandler("onResourceStart", function(resource)
    if resource ~= GetCurrentResourceName() then return end

    if QBCore.Functions.GetPlayerData() == nil then return end

    local loginData = Callbacks.Create("login", {}, "EnforceAI::server::login", 1000)
    local result = Citizen.Await(loginData)
    LocalPlayer = Player.FromData(result.player)
end)

AddEventHandler("onResourceStop", function(resource)
    if resource ~= GetCurrentResourceName() then return end

    PropManagerState.DrawGhost = false
    CleanUpPropManager()
end)

RegisterNetEvent('QBCore:Client:OnPlayerLoaded', function()
    local loginData = Callbacks.Create("login", {}, "EnforceAI::server::login", 1000)
    local result = Citizen.Await(loginData)
    LocalPlayer = Player.FromData(result.player)
end)

RegisterNetEvent('QBCore:Client:OnPlayerUnload', function()
    if LocalPlayer == nil then return end
    local logoutData = Callbacks.Create("login", {id = LocalPlayer.id}, "EnforceAI::server::logout", 1000)
    Citizen.Await(logoutData)
    LocalPlayer = nil
end)