if ctx.AskYesNo("Would you like to travel back to orbis?"):
	ctx.Warp(200000100, 4)
else:
	ctx.SayOk("No problem. Check out the dojo while you're here!")