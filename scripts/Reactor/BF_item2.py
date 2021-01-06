# Sheep Ranch Pink Box
# Sheep v Wolves event
# ID: 1002003
# Function: Drops any type of wolf item

import random

WOLF_ITEMS = [
    2022548,  # Sheep are temporarily unable to move.
    2022549,  # Intimidates the sheep, making them weaker.
    2022543,  # Causes the sheep to become confused and lose direction.
]


def start():
    if reactor.nState == 4:
        reactor.SetReactorState(5, False, True)
        ctx.DropItemOnGroundBelow(random.choice(WOLF_ITEMS))


start()
