# sheep ranch - ranch backstreet
# event exit map portal 
# map id: 910040001

ENTRANCE_MAP = 910040000
EXIT_MAP = 910040001
WAITING_ROOM = 910040002

SHEEP_RANCH_PREVMAP_QID = 9010


def start():
    global ENTRANCE_MAP, EXIT_MAP, WAITING_ROOM
    
    prevMap = int(ctx.GetQuestRecord(SHEEP_RANCH_PREVMAP_QID, str(100000000)))  # default henesys
    
    switch = {
        WAITING_ROOM: ENTRANCE_MAP,
        EXIT_MAP: ENTRANCE_MAP,
        ENTRANCE_MAP: prevMap
    }
    ctx.Warp(switch[ctx.PlayerMapId])


start()