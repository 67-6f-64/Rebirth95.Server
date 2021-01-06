#
#	NPC Name: 		Shanks
#	Map(s): 		Maple Road : Southperry (60000)
#	Description: 		Brings you to Victoria Island
#

response = ctx.AskYesNo("Take this ship and you'll head off to a bigger continent. For #e150 mesos#n, I'll take you to #bVictoria Island#k. The thing is, once you leave this place, you can't ever come back. What do you think? Do you want to go to Victoria Island?")

if response:
    if ctx.PlayerLevel < 7:
        ctx.SayOk("Let's see... I don't think you are strong enough. You'll have to be at least Level 7 to go to Victoria Island.")
    elif ctx.HasItem(4038101, 1):
        ctx.SayNext("Okay, now give me 150 mesos... Hey, what's that? Is that the recommendation letter from Lucas, the chief of Amherst? Hey, you should have told me you had this. I, Shanks, recognize greatness when I see one, and since you have been recommended by Lucas, I see that you have a great, great potential as an adventurer. No way would I charge you for this trip!")
        ctx.SayNext("Since you have the recommendation letter, I won't charge you for this. Alright, buckle up, because we're going to head to Victoria Island right now, and it might get a bit turbulent!!")
        ctx.RemoveItem(4031801, 1)
        ctx.Warp(2010000)
    elif ctx.PlayerMeso >= 150
        ctx.SendOk("Awesome! #e150#n mesos accepted! Alright, off to Victoria Island!")
        ctx.GainMeso(-150)
        ctx.Warp(2010000)
    else:
        ctx.SayOk("What? You're telling me you wanted to go without any money? You're one weirdo...")
else:
    ctx.SayOk("Hmm... I guess you still have things to do here?")