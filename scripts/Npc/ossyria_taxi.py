# Danger Zone Taxi

if ctx.PlayerMapId == 240000000:
    selText = "Would you like to goto the Entrance to Dragon Forest?"
    destMap = 240030000
elif ctx.PlayerMapId == 211000000:
    selText = "Would you like to go visit the Icy Valley II?"
    destMap = 211040200
elif ctx.PlayerMapId == 220000000:
    selText = "Would you like to go visit the Path of Time?"
    destMap = 220050300
else:
    selText = "This case is not handled. Would you like to go visit Henesys?"
    destMap = 100000000

if ctx.AskYesNo(selText) == 1:
	ctx.Warp(destMap)