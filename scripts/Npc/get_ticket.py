
if ctx.AskYesNo("It looks like you have approximately.. Zero minutes until the ship leaves. Do you wish to board?"):
	if ctx.PlayerMapId == 104020110 or ctx.PlayerMapId == 220000110 or ctx.PlayerMapId == 240000110 or ctx.PlayerMapId == 260000100:
		ctx.Warp(200000100, 4)
else:
	ctx.SayOk("Roger that, let me know when you wish to travel!")