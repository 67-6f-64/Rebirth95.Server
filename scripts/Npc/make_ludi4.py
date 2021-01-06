# Rydole
# Ludibrium: Toy Factory < Aparatus Room > (220020600)
# Refining NPC: Level 30 - 50 weapons - Stimulator allowed

FirstMessage = "Ah, you've found me! I spend most of my time here, working on weapons to make for travellers like yourself. Did you have a request?#b"
OptionArray = ["What's a stimulator?", "Create a Warrior weapon", "Create a Bowman weapon", "Create a Magician weapon", "Create a Thief weapon", "Create a Warrior weapon with a Stimulator", "Create a Bowman weapon with a Stimulator", "Create a Magician weapon with a Stimulator", "Create a Thief weapon with a Stimulator"]

for (key, value) in enumerate(OptionArray):
    FirstMessage += "\r\n#L" + str(key)

result = ctx.AskMenu(FirstMessage)