# NPC ID: 9000054
# NPC Name: Ranch Owner
# Function: Warps player into the Sheep Ranch waiting room

WAITING_ROOM = 910040002


def start():
    global WAITING_ROOM
    
    if ctx.PlayerMapId == WAITING_ROOM:
        waiting_room_npc()
        return
    
    if ctx.PlayerLevel < 30:
        ctx.SayOk("You must be level 30 to join the event!")
        return
    
    res = ctx.AskMenu("Hi! Which mode would you like to join?\r\n"\
    "If the waiting room doesn't fill up during the waiting period, the match will be abandoned.\r\n"\
    "#g#L0#5 person game#l\r\n"\
    "#r#L1#10 person game#l")
    
    if res == 0:
        ctx.WarpToInstance(WAITING_ROOM, 0, 1)  # MapID, PortalID, InstanceID
    else:
        ctx.SayOk("That game mode will be available sometime in the future :)")


def waiting_room_npc():
    ctx.SayOk("You will be warped in when the waiting room reaches 5 players. Have fun!")
    

start()