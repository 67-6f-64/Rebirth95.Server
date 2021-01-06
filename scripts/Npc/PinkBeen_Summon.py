# Kirston - Pink Bean Spawner
# NPC ID: 2141000
# Map ID: 270050100

INITIAL_MOB = 8820008
KIRSTON = 2141000

PX = 5
PY = -43

ctx.SayOk("You have disturbed great forces.. you will live to regret this!")
ctx.RemoveNpc(KIRSTON)
ctx.SpawnMonster(INITIAL_MOB, PX, PY)
