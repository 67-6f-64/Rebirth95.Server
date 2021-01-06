


if ctx.HasItem(4000141):
	ctx.SayOk("That... that flashlight!! You really defeated the boss...?? You...! Wow, I must have a knack for finding talent. That's just incredible! Now let's get out of here!")
	ctx.Warp(801040101)
elif ctx.AskYesNo("Once you eliminate the boss, you'll have to show me the boss's flashlight as evidence. I won't believe it until you show me the flashlight! What? You want to leave this room?"):
	ctx.Warp(801040000)
else:
	ctx.SayOk("I really admire your toughness! Well, if you decide to return to Showa Town, let me know~!")