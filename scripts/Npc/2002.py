# Name: Peter
# ID:   2002
# Map: Maple Road: Inside the Small Forest
# Map ID: 40000
# Function: Continues with beginner tutorial. <== TODO expand this.

ctx.SayNext(f"Uh oh, you have encountered an uncoded NPC with ID {ctx.Script.SpeakerTemplateID}.")
ctx.SayNextPrev("In order to proceed, I need to have proper instructions.")
ctx.SayNextPrev("Hint: I need to give you an apple.")

base_text = "Please enter my new script line by line.\r\nTo undo a line, enter #r/undo#k.\r\nTo finish, enter #g/finish#k\r\n"

script = []

while True:
    text = base_text

    for line in script:
        text += f"{line}\r\n"
        
    userinput = ctx.AskText(text)
    
    if len(userinput) <= 0:  # hax
        continue
    elif userinput.lower() == "/undo":  # TODO
        if len(script) <= 0:
            ctx.SayOk("No lines to undo.")
        else:
            del script[len(script) - 1]
    elif userinput.lower() == "/finish":
        break
    else:
        script.append(userinput)
    
finalscript = ""

for line in script:
    finalscript.append(f"{line}\r\n")

ctx.SayOk(f"Your script input is:\r\n\r\n{finalscript}")