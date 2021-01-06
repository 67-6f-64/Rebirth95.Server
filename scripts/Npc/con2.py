


ret = ctx.AskYesNo("Here you are, right in front of the hideout! What? You want to return to Showa Town?")

if ret:
	ctx.Warp(801000000)
else:
	ctx.SayOk("If you want to return to Showa Town, then talk to me.")