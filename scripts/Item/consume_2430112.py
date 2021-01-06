# ItemID: 02430112
# Item Name: Miracle Cube Fragment
# Description: A fragment from Miracle Cube. Double-click it to use the item. \n Collecting 5 of these fragments will earn you a Potential Scroll, and collecting 10 will earn you an Advanced Potential Scroll.
# Item Data: info.price=1,  info.tradeBlock=1,  info.slotMax=100,  spec.script=consume_2430112,  spec.npc=9010000  

# define constants
NOT_ENOUGH_ITEMS = "You need at least five fragments to receive a potential scroll."
TRADE_BASIC = "You have traded five fragments for one potential scroll."
TRADE_ADVANCED = "You have traded ten fragments for one advanced potential scroll."

ADVANCED_SCROLL = 2049400
ADVANCED_COST = 10
BASIC_SCROLL = 2049401
BASIC_COST = 5

FRAGMENT_ID = ctx.ScriptItemID

# entry point
def start():
    fragmentcount = ctx.GetItemCount(FRAGMENT_ID)

    if fragmentcount < BASIC_COST:
        ctx.SystemMessage(NOT_ENOUGH_ITEMS)
    elif fragmentcount >= BASIC_COST and fragmentcount < ADVANCED_COST:
        ctx.RemoveItem(FRAGMENT_ID, BASIC_COST)
        ctx.AddItem(BASIC_SCROLL)
        ctx.SystemMessage()
    elif fragmentcount >= ADVANCED_COST:
        ctx.RemoveItem(FRAGMENT_ID, ADVANCED_COST)
        ctx.AddItem(ADVANCED_SCROLL)
        ctx.SystemMessage()


# call entry point
start()