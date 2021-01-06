# Gaga npc
# ID: 9000021

EVENT_EXIT_QR_CODE = 9000
DEFAULT_RETURN_MAP = 100000000  # henesys

qr = ctx.GetQuestRecord(EVENT_EXIT_QR_CODE, str(DEFAULT_RETURN_MAP))

if ctx.PlayerMapId == 910030000: # russian roulette map
    if ctx.AskYesNo("Are you sure you want to leave? You won't be able to return once you exit the map."):
        ctx.Warp(int(qr))
else:
    ctx.SayOk("Gachapon's are now live for testing!\r\nBut only if you can find a ticket..")