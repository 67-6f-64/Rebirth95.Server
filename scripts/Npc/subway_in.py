# The Ticket Gate
# Kerning City Subway: Subway Ticketing Booth


tp_option = ctx.AskMenu("Where would you like to go?\r\n\r\n#L0#Kerning City Subway#l\r\n#L1#New Leaf City#l\r\n")

if tp_option == 0:
    ctx.Warp(103020100) #Kerning Subway
elif tp_option == 1:
    ctx.Warp(600000000) #NLC
