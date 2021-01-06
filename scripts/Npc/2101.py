#
# TODO fill out npc info here
#
#

if ctx.AskYesNo("Are you done with your training? If you wish, I will send you out from this training camp."):
	ctx.SendNextPrev("Then, I will send you out from here. Good job.")
	ctx.Warp(40000)
else:
	ctx.SendOk("Haven't you finished the training program yet? If you want to leave this place, please do not hesitate to tell me.")