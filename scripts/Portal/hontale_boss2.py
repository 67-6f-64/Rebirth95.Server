# Map: Cave of Trial 2 (normal and chaos horntail maps)
# Function: Spawns horntail right head

qr = ctx.Character.Field.QR

QR_SPAWNED = "spawned"

chaos = ctx.PlayerMapId == 240060101

if QR_SPAWNED not in qr and ctx.Field.Mobs.Count <= 0 and ctx.PlayerFieldUID != 0:
	ctx.Field.BroadcastNotice("The enormous creature is approaching from the deep cave.")
	ctx.Field.QR = QR_SPAWNED
	ctx.SummonMobFromSack(2100176 if chaos else 2100032, -350, 220)