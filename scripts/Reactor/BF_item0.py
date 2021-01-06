# Sheep Ranch Brown Box
# Sheep v Wolves event
# ID: 1002001
# Function: Drops any type of sheep v wolf item

import random

SHEEPRANCH_ITEMS = [
    # wolf items
    2022548,  # Sheep are temporarily unable to move.
    2022549,  # Intimidates the sheep, making them weaker.
    2022543,  # Causes the sheep to become confused and lose direction.
    # sheep items
    2022547,  # Slows wolves' movement speed.
    2022542,  # Attacks a Wolf's back, temporarily immobolizing it.
    2022541,  # Protects from a Wolf's attack 1 time.
    2022540,  # Movement speed increased for 3 seconds.
    2022539,  # Wolves slow down when they hear a lamb cry.
]


def start():
    ctx.SystemMessage("asd")
    if not reactor.bDead: return
    
    ctx.DropItemOnGroundBelow(random.choice(SHEEPRANCH_ITEMS))


start()