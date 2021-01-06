# Furball
# 910000000 - Free Market Entrance
# Item NPC

ITEMS = {
    5062000 : "Miracle Cube",
    2049401 : "Potential Scroll",
    2049400 : "Advanced Potential Scroll",
    2049301 : "Enhancement Scroll",
    2049300 : "Advanced Enhancement Scroll",
    2049100 : "Chaos Scroll",
    2049116 : "Miraculous Chaos Scroll",   
    
    1112920 : "Zombie Army Ring",    
    1112439 : "VIP Ring",    
    1112428 : "Explorer's Critical Ring",  
    
    1022058 : "Brown Raccoon Mask",
    1022060 : "White Raccoon Mask",
    1022082 : "Spectrum Goggles",
    
    1142247 : "Time Traveller Medal",
    1122014 : "Silver Deputy Star",
    
    1122059 : "Mark of Naricain",
    1142249 : "I'm a Lucky Guy",
    
    1032084 : "VIP Earring",
    
    1082149 : "Brown Work Gloves",
    1082223 : "Stormcaster Gloves",
    1082230 : "Glitter Gloves",
    
    1050100 : "Man Bathrobe",
    1051098 : "Women Bathrobe",
    
    1072238 : "Violet Snowshoes",
    
    1102041 : "Pink Adventurer Cape",
    1102042 : "Purple Adventurer Cape",
    
    1332025 : "Maple Wagner",
    1382009 : "Maple Staff",
    1452016 : "Maple Bow",
    1462019 : "Maple CrossBow",
    1442051 : "Maple Karstan",
    1472030 : "Maple Claw",
    1492020 : "Maple Gun",
    1482020 : "Maple Knuckle",
    
    1302147 : "VIP One-Handed Sword",
    1312062 : "VIP One-Handed Axe",
    1322090 : "VIP One-Handed Blunt Weapon",
    1332120 : "VIP Dagger (LUK)",
    1342033 : "VIP Katara",
    1372078 : "VIP Wand",
    1382099 : "VIP Staff",
    1402090 : "VIP Two-Handed Sword",
    1432081 : "VIP Spear",
    1442111 : "VIP Polearm",
    1452106 : "VIP Bow",
    1462091 : "VIP Crossbow",
    1472117 : "VIP Claw",
    1492079 : "VIP Gun",
    1482079 : "VIP Knuckle",
    
    1092074 : "VIP Warrior Shield",
    1092079 : "VIP Magician Shield",
    1092084 : "VIP Thief Shield",    
    1092030 : "Maple Shield",
    
    2046204 : "Scroll for Armor for STR 50%",
    2046205 : "Scroll for Armor for INT 50%",
    2046206 : "Scroll for Armor for DEX 50%",
    2046207 : "Scroll for Armor for LUK 50%",
    
    2040834 : "Scroll for Gloves for ATT 50%",
    
    2046002 : "Scroll for One-Handed Weapon for ATT 50%",
    2046003 : "Scroll for One-Handed Weapon for MAGIC ATT 50% ",
    
    2046102 : "Scroll for Two-Handed Weapon for ATT 50%",
    2046103 : "Scroll for Two-Handed Weapon for MAGIC ATT 50%",
}

selText = "Hello! I will spoonfeed you temporarily for Alpha. What would you like?"

for i in ITEMS:
    selText += "\r\n#L" + str(i) + "#" + ITEMS[i] + "#l"

sel = ctx.AskMenu(selText)

amount = 1

if sel >= 2000000:
    amount = 25

ctx.AddItem(sel, amount)