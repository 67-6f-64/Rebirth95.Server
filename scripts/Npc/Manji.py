# 4031049 - A Piece of an Ancient Scroll - An old piece of paper that seems to be part of an ancient document.
# 4001028 - Scroll of Wisdom - A scroll that contains various invaluable strategies that can be used in combat.
# 4031019 - Scroll of Secrets - A mystical scroll written in a lost ancient language.

# 4031056 - The Book of Ancient - A book that contains incredible spells that are banned. A regular person can't even open the book; it's that powerful...

# item id: amount
GENERIC_REQUIREMENTS = {
    4031049: 9, # A Piece of an Ancient Scroll
    4001028: 3,  # Scroll of Wisdom
    4031019: 1,  # Scroll of Secrets
}

GENERIC_REQUIREMENTS_LEVEL = {
    4031049: 70, # A Piece of an Ancient Scroll
    4001028: 70, # Scroll of Wisdom
    4031019: 120,  # Scroll of Secrets
}

# TODO use the forging manuals when the server is a bit larger
FORGING_MANUALS = {
    110: 4131000, # one handed sword
    120: 4131005, # two handed mace
    130: 4131006, # spear
    
    210: 4131009, # staff
    220: 4131009, # staff
    230: 4131008, # wand
    
    310: 4131010, # bow
    320: 4131011, # crossbow
    
    410: 4131013, # claw
    420: 4131012, # dagger
    430: 4131016, # katara
    
    510: 4131014, # knuckler
    520: 4131015, # gun
    
	2100: 4131007, # polearm
    2200: 4131008, # wand
    
    3200: 4131009, # staff
    3300: 4131011, # crossbow
    3500: 4131015, # gun
}

PRODUCT = 4031056 # The Book of Ancient

MASTERY_BOOKS = {
    110: 0,
    120: 0,
    130: 0,
    
    210: 0,
    220: 0,
    230: 0,
    
    310: 0,
    320: 0,
    
    410: 0,
    420: 0,
    430: 0,
    
    510: 0,
    520: 0,
    
	2100: 0,
    2200: 0,
    
    3200: 0,
    3300: 0,
    3500: 0,
}

ctx.SayNext("I'm skilled in the ancient arts of combat. Also language.")

hasItem = any(ctx.HasItem(item) for item in GENERIC_REQUIREMENTS.keys())
    
if hasItem:
    if all(ctx.HasItem(item, GENERIC_REQUIREMENTS[item]) for item in GENERIC_REQUIREMENTS.keys()):
        if ctx.AskYesNo(f"Good, you have all the ancient texts. I can translate and document them for you, which will yield a #v{PRODUCT}#.\r\nYou can then take this book to the Zenumist President in Magatia, he will continue the process.\r\nI will help you, but you must speak of this to no one. Do you wish to proceed?"):
            if ctx.CanHoldItem(PRODUCT, 1) and all(ctx.HasItem(item, GENERIC_REQUIREMENTS[item]) for item in GENERIC_REQUIREMENTS.keys()):  # always recheck requirements before items are given
                ctx.AddItem(PRODUCT, 1)
                for item in GENERIC_REQUIREMENTS.keys():
                    ctx.RemoveItem(item, GENERIC_REQUIREMENTS[item])
                ctx.SayOk("Remember, take the item to the Zenumist President in Magatia. I won't repeat myself again.\r\nTell no one of this, now begone.")
            else:
                ctx.SayOk("Please make some space in your etc inventory..")
        else:
            ctx.SayOk("You come here and waste my time, risking us both get caught with the dark arts? Get out of my sight you sick creature.")
    else:
        response = f"I sense you carry some valuable documents. I can translate them for you, but your set is incomplete.\r\nTo make a #v{PRODUCT}#, provide me with the following:#b#r"
        
        for item in GENERIC_REQUIREMENTS.keys():
            response += f"\r\n\t{GENERIC_REQUIREMENTS[item]} of #v{item}# (level {GENERIC_REQUIREMENTS_LEVEL[item]}+ monsters)."
        
        response += "\r\n#kYou can smuggle the book out to.. an associate of mine.. to forge a mastery book."
        
        ctx.SayOk(response)
else:
    ctx.SayOk("When you find one of the items I need, I will talk to you.. Until then, there can be no trust.\r\nLook for rare documents, I.. love rare documents.")