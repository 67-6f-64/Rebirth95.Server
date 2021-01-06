# aggregate travel npc -> not taxis, just one-to-one destination NPCs


dict = {
	"contimoveEreEli" : 104020120,
	"contimoveEreOrb" : 200000100,
	"contimoveEliEre" : 130000210,
	"contimoveOrbEre" : 0, # also unused by us rn
	"contimoveRieRit" : 104000000, # rien to lith (Puro)
	"contimoveRitRie" : 140020300, # lith to rien (Puro)
	"contimoveEdeGo" : 0, #unused but needs to be in dict
	"contimoveOrbEde" : 310000010,
	"contimoveEliEde" : 310000010,
}

warp = dict[script.ScriptName]

if script.ScriptName in "contimoveEdeGo":
	res = ctx.AskMenu("Where would you like to go?\r\n#b#L0#Victoria Station#l\r\n#L1#Orbis Station#l")
	if res == 0:
		dest = 104020130
	elif res == 1:
		dest = 200000170
	
	if ctx.AskYesNo("Are you sure you wish to travel to #r#m{}##k?".format(dest)):
		ctx.Warp(dest)
	else:
		ctx.SayOk("Alright, come speak to me when you wish to travel.")
elif script.ScriptName in dict:
	if ctx.AskYesNo("Hey there, bored of #m{}# yet? I can take you to #m{}# if you want..".format(ctx.PlayerMapId, warp)):
		ctx.Warp(warp)
	else:
		ctx.SayOk("No worries, I understand. Come back and see me when you wish to leave this god forsaken island.")
else:
	ctx.SayOk("I've got a pretty big boat, don't you think?") # just olaf in v95