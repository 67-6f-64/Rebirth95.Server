# Aquarium: Aquarium
# Herb Town : Pier on the Beach
# Warps players to Aqua Road <--> Mu Lung

if ctx.PlayerMapId == 251000100:
    selText = "Would you like to go to Aqua Road?"
    destMap = 230000000
else:
    selText = "Would you like to go visit the Herb Town?"
    destMap = 251000100

if ctx.AskYesNo(selText) == 1:
	ctx.Warp(destMap)