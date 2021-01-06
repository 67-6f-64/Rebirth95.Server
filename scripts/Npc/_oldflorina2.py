# Pason
# Lith Harbor
# Warps players to Florina Beach

tp = False

tp_option = ctx.AskMenu("Have you heard of the beach with a spectacular view of the ocean called #bFlorina Beach#k, located near Lith Harbor? I can take you there right now for either #b1500 mesos#k, or if you have a #bVIP Ticket to Florina Beach#k with you, in which case you'll be there for free.\r\n\r\n#L0##b I'll pay 1500 mesos.#l\r\n#L1# I have a VIP Ticket to Florina Beach.#l\r\n#L2# What is a VIP Ticket to Florina Beach#k?#l")

if tp_option == 0:
    if ctx.PlayerMeso < 1500:
        ctx.Say("I think you're lacking mesos. There are many ways to gather up some money, you know, like... selling your armor... defeating monsters... doing quests... you know what I'm talking about.")
    else:
        tp = True
        ctx.RemoveMeso(1500)
    
elif tp_option == 1:
    use_ticket = ctx.AskYesNo("So you have a #bVIP Ticket to Florina Beach#k? You can always head over to Florina Beach with that. Alright then, but just be aware that you may be running into some monsters there too. Okay, would you like to head over to Florina Beach right now?")

    if use_ticket:
        if ctx.HasItem(4031134):
            tp = True
        else:
            ctx.SayOk("Are you trying to scam me!? You don't have a #bVIP Ticket to Florina Beach#k !!");
    
elif tp_option == 2:
    ctx.Say("You must be curious about a #bVIP Ticket to Florina Beach#k. Haha, that's very understandable. A VIP Ticket to Florina Beach is an item where as long as you have in possession, you may make your way to Florina Beach for free. It's such a rare item that even we had to buy those, but unfortunately I lost mine a few weeks ago during my precious summer break.")
    
    ctx.SayOk("I came back without it, and it just feels awful not having it. Hopefully someone picked it up and put it somewhere safe. Anyway, this is my story and who knows, you may be able to pick it up and put it to good use. If you have any questions, feel free to ask.")
    
if tp :
    ctx.Warp(120030000)
    