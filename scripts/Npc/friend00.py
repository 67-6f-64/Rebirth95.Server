# Mr. Goldstein : Lith Harbor

response = ctx.AskYesNo("I hope I can make as much as yesterday... well, hello! Don't you want to extend your buddy list? You look like someone who'd have a whole lot of friends... well, what do you think? With some money I can make it happen for you. Remember, though, it only applies to one character at a time, so it won't affect any of your other characters on your account. Do you want to extend your buddy list?")

ctx.AskYesNo("Alright, good call! It's not that expensive actually. #b250,000 mesos and I'll add 5 more slots to your buddy list#k. And no, I won't be selling them individually. Once you buy it, it's going to be permanently on your buddy list. So if you're one of those that needs more space there, then you might as well do it. What do you think? Will you spend 250,000 mesos for it?")

buddylist_capacity = 20 # We don't do this yet

if buddylist_capacity >= 50 or ctx.Character.Stats.nMoney < 250000:
    ctx.Say("Hey... are you sure you have #b250,000 mesos#k? If so, then check and see if you have extended your buddy list to the max. Even if you pay up, the most you can have on your buddy list is #b50#k.")
else:
    ctx.Character.Modify.GainMeso(-250000)
    ctx.Character.Action.SystemMessage("This isn't setup yet, but you lost mesos :(", "sad day")

ctx.Dispose()
    

def close():
    if response == 1:
        ctx.Say("I see... I don't think you don't have as many friends as I thought you would. If not, you just don't have 250,000 mesos with you right this minute? Anyway, if you ever change your mind, come back and we'll talk business. That is, of course, once you have get some financial relief... hehe ...")
    else:
        ctx.Say("I see... you don't have as many friends as I thought you would. Hahaha, just kidding! Anyway if you feel like changing your mind, please feel free to come back and we'll talk business. If you make a lot of friends, then you know ... hehe ...")
    
    ctx.Dispose()