# Mark of the Squad
# Horntail entrance NPC
# Exists in map: Entrance to Horntail's Cave

# Requirements: at least 3 party members, all party members level 80+

if ctx.PlayerPartyLeader:
	if not ctx.PlayerInParty:
		ctx.SayOk("You must have a party to challenge.")
	elif ctx.PlayerPartyLowestCharLevel < 80:
		ctx.SayOk("Only the users with level 80 or above can join in.")
	elif ctx.PlayerPartyMemberCount < 1:
		ctx.SayOk("Party leader with more than three party members with level 80 or above can enter Horntail.")
	else:
		ret = ctx.AskMenu("   Which horned epic boss would you like to challenge today?\r\n#b#L0#Horntail#l\r\n#L1#Chaos Horntail#l")
		if ret == 0:
			ctx.WarpPartyToInstance(240060000)
		elif ret == 1:
			ctx.WarpPartyToInstance(240060001)
else:
	ctx.SayOk("Please have your leader speak with me.")