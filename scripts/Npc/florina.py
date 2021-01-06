# warps players to and from florina beach
# mostly ripped from BMS

# florina1: 1081001 (Pison) -> warps from florina to lith, orbis, and ludi
# florina2: 1002002 (Pason), 2010005 (Shuri), 2040048 (Nara) -> warps from lith, orbis, ludi to florina

def start():
	if script.ScriptName is "florina1":
		florina_to_mainland()
	else:
		mainland_to_florina()


def florina_to_mainland():
	res = ctx.AskMenu("So you want to leave Florina Beach? Where do you wish to go?\r\n#b#L0#Lith Harbor#l\r\n#L1#Orbis#l\r\n#L2#Ludibrium#l")
	
	if res > 2 or res < 0: return
	
	destinations = [ 104000000, 200000000, 220000000 ]
	map = destinations[res]
	
	if ctx.AskYesNo("Are you sure you want to go to #r#m" + str(map) + "##k?"):
		ctx.Warp(map)
	else:
		ctx.SayOk("You must still have some business to do here. It's not a bad idea to rest for a while in Florina Beach. Look at me, I like it here so much that I'm going to end up living here. Hahaha ... well, anyway way, talk to me when you want to leave.")


def mainland_to_florina():
	if ctx.AskYesNo("Have you heard of the beach with a spectacular view of the ocean called #bFlorina Beach#k, located near Lith Harbor? I can take you there right now for free. It used to cost 1,500 mesos but ever since the outbreak our travel branch has been subsidized by the government. Would you like to go?"):
		ctx.Warp(120030000)
	else:
		ctx.SayOk("No problem, come back any time!~")


start()