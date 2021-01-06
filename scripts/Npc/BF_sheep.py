# NPC ID: 9000052
# NPC Name: Courageous Little Lamb
# Exists in battlefield maps


WOLF_TEAM = 1
SHEEP_WIN_MAP = 910040004
SHEEP_LOSE_MAP = 910040006
ENTRANCE_LOBBY = 910040000
SNARE_ITEM = 2109005
SNARE_AMOUNT = 5


def start():
    global WOLF_TEAM, SHEEP_LOSE_MAP, SNARE_ITEM, SNARE_AMOUNT, ENTRANCE_LOBBY, SHEEP_WIN_MAP
    
    if ctx.PlayerMapId == SHEEP_LOSE_MAP or ctx.PlayerMapId == SHEEP_WIN_MAP:
        ctx.SayNext("Hope you had fun!")
        ctx.Warp(ENTRANCE_LOBBY)
        return
    
    if ctx.PlayerBattlefieldTeam != WOLF_TEAM:
        res = ctx.AskMenu("Are you going to exit?\r\n#b#L0#Receive more snares#l\r\n#L1#Exit#l");
        
        if res == 0:
            ctx.AddItem(SNARE_ITEM, SNARE_AMOUNT)
        elif ctx.AskYesNo("Are you sure you want to leave?"):
            ctx.Warp(SHEEP_LOSE_MAP)
    else:
        ctx.SayOk("You ugly dog..")


start()