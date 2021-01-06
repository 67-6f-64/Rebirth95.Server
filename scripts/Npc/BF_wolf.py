# NPC ID: 9000053
# NPC Name: Big Bad Wolf 
# Exists in battlefield maps


WOLF_TEAM = 1
WOLF_WIN_MAP = 910040003
WOLF_LOSE_MAP = 910040005
ENTRANCE_LOBBY = 910040000
BOMB_ITEM = 2109004
BOMB_AMOUNT = 50


def start():
    global WOLF_TEAM, WOLF_LOSE_MAP, BOMB_ITEM, BOMB_AMOUNT, ENTRANCE_LOBBY, WOLF_WIN_MAP
    
    if ctx.PlayerMapId == WOLF_LOSE_MAP or ctx.PlayerMapId == WOLF_WIN_MAP:
        ctx.SayNext("Hope you had fun!")
        ctx.Warp(ENTRANCE_LOBBY)
        return
    
    if ctx.PlayerBattlefieldTeam == WOLF_TEAM:
        res = ctx.AskMenu("Are you going to exit?\r\n#b#L0#Receive more bombs#l\r\n#L1#Exit#l");
        
        if res == 0:
            ctx.AddItem(BOMB_ITEM, BOMB_AMOUNT)
        elif ctx.AskYesNo("Are you sure you want to leave?"):
            ctx.Warp(WOLF_LOSE_MAP)
    else:
        ctx.SayOk("My buddies are gonna get you..")


start()