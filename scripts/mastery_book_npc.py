# 4031056 - The Book of Ancient - A book that contains incredible spells that are banned. A regular person can't even open the book; it's that powerful...
# 4032513 - Wrinkled Sketchbook Loose-Leaf - A sheet of paper that appears to be from a sketchbook.

# item id: amount
GENERIC_REQUIREMENTS {
    4031049: 10, # A Piece of an Ancient Scroll
    4001028: 10, # Scroll of Wisdom
    4031019: 1,  # Scroll of Secrets
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
    520: 04131015, # gun
    
	2100: 4131007, # polearm
    2200: 4131008, # wand
    
    3200: 4131009, # staff
    3300: 4131011, # crossbow
    3500: 04131015, # gun
}

PRODUCT = 4031056 # The Book of Ancient

MASTERY_BOOKS {
    110:
    120:
    130:
    
    210:
    220:
    230:
    
    310:
    320:
    
    410:
    420:
    430:
    
    510:
    520:
    
	2100:
    2200:
    
    3200:
    3300:
    3500:
}

ctx.SayNext("Hey there kid, you got a uhh.. you know what?")

result = False
for item in GENERIC_REQUIREMENTS.keys():
    if ctx.HasItem(item): result = True
    
if result:
    for item in GENERIC_REQUIREMENTS.keys():
        if not ctx.HasItem(item, GENERIC_REQUIREMENTS[item]): result = False
    
    if result:
        ret = ctx.AskYesNo(f"It seems you have all the items I need to make a #t{PRODUCT}##v{PRODUCT}#.\r\nI will help you, but you must speak of this to no one. Do you wish to proceed?")
        if ret:
            if ctx.CanHoldItem(PRODUCT, 1):
                ctx.AddItem(PRODUCT, 1)
                for item in GENERIC_REQUIREMENTS.keys():
                    ctx.RemoveItem(item, GENERIC_REQUIREMENTS[item])
                ctx.SayOk("Tell no one of this, now begone.")
            else:
                ctx.SayOk("Please make some space in your etc inventory..")
        else:
            ctx.SayOk("You come here and waste my time, risking us both get caught with the dark arts? Get out of my sight you sick creature.")
    else:
        response = f"It seems you have acquired some of the items but not all of them.\r\nTo make a #t{PRODUCT}##v{PRODUCT}# I need the following:"
        
        for item in GENERIC_REQUIREMENTS.keys():
            response += f"\r\n\t{GENERIC_REQUIREMENTS[item]} #t{item}##v{item}#"
        
        ctx.SayNext(response)
else:
    ctx.SayOk("When you find one of the forbidden items, I will talk to you.. Until then, there can be no trust.")