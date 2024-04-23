fx_version "cerulean"
game "gta5"

author "North Western Development and Contributors"
title "EnforceAI"
description "Police the locals in your server"

client_scripts {
    "config.lua",
    "@menuv/menuv.lua",
    "client/utilities.lua",
    "client/propmanager.lua",
    "client/client.lua"
}

server_scripts {
    "config.lua",
    "server/*.lua"
}

shared_scripts {
 "shared/*.lua",
 "shared/types/*.lua"
}

lua54 "yes"