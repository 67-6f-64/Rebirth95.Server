# Map: Cave of Trial 1 (normal and chaos horntail maps)
# Function: Spawns horntail left head

qr = ctx.Character.Field.QR

QR_SPAWNED = "spawned"

chaos = ctx.PlayerMapId == 240060001

# ctx.Field.BroadcastNotice("{} {} {}".format(qr, QR_SPAWNED, ctx.Field.Mobs.Count)) # logging

if QR_SPAWNED not in qr and ctx.Field.Mobs.Count <= 0 and ctx.PlayerFieldUID != 0:
	ctx.Field.BroadcastNotice("The enormous creature is approaching from the deep cave.")
	ctx.Field.QR = QR_SPAWNED
	ctx.SummonMobFromSack(2100175 if chaos else 2100031, 880, 220)