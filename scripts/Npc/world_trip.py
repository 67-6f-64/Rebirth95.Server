# Spinel
# World Tour Guide

options = [
	"#g#L0#Rock Dungeon Raid#l#k", # golem -> 100040100
	"#r#L1#Dark Dungeon Raid#l", # sleepwood
	"#L2#Shopping Dungeon Raid#l", # kerning square
	"#L3#Sky Dungeon Raid#l", # orbis
	"#L4#Toy Dungeon Raid#l", # ludi
	"#L5#Desert Dungeon Raid#l", # nihal desert
	"#L6#Hell Dungeon Raid#l", # el nath
]

maps = [100040100, 0, 0, 0, 0, 0, 0]

if ctx.PlayerLevel < 45:
	ctx.SayNext("Why travel to another dimension when you could travel the world?")
	ctx.SayNextPrev("Come talk to me when you are a bit stronger..")
else:
	ctx.SayNext("I provide the most exquisite services to monster hunters such as yourself.")
	
	if ctx.PlayerInParty and not ctx.PlayerPartyLeader:
		ctx.SayOk("Please have your party leader speak to me..")
	else:
		menu = "What would you like to slay today?"
		
		for i in options:
			menu += "\r\n{}".format(i)
		
		res = ctx.AskMenu(menu)
		
		if res >= 0 and res <= len(maps):
			if res == 0:
				ctx.SayOk("On you go, then!")
				ctx.WarpPartyToInstance(maps[res])
			else:
				ctx.SayOk("Coming Soon!")