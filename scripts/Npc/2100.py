#########
#	NPC Name:       Sera
#	Map(s): 	    Maple Road : Entrance - Mushroom Town Training Camp (0), Maple Road: Upper level of the Training Camp (1), Maple Road : Entrance - Mushroom Town Training Camp (3)
#	Description:    First NPC
########

if ctx.PlayerMapId == 0 || ctx.PlayerMapId == 3:
	response = ctx.AskYesNo("Welcome to the world of MapleStory. The purpose of this training camp is to help beginners. Would you like to enter this training camp? Some people start their journey without taking the training program. But I strongly recommend you take the training program first.")
	
	# yes camp
	if response:
		ctx.SayOk("Ok then, I will let you enter the training camp. Please follow your instructor's lead.")
		ctx.Warp(1)
	# no camp
	else:
		if ctx.AskYesNo("Do you really want to start your journey right away?"):
			ctx.SayOk("It seems like you want to start your journey without taking the training program. Then, I will let you move on to the training ground. Be careful~")
			ctx.Warp(40000)
		else:
			ctx.SayOk("Please talk to me again when you finally made your decision.")
# wrong map text
else:
	ctx.Say("This is the image room where your first training program begins. In this room, you will have an advance look into the job of your choice.")

# end text
ctx.Say("Once you train hard enough, you will be entitled to occupy a job. You can become a Bowman in Henesys, a Magician in Ellinia, a Warrior in Perion, and a Thief in Kerning City...")