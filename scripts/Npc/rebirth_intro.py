# Maggie
# Starting NPC for Alpha
# Assigns class and starting stats
# Warps to six path crossway
# Only works in starting map ( 180000001 )

INTRO_MAP = 180000001

WARP_OUT = 104020000  # six path crossway
WARP_OUT_PORTAL = 1

HPMPWEAPON = {
    100: [500, 250, 1302007],
    1100: [500, 250, 1302007],
    2100: [500, 250, 1442039],
    3500: [500, 250, 1492065],

    200: [300, 500, 1372005],
    1200: [300, 500, 1372005],
    2200: [300, 500, 1372062],
    3200: [300, 500, 1382054],

    300: [400, 300, 1452002],
    1300: [400, 300, 1452002],
    3300: [400, 300, 1452002],

    400: [400, 300, 1472000, 1332102],
    1400: [400, 300, 1472000],

    500: [400, 300, 1482000, 1492014],
    1500: [400, 300, 1482000]
}

BULLETS = 2330000
STARS = 2070000
KATARA = 1342000
CROSSBOW_ARROW = 2061000  # slotmax is 2000
BOW_ARROW = 2060000  # slotmax is 2000
CROSSBOW = 1462047

CLASSES = {
    100: "Warrior",
    200: "Magician",
    300: "Bowperson",  # kek
    400: "Thief",
    500: "Pirate",
    1100: "Dawn Warrior",
    1200: "Blaze Wizard",
    1300: "Wind Archer",
    1400: "Night Walker",
    1500: "Thunder Breaker",
    2100: "Aran",
    2200: "Evan",
    3200: "Battle Mage",
    3300: "Wild Hunter",
    3500: "Mechanic"
}

def dostuff(sel):
    ctx.SetLevel(10)
    ctx.SetJob(sel)
    ctx.SetAP(10 + (ctx.PlayerLevel * 5))
    ctx.SetSP(3)
    ctx.SetMaxHP(HPMPWEAPON[sel][0])
    ctx.SetMaxMP(HPMPWEAPON[sel][1])
    ctx.AddItem(HPMPWEAPON[sel][2])

    if sel == 400 or sel == 500:
        ctx.AddItem(HPMPWEAPON[sel][3])

    if sel == 400 or sel == 500 or sel == 1400:
        ctx.AddItem(STARS)

    if sel == 500 or sel == 3500:
        ctx.AddItem(BULLETS)

    if sel == 400 and ctx.PlayerSubJob == 1:  # dual blade
        ctx.AddItem(KATARA)

    if sel == 3300 or sel == 300 or sel == 1300:
        ctx.AddItem(CROSSBOW_ARROW, 2000)
        ctx.AddItem(BOW_ARROW, 2000)
        ctx.AddItem(CROSSBOW)

    ctx.Warp(WARP_OUT, WARP_OUT_PORTAL)
    ctx.Say(
        "Have fun! You can speak to Furball in the Free Market for some freebies! Taco can also warp you to some "
        "cool training places! Press the Maple button in the status bar to access the Rebirth menu! "
        "Type @help for a list of commands. Don't forget to report bugs in the Discord :)")


if ctx.PlayerMapId == INTRO_MAP:
    ctx.SayNext("Welcome to Rebirth!\r\nPlease excuse the mess, we are still getting things in order.")
    if ctx.PlayerSubJob == 1:  # dual blade
        ctx.SayNext("I see you are a Dual Blade, welcome!")
        dostuff(400)

    else:
        selText = "What class would you like to play as?"
        for i in CLASSES:
            selText += "\r\n#L" + str(i) + "#" + CLASSES[i] + "#l"
        sel = ctx.AskMenu(selText)
        if sel in CLASSES and ctx.AskYesNo("Are you sure you want to pick " + CLASSES[sel] + "?"):
            dostuff(sel)
else:
    ctx.SayOk("Disabled")
