# NLC Taxi

if ctx.PlayerMapId == 682000000:
    selText = "Would you like to go back to New Leaf City?"
    destMap = 600000000
else:
    selText = "Would you like to go visit the Haunted House?"
    destMap = 682000000

if ctx.AskYesNo(selText) == 1:
	ctx.Warp(destMap)