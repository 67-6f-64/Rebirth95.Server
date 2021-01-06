# Taco
# 910000000 - Free Market Entrance
# Warper NPC

MAPS = {
	682000000: "Haunted House",
	682010203: "Chimney Possessed by the Clown",
	610010200: "Crossroads",
	801040003: "Parlor",
}

BOOKS = {
    0 : 0
}

def start():
    selText = "#L0#Travel#l\r\n#L1#Mastery Books#l#"
    
    sel = ctx.AskMenu(selText)
    
    if sel == 0:
        selText = "Where would you like to travel?"
            
        for i in MAPS:
            selText += "\r\n#L{}#{}#l".format(i, MAPS[i])

        sel = ctx.AskMenu(selText)

        ctx.Warp(sel)
    elif sel == 1:
        ctx.SayOk("TODO")

start()