# zakum npc script collection

EYE_OF_FIRE = 4001017

DOOR_TO_ZAKUM = 211042300
ENTRANCE_TO_ZAKUM = 211042400
ZAKUM_ALTAR = 280030000

ZAKUM_QR = 7001
ZAKUM_CHAOS_QR = 7002

def start():
	mapid = ctx.PlayerMapId
	leader = ctx.PlayerPartyLeader
	
	chaos = mapid % 2 == 1 # only for maps where two versions exist (door, entrance, altar)
	
	sel = mapid - 1 if chaos else mapid
	
	if sel == DOOR_TO_ZAKUM:
		door_to_zakum()
	elif sel == ENTRANCE_TO_ZAKUM:
		entrance_to_zakum()
	elif sel == ZAKUM_ALTAR:
		zakum_altar()

def door_to_zakum():
	ctx.SayOk("The portal will teleport all the party members in your map to the boss entrance.\r\nParty members will not be able to enter after the party is warped in\r\nOnly the party leader may enter.\r\nGood luck, Zaqqum is not for the faint of heart..")

def entrance_to_zakum():
	if ctx.Character.Position.X < -1080:
		ctx.SayOk("Come closer, I can't hear you from over there.")
	else:

		if ctx.PlayerPartyLeader:
			ctx.SayNext("If you are ready, I can take you to the boss fight.\r\nMake sure you have all the equips and items you require, you will not be able to re-enter if you leave.")
			warp = ctx.AskYesNo("Are you ready?")
			
			if warp:
				targetmap = ZAKUM_ALTAR if ctx.PlayerMapId % 2 == 0 else ZAKUM_ALTAR + 1 # normal / chaos
				ctx.WarpPartyToInstance(ctx.PlayerFieldUID, targetmap, 1)
				
		else:
			ctx.SayOk("I only speak to leaders.")

def zakum_altar():
	leave = ctx.AskYesNo("Would you like to leave this place?")
	
	if leave:
		leave = ctx.AskYesNo("Are you sure you want to quit and leave this place? Next time you come back in, you'll have to start all over again.")
		
		if leave:
			ctx.Warp(211042301 if ctx.PlayerMapId % 2 == 1 else 211042300, 2)

start()