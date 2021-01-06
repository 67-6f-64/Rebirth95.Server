# Dimensional Mirror
# Exists in all towns

DESTINATIONS = {
    1 : 925020000  # dojo
}

QUICK_MOVE_QR = 7700

def start():
    global DESTINATIONS, QUICK_MOVE_QR
    ret = ctx.AskSlideMenu("#1# Dojo PQ")
    
    if ret not in DESTINATIONS: return
    
    ctx.UpdateQuestRecordInternal(QUICK_MOVE_QR, str(ctx.PlayerMapId))  # save previous field ID as quest record
    
    ctx.Warp(DESTINATIONS[ret], "out00")
    
start()