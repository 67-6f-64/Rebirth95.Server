#Forgotten Temple Keeper - Forgotten Twilight (PB Entrance)

MAP = 270050100

if ctx.AskYesNo("Are you sure you want to go in?"):
    if not ctx.PlayerInParty:
        ctx.SayOk("Please join a party before going in.")
    elif not ctx.PlayerPartyLeader:
        ctx.SayOk("Please have your party leader talk to me if you wish to face Pink Bean.")
    elif ctx.PlayerPartyLowestCharLevel < 160:
        ctx.SayOk("Someone in your party is below the required level of 160.")
    else:
        if ctx.AskMenu("Make sure all your party members are here, they won't be able to join after you have entered the boss battle.\r\n#b#L0#They are all here.#l\r\n#L1#I'm gonna wait for them to arrive..#l") == 0:
            ctx.WarpPartyToInstance(MAP)
else:
    ctx.SayOk("Understandable, have a nice day.")