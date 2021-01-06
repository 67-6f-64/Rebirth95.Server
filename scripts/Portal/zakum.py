# zakum portals

def start():
    DOOR_TO_ZAKUM = 211042300
    DOOR_TO_CHAOS = 211042301

    ENTRANCE_TO_ZAKUM = 211042400
    ENTRANCE_TO_CHAOS = 211042401

    if ctx.PlayerPartyLeader:

        if not ctx.HasSpace(4001017):
            ctx.SystemMessage("Please make space in your etc inventory..")
            return
        
        ctx.AddItem(4001017)

        if ctx.PlayerMapId == DOOR_TO_ZAKUM or ctx.PlayerMapId == DOOR_TO_CHAOS:
            ctx.WarpPartyToInstance(ctx.PlayerFieldUID, ctx.PlayerMapId + 100, 1)
        
    else:
        ctx.SystemMessage("Only the party leader may enter..")

       
start()