if ctx.PlayerMapId >= 100040100 and ctx.PlayerMapId <= 100040500:
	ctx.SayNext("You must kill all the terrifying monsters before I'll let you proceed.\r\nI don't want any of them left in my beatiful ancient ruins.")

	if ctx.AskYesNo("Would you like to forfeit? You will not receive any rewards from this stage if you end prematurely."):
		ctx.Warp(100000000) # todo proper exit map
	else:
		ctx.SayOk("Well go on then. #gSlay, queen#k, as they say~")
else:
	ctx.SayOk("The training camp is not for the faint of heart.")