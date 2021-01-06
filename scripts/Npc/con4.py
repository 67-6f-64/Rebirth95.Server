

ctx.SayOk("Oh wow, you did it! You know, that man sure stood firm. Hopefully this'll lead to some much-needed peace here, but I keep fearing for the worst. In any case, I'm just glad he's gone now. ")

count = ctx.GetItemCount(4000141)

if count > 0:
	ctx.SayOk("That's right! The flashlight that the boss drops will be taken care of by me for future purposes. Now that we know who that really is, I feel like the peaceful days may be on its way. I have to admit, finding out the monster is indeed him... that caught me off guard.")
	ctx.RemoveItem(4000141, count) # BMS exchanges it for some shitty potion but idc about that
	ctx.SayOk("CHEERS!")

if ctx.AskYesNo("Do you want to return to Showa Town?"):
	ctx.Warp(801000000)
else:
	ctx.SayOk("That toughness of yours! That was awesome!! Well, if you  need to return to Showa Town, let me know.")