# Pietra
# exists in event exit maps
# warps player to their previous field

EVENT_EXIT_QR_CODE = 9000
DEFAULT_RETURN_MAP = 100000000  # henesys

qr = ctx.GetQuestRecord(EVENT_EXIT_QR_CODE, str(DEFAULT_RETURN_MAP))

ctx.SayOk("Hope you had fun! I'll warp you back to where you were now.")

ctx.Warp(int(qr))  # no portal id