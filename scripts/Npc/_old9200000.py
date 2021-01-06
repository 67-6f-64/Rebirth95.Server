#
# NPC Name: Cody
# NPC ID: 9200000
# NPC Location:
#

# [[Boss Maps][Monster Maps][Town Maps]]
WarpMaps = [[100000005, 105070002, 105090900, 230040420, 280030000, 220080000, 240020402, 240020101, 801040100, 240060200, 610010005, 610010012, 610010013, 610010100, 610010101, 610010102, 610010103, 610010104],
[100040001, 101010100, 104040000, 103000101, 103000105, 101030110, 106000002, 101030103, 101040001, 101040003, 101030001, 104010001, 105070001, 105090300, 105040306, 230020000, 230010400, 211041400, 222010000, 220080000, 220070301, 220070201, 220050300, 220010500, 250020000, 251010000, 200040000, 200010301, 240020100, 240040500, 240040000, 600020300, 801040004, 800020130, 800020400],
[130000000, 300000000, 1010000, 680000000, 230000000, 101000000, 211000000, 100000000, 251000000, 103000000, 222000000, 104000000, 240000000, 220000000, 250000000, 800000000, 600000000, 221000000, 200000000, 102000000, 801000000, 105040300, 610010004, 260000000, 540010000, 120000000]]

FirstJobs = [100, 200, 300, 400, 500, 1100, 1200, 1300, 1400, 1500, 2100, 2200, 3200, 3300, 3500]

RebirthBetaItemHunt = [3991017, 3991004, 3991001, 3991008, 3991017, 3991019, 3991007]
# 3991017   Red "R" (double odds)
# 3991004   Red "E"
# 3991001   Red "B"
# 3991008   Red "I"
# 3991017   Red "R"
# 3991019   Red "T"
# 3991007   Red "H"

RebirthBetaItemReward = [1002419]

# trade 200 maple leaves for tier 1
# trade 400 maple leaves plus tier 1 item for tier 2
# trade 600 maple leaves plus tier 2 item plus all rebirth letters for tier 3
RebirthMapleItemExchange = 
[[# level 35
1302020, # Maple Sword

],
[ # level 43
1302030 # Maple Soul Singer     
],
[ # level 64
1302064 # Maple Glory Sword  
]]

MapleItems = [
1012050, # Maple Stein
1012129, # Maple leaf paint
1032040, # Maple earring
1032041, # Maple earring
1032042, # Maple earring
1122015, # Maple Scarf
1122039, # Mind of maple necklace
1142006, # maple idol
1142100, # maple lover
1142101, # maple lover
1142120, # maple explorer
1142126, # Maple school
1001035, # Maplehontas
1002508, # Maple Hat
1002509, # Maple Hat
1002510, # Maple Hat
1002511, # Maple Hat
1002513, # Maple Party Hat
1002515, # Maple Bandana White
1002516, # Maple Bandana Yellow
1002517, # Maple Bandana Red
1002518, # Maple Bandana Blue  
1002600, # Red Maple Bandana
1002601, # Yellow Maple Bandana
1002602, # Blue Maple Bandana
1002603, # White Maple Bandana   
1002717, # Maplemas Hat 
1002758, # Maple Hat (level 4)
1002759, # Maple Hood Hat
]

MenuChoice = ctx.AskMenu("#fUI/UIWindow.img/BetaEdition/BetaEdition#\r\nHey there #d#e#h ##k#n, thanks for playing #bRebirth!#k\r\nI'm Cody and I have lice. Why are you talking to me?\r\n#L0#I need to go somwhere#l\r\n#L1#My job isn't good enough#l\r\n\#L2#Special beta item hunt#l")

