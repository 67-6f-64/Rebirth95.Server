#Pink Bean Exit
if ctx.AskYesNo("Are you sure you want to leave?\r\nYou will not be able to return."):
	ctx.Warp(270050300, 0)
else:
	ctx.SayOk("I'll talk to you again soon.")