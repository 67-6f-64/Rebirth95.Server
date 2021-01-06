# contains all the guild & alliance NPCs related to creation/modification
# guild rank NPCs arent scripted, selecting them sends a request to the server for the guild ranking information
# 2010007 - Heracle - guild_proc  - Guild creation NPCs
# 2010008 - Lea 	- guild_mark  - Guild mark/emblem modification NPC
# 2010009 - Lenario - guild_union - Alliance creation NPC
# 9040004 - Honorable Rock - Non-scripted guild ranking NPC
# 9040008 - Guild Rank Board - Non-scripted guild ranking NPC

# guild_proc and guild_mark ripped from BMS - guild_union was unfortunately not released yet back then
# script prefix guild_ is directed here.

from Rebirth import GuildConstants

def start():
	res = {
		"guild_proc" : guild_npc,
		"guild_mark" : guild_mark_npc,
		"guild_union" : alliance_npc
	}
	res[script.ScriptName]()


def guild_npc():
	if ctx.PlayerInGuild: guild_npc_player_in_guild()
	else: guild_npc_not_in_guild()


def guild_npc_player_in_guild():
	sel = ctx.AskMenu("So how can I help?\r\n#r#L0#I want to increase the number of members my guild can have.#l\r\n#L1#I want to disband my guild.#l")
	
	if ctx.PlayerIsGuildMaster:
		if sel == 0: # increase size
			cost = ctx.GuildCapacityIncreaseCost;
			if cost == 0: # either an error occurred or the guild has reached max capacity
				ctx.SayOk("Your guild seems to have grown a little bit. I can't increase the capacity of your guild anymore.")
			else: # guild capacity is able to be increased
				ctx.SayOk("Are you here to increase the capacity of your guild? Your guild must have grown a little~ To increase the capacity of your guild, it needs to be re-registered at our guild headquarters and this will require payment for the service...")
				res = ctx.AskYesNo("The cost of the service is #r{} mesos#k\r\nWould you like to increase the capacity of your guild?".format(cost))
				if res:
					if ctx.PlayerMoney < cost: # they want to increase but cant afford it
						ctx.SayOk("It seems like you are lacking the funds required, please check again. You will need to pay the cost of the service to increase capacity and re-register your guild...")
					elif not ctx.IncreaseGuildCapacity(cost): # internal error
						ctx.SayOk("An internal error has occurred. Please try again later or contact staff on discord.")
					# a response is sent from the guild manager if all goes well
		elif sel == 1: # disband
			
			ctx.SayOk("This feature has not been completed yet.")
			return 0
			
			confirm = "Are you sure you want to disband your guild? Seriously... remember, if you disband the guild, it will be eliminated forever. Oh, and one more thing. If you want to disband your guild, you'll need to pay 200,000 mesos for service cost. Do you still want to do this?"
			if ctx.AskYesNo(confirm): #disband
				if not ctx.DisbandGuild(200000): # wants to disband but cant afford it
					ctx.SayOk("Hey, you don't have the money for the job... are you sure you have enough money there?")
				# response packet is sent from guild manager
			else: # dont disband
				ctx.SayOk("Well thought out. I wouldn't want to undo my guild that is already so strong...")
		else: # invalid selection
			ctx.SayOk("An error has occurred.")
	else: # not guild master
		ctx.SayOk("Hey, you are not the guild master!! This decision can only be made by the guild master.")


