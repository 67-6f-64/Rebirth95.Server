FM_QR_CODE = 7600

FM_MAP = 910000000
INTRO_MAP = 180000001
DEFAULT_RETURN_MAP = 100000000  # henesys

FM_INSIDE_PORTAL = 'out00'
FM_TOWN_GATE_NAME = 'market00'

if ctx.PlayerMapId >= 910000000 and ctx.PlayerMapId <= 910000022:  # exiting fm
	targetmap = ctx.GetQuestRecord(FM_QR_CODE, str(DEFAULT_RETURN_MAP))
	ctx.Warp(int(targetmap),FM_TOWN_GATE_NAME)
   
elif ctx.PlayerMapId >= 749050500 and ctx.PlayerMapId <= 749050502:
    ctx.ScriptProgressMessage("Not available in this map")

elif ctx.PlayerMapId == INTRO_MAP:
    ctx.ScriptProgressMessage("Not available in this map")
    
else:  # entering fm
	ctx.UpdateQuestRecordInternal(FM_QR_CODE, str(ctx.PlayerMapId))  # save previous field ID as quest record
	ctx.Warp(FM_MAP, FM_INSIDE_PORTAL)