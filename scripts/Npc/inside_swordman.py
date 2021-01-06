# Warrior Job Instructor

job = {
	100 : [["Fighter", "Page", "Spermen"], [110, 120, 130]],
	200 : [["Fire/Poison Wizard", "Ice/Lightning Wizard", "Cleric"], [210, 220, 230]],
	300 : [["Hunter", "Crossbow Man"], [310, 320]],
	400 : [["Assassin", "Bandit"], [410, 420]],
	500 : [["Brawler", "Gunslinger"], [510, 520]],
}

# ensure player is correct level, first job, and an adventurer (dualblade wouldve auto-advanced already at this point)

if ctx.PlayerLevel >= 30 and ctx.PlayerJob in job:
	selText = "Why hello there, lil' Spermie, it looks like you have some job advancement options. What would you like to pick? Remember, you cannot change after you've confirmed your selection."
	for i in range(len(job[ctx.PlayerJob][0])):
		selText += "\r\n#L" + str(i) + "#" + job[ctx.PlayerJob][0][i] + "#l"
	sel = ctx.AskMenu(selText)
	if ctx.AskYesNo("You wish to be a " + job[ctx.PlayerJob][0][sel] + "?\r\nAlright, if you are sure, I will grant you this class."):
		ctx.SetJob(job[ctx.PlayerJob][1][sel])
elif ctx.AskYesNo("I like birds! Do you like birds?"):
	ctx.SendOk("Wow, that's amazing")