def guild_npc_not_in_guild(): # guild_proc
	sel = ctx.AskMenu("Hey... Do you happen to be interested in guilds?\r\n#b#L0#What is a guild?#l\r\n#L1#What do I do to create a guild??#l\r\n#L2#I want to create a guild#l")
	
	if sel == 0: # What is a guild?
		say1 = "A guild is... well, you can think of it as a small group full of people with similar interests and goals. In addition, it will be registered at our guild headquarters to be validated."
		
		ctx.SayOk(say1)
		
	elif sel == 1: # How do I create a guild?
		say1 = "To make your own guild, you will need to be at least level {}. You will also need to have at least {} million mesos with you. This is the price for registering your guild.".format(GuildConstants.GUILD_MIN_CHAR_LEVEL, GuildConstants.GUILD_COST_MILLION * 1000000)
		say2 = "To make a guild, you will need {} people in total. These {} must be in the same party and must be at least level {} and the leader must come and talk to me. Also be aware that the party leader also becomes the guild master. Once appointed the guild master, the position remains the same until the guild is disbanded.".format(GuildConstants.GUILD_MIN_PARTY_MEMBERS, GuildConstants.GUILD_MIN_PARTY_MEMBERS, GuildConstants.GUILD_MIN_CHAR_LEVEL)
		say3 = "Okay, to register your guild, bring people here~ You can't create one without all {} with you.\r\nAh, of course, the {} can't be part of another guild!".format(GuildConstants.GUILD_MIN_PARTY_MEMBERS, GuildConstants.GUILD_MIN_PARTY_MEMBERS)
		
		ctx.SayNext(say1)
		ctx.SayNextPrev(say2)
		ctx.SayNextPrev(say3)
		
	elif sel == 2: # I want to create a guild
		if ctx.PlayerLevel < GuildConstants.GUILD_MIN_CHAR_LEVEL:
			ctx.SayOk("Umm ... I don't think you have the qualifications to be a guild master. Please train more to become a guild master.")
		elif ctx.PlayerPartyMemberCount < GuildConstants.GUILD_MIN_PARTY_MEMBERS:
			ctx.SayOk("I don't care how strong you think you are... To form a guild, you need to be in a party of {}. Create a party and then bring all the members here if you really want to create a guild.".format(GuildConstants.GUILD_MIN_PARTY_MEMBERS))
		elif not ctx.PlayerPartyLeader:
			ctx.SayOk("You are not the leader of the party. Please have you leader speak to me to form a guild.")
		elif ctx.PlayerPartyMemberCount != ctx.PlayerPartyMembersInMap:
			ctx.SayOk("It seems that some of your party members are not present. I need all {} members here to register your guild. If your party is unable to coordinate this simple task, you should think twice before forming a guild.".format(GuildConstants.GUILD_MIN_PARTY_MEMBERS))
		elif not ctx.AllPartyMembersGuildless:
			ctx.SayOk("It seems that there is a traitor among you. Someone in your party is already part of another guild. To form a guild, everyone in your party needs to be without a guild. Come back when you have resolved the problem with the traitor.") # lol these dialogs
		elif ctx.PlayerMeso < GuildConstants.GUILD_COST_MILLION * 1000000: # response packet is sent from guild manager
			ctx.SayOk("It seems like you lack the funds to create a guild. You need to pay the cost of the service to create a guild and register it.")
		else:
			ctx.SayOk("Enter the name of your guild and it will be created. The guild will also be officially registered at our guild headquarters. So, good luck to you and your guild!")
			ctx.StartCreateGuild()


def guild_mark_npc(): # guild_mark
	if ctx.PlayerIsGuildMaster:
		ctx.SayNext("Hi! My name is #bLea#k. I am responsible for #bguild emblem#k modifications.")
		res = ctx.AskYesNo("You need #r5,000,000 mesos#k to create or modify a guild emblem. To better explain, a guild emblem is a unique icon for your guild. It will appear next to the guild name in the game. \r\nNow, do you want to modify your guild emblem?")

		if res:
			if ctx.PlayerMeso < 5000000:
				ctx.SayOk("Please revisit your finances, it seems as though you are trying to pull a fast one. You need #r5 million mesos#k to register or change a guild emblem.")
			else:
				ctx.SendModifyGuildMark()
		else:
			ctx.SayOk("Oh... ok... The emblem would make the guild more united. Do you need more time to prepare the guild emblem? Please come back whenever you want.")
			
	else:
		ctx.SayOk("Oh... You are not the guild master. The guild emblem can be made, deleted, or modified only by the #rguild master#k.")

def alliance_npc(): # guild_union
	ctx.SayOk("Coming soon!")
	
start()