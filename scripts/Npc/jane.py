# Jane The Alchemist

nl = "\r\n"

items = [
    [2000002, 310],
    [2022003, 1060],
    [2022000, 1600],
    [2001000, 3120],
]

recover = [
    "300 HP.",
    "1000 HP.",
    "800 MP",
    "1000 HP and MP."
]

if ctx.IsQuestCompleted(2013):
    ctx.SendNext("It's you... thanks to you I was able to get a lot done. \
        Nowadays I've been making a bunch of items. \
        If you need anything let me know.")

elif ctx.IsQuestCompleted(2010):
    ctx.SendNext("You don't seem strong enough to be able to purchase my potion...")

    ctx.dispose()

else:
    ctx.SendOk("My dream is to travel everywhere, much like you. \
        My father, however, does not allow me to do it, because he thinks it's very dangerous. \
        He may say yes, though, if I show him some sort of a proof that I'm not the weak girl that he thinks I am...")

    ctx.dispose()

selection = ctx.SendSimple(f"Which item would you like to buy?#b{nl} \
    { f'{nl}'.join([f'#L{i}##i{items[i][0]}# (Price : {items[i][1]}mesos)#l' for i, _ in enumerate(items)])}")

amount = ctx.SendGetNumber(f"You want #b#t{items[selection][0]}##k?\
    #t{items[selection][0]}# allows you to recover {recover[selection]} \
    How many would you like to buy?", 1, 1, 100)

buy = ctx.SendYesNo(f"Will you purchase #r{amount}#k #b#t{items[selection][0]}#(s)#k? \
    #t{items[selection][0]}# costs {items[selection][1]} mesos for one, so the total comes to be #r{items[selection][1] * amount}#k mesos.")

if not buy:
    ctx.dispose()

if ctx.GetMeso() < items[selection][1] * amount:
    ctx.SendNext(f"Are you lacking mesos by any chance? \
        Please check and see if you have an empty slot available in your etc. inventory, and that you have at least #r{items[selection][1] * amount}#k mesos with your.")
    ctx.dispose()

if ctx.CanHold(items[selection][0]):
    ctx.GainMeso(-1 * items[selection][1] * amount)
    ctx.GainItem(item[selection][0], amount)
    ctx.SendNext("Thank you for coming. \
        Stuff here can always be made so if you need anything, please come again.")

else:
    ctx.SendNext("Please check and see if you have an empty slot available in your etc. inventory.")

ctx.dispose()
