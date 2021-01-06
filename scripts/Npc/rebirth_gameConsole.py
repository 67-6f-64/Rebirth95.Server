# Game Console
# 910000000 - Free Market Entrance
# Damage Skin NPC

DMGSKINS = {
    0 : "Normal Damage (No Skin)",
    12 : "Gentle Springtime Breeze Damage Skin",
    13 : "Singles Army Damage Skin",
    14 : "Reminiscence Damage Skin",
    15 : "Orange Mushroom Damage Skin",
    16 : "Crown Damage Skin",
    28 : "Pink Bean Damage Skin",
    31 : "Keybaord Warrior Damage Skin",
    1005 : "Jett Damage Skin",
    1017 : "Black and White Damage Skin",
    1031 : "Black Heaven Damage Skin",
    1032 : "Orchid Damage Skin",
    1033 : "Lotus Damage Skin",
    1354 : "8-Bit Damage Skin",
}

def start():
  
    selText = "Hello! I am managing Damage Skins for Alpha. What will it be?\r\n"
  
    for i in DMGSKINS:    
        selText += "\r\n#L" + str(i) + "#" + DMGSKINS[i] + "#l"
        
    sel = ctx.AskMenu(selText)
    
    if sel not in DMGSKINS:
        ctx.SayOk("Invalid Selection.")
        return
  
    ctx.SetDamageSkin(sel)
    ctx.SayOk("Your damage skin has been set. It will be visible the next time you change maps!")

start()
#ctx.SayOk("#FEffect/DamageSkin.img/15/NoRed1/0#")