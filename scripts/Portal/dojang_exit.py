QUICK_MOVE_QR = 7700

DEFAULT_RETURN_MAP = 100000000 # henesys

targetmap = ctx.GetQuestRecord(QUICK_MOVE_QR, str(DEFAULT_RETURN_MAP))
ctx.Warp(int(targetmap), "unityPortal2")