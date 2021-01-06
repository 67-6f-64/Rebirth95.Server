# Carson - 2111000
# Magatia : Zenumist Society - 261000010

REQ_ITEM = 4031056 # The Book of Ancient

MASTERY_BOOKS = {
    110: "Fighter",
    120: "Hero",
    130: "Spearman",
    
    210: "F/P Wizard",
    220: "I/L Wizard",
    230: "Bishop",
    
    310: "Hunter",
    320: "Crossbowman",
    
    410: "Assassin",
    420: "Bandit",
    430: "Dualblade",
    
    510: "Brawler",
    520: "Gunslinger",
    
	2100: "Aran",
    2200: "Evan",
    
    3200: "Wild Hunter",
    3300: "Battle Mage",
    3500: "Mechanic",
}

if ctx.HasItem(REQ_ITEM):
    ctx.SayNext(f"YOU! I sense the power of #t{REQ_ITEM}#.")
    ctx.SayNextPrev(f"I haven't seen one of those in a very long time, smuggling them out of Victoria Island has been increasingly difficult since the Black Magic scare of '09.")
    ctx.SayNextPrev("HUSH. Did you hear someone? ... Maybe not, this stuff is so hot - makes me paranoid.")
    if ctx.AskYesNo("I'll do you a favor, I can't focus well enough with all the heat to properly make a specific mastery book, but I can make one within a specific class. Is that alright?"):
        text = "Which class would you like?#b"
        
        for key in MASTERY_BOOKS.keys():
            text += f"\r\n#L{key}#{MASTERY_BOOKS[key]}"
        
        ret = ctx.AskMenu(text)
        
        if ctx.AskYesNo(f"Are you sure you want me to make a random {MASTERY_BOOKS[ret]} mastery book?"):
            if ctx.HasItem(REQ_ITEM):  # always check item existence right before giving reward item
                if not ctx.AddRandomMasteryBook(ret):
                    ctx.SayOk("I sense your inventory is full, please make some room.")
                else:
                    ctx.RemoveItem(REQ_ITEM)
                    ctx.SayOk("It's been a pleasure. Now please leave before we are seen together.")
        else:
            ctx.SayOk("Did Manji let you waste his time like this? Leave this place at once.")
    else:
        ctx.SayOk("I have revealed too much. Leave at once.")
else:
    ctx.SayOk("Alchemy is my game, rhyming is not something I'm good at.")