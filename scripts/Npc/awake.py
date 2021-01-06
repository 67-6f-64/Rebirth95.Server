

if ctx.AskYesNo("Oh, you're back. Why are you here? Do you need help leaving?"):
	ctx.Warp(140030000)
else:
	ctx.SayOk("Well, sit tight then.")