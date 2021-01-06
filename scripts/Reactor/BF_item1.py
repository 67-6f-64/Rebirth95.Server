# Sheep Ranch Blue Box
# Sheep v Wolves event
# ID: 1002002
# Function: Drops any type of sheep item

import random

SHEEP_ITEMS = [
    2022547,  # Slows wolves' movement speed.
    2022542,  # Attacks a Wolf's back, temporarily immobolizing it.
    2022541,  # Protects from a Wolf's attack 1 time.
    2022540,  # Movement speed increased for 3 seconds.
    2022539,  # Wolves slow down when they hear a lamb cry.
]

def start():
    ctx.SystemMessage(str(reactor.nState))
    
    if reactor.nState == 4:
        reactor.SetReactorState(5, False, True)
        ctx.DropItemOnGroundBelow(random.choice(SHEEP_ITEMS))

start()