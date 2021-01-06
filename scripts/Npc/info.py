# Maple Road - Split Road of Destiny
# Shows short video clips to users based on their desired jobs
# Map ID: 1020000

def start():
	if ctx.PlayerMapId == 1020000:
		npc_switch()

def npc_switch():
	ret = {
		10200 : athena,
		10201 : grendel,
		10202 : dances,
		10203 : darklord,
		10204 : kyrin
	}
	return ret[script.SpeakerTemplateID]()

def athena():
	ctx.SayNext("Bowmen are blessed with dexterity and power, taking charge of long-distance attacks, providing support for those at the front line of the battle. Very adept at using landscape as part of the arsenal.")

	if ctx.AskYesNo("Would you like to experience what it's like to be a Bowman?"):
		ctx.ToggleUILock(true)
		ctx.Warp(1020300, 0) # theres prolly some kind of map script for this one?? should prolly look into this
	else:
		ctx.SayOk("If you wish to experience what it's like to be a Bowman, come see me again.")
	
def grendel():
	ctx.SayNext("Magicians are armed with flashy element-based spells and secondary magic that aids party as a whole. After the 2nd job adv., the elemental-based magic will provide ample amount of damage to enemies of opposite element.")

	if ctx.AskYesNo("Would you like to experience what it's like to be a Magician?"):
		ctx.ToggleUILock(true)
		ctx.Warp(1020200)
	else:
		ctx.SayOk("If you wish to experience what it's like to be a Magician, come see me again.")

def dances():
	ctx.SayNext("Warriors possess an enormous power with stamina to back it up, and they shine the brightest in melee combat situation. Regular attacks are powerful to begin with, and armed with complex skills, the job is perfect for explosive attacks.")

	if ctx.AskYesNo("Would you like to experience what it's like to be a Warrior?"):
		ctx.ToggleUILock(true)
		ctx.Warp(1020100)
	else:
		ctx.SayOk("If you wish to experience what it's like to be a Warrior, come see me again.")
		
def darklord():
	ctx.SayNext("Thieves are a perfect blend of luck, dexterity, and power that are adept at the surprise attacks against helpless enemies. A high level of avoidability and speed allows Thieves to attack enemies from various angles.")

	if ctx.AskYesNo("Would you like to experience what it's like to be a Thief?"):
		ctx.ToggleUILock(true)
		ctx.Warp(1020400)
	else:
		ctx.SayOk("If you wish to experience what it's like to be a Thief, come see me again.")
		
def kyrin():
	ctx.SayNext("Pirates are blessed with outstanding dexterity and power, utilizing their guns for long-range attacks while using their power on melee combat situations. Gunslingers use elemental-based bullets for added damage, while Infighters transform to a different being for maximum effect.")

	if ctx.AskYesNo("Would you like to experience what it's like to be a Pirate?"):
		ctx.ToggleUILock(true)
		ctx.Warp(1020500)
	else:
		ctx.SayOk("If you wish to experience what it's like to be a Pirate, come see me again.")

start()