# map warp
if MenuChoice == 0:
    MapMenuChoice = ctx.AskMenu("#fUI/UIWindow.img/BetaEdition/BetaEdition#\r\nOooh alrighty then, these are the sections that are currently available.\r\n#L0#Boss Maps#l\r\n#L1#Monster Maps#l\r\n#L2#Town Maps#l")
    MapString = "Good choice. Select your destination.#b"
    for (idx, val) in enumerate(WarpMaps[MapMenuChoice]):
        MapString += "\r\n#L" + str(idx) + "##m" + str(val) + "#"
    MapSelection = ctx.AskMenu(MapString)
    if ctx.AskYesNo("Do you want to go to #m" + str(WarpMaps[MapMenuChoice][MapSelection]) + "#?"):
        ctx.Warp(WarpMaps[MapMenuChoice][MapSelection])
    else:
        ctx.Say("Make up your dang mind, friendo.")

# job advancement
elif MenuChoice == 1:
    # already fourth job
    if ctx.PlayerJob % 10 == 2:
        ctx.Say("You can't fool me, you don't have any job advances left!")
    # into first job
    elif (ctx.PlayerJob % 1000 == 0 or ctx.PlayerJob == 2001) and ctx.PlayerLevel >= 10:
        JobString = "Pick your desired job.#b" 
        for (idx, val) in enumerate(FirstJobs):
            JobString += "\r\n#L" + str(idx) + "#" + ctx.FetchJobName(val) + "#l"
        JobChoice = ctx.AskMenu(JobString)
        if ctx.AskYesNo("Are you sure you want to become a #b" + ctx.FetchJobName(FirstJobs[JobChoice]) + "#k ?"):
            ctx.ChangeJob(FirstJobs[JobChoice])
        else:
            ctx.Say("Understandable, I figured that job would be a little too difficult for you.")
    # into third or fourth job
    elif ctx.PlayerJob % 10 >= 0 and ctx.PlayerJob % 100 != 0:
        secondJob = ctx.PlayerJob % 10 == 0
        if (secondJob and ctx.PlayerLevel < 70) or (not secondJob and ctx.PlayerLevel < 120):
            ctx.Say("Do you take me for a fool!? You have no job advances left!")
        elif ctx.AskYesNo("Are you sure you want to advance and become a #e" + ctx.FetchJobName(ctx.PlayerJob + 1) + "#n?"):
            ctx.SetJob(ctx.PlayerJob + 1)
        else:
            ctx.Say("Well you let me know as soon as you're ready, hun.")
    # into second job
    elif ctx.PlayerLevel >= 30:
        SecondJobs = [ctx.PlayerJob + 10]
        if ctx.PlayerJob <= 500:
            SecondJobs.append(ctx.PlayerJob + 20)
        if ctx.PlayerJob <= 200 or ctx.PlayerJob == 400:
            SecondJobs.append(ctx.PlayerJob + 30)
        JobText = "Which job would you like?"
        for (idx, val) in enumerate(SecondJobs):
            JobText += "\r\n#L" + str(idx) + "#" + ctx.FetchJobName(val) + "#l"
        JobChoice = ctx.AskMenu(JobText)
        ctx.SetJob(SecondJobs[JobChoice])
    else:
        ctx.SayOk("You don't got no options!")

elif MenuChoice == 2:
	SubMenuChoice = ctx.AskMenu("Rebirth beta item hunt!!\r\n#L0#Tell me how it works#l\r\n#L1#Trade items#l")
	
	if SubMenuChoice == 0:
		SayString = "Battle monsters that are no less than five levels below yours to have a chance at finding rebirth letters!\r\nFind one of each letter and come back and talk to me to trade it for unique, limited edition items!\r\nHere are the items you need to find: "
		for (idx, val) in enumerate(RebirthBetaItemHunt):
			SayString.join("#v" + val + "# ")
		ctx.SayPrev(SayString)
	elif SubMenuChoice == 1:
		if ctx.ContainsItemRange(RebirthBetaItemHunt) and ctx.ContainsItem(3991017, 2):
			
		else:
			ctx.SayPrev("Check your inventory and make sure you have all the items, then come back and talk to me again!!")