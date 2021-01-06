
QR_SPAWNED = "spawned"

if ctx.Field.QR not in QR_SPAWNED or ctx.Field.Mobs.Count > 0 and ctx.PlayerFieldUID != 0:
	ctx.SystemMessage("The portal doesn't work now.")
else:
	ctx.WarpPartyToInstance(ctx.PlayerMapId + 100)