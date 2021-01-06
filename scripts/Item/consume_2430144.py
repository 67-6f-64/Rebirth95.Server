# == Copy-paste all the data from the GM Handtool into the top of the script

# Item ID: 2430144
# Item Name: Secret Mastery Book
# Description: Use it, and it will change into a Skill Mastery Book. However, you won't know what Skill Mastery Book it will become until you use it.
# Item Data: info.price=1,  info.slotMax=1,  spec.script=consume_2430144,  spec.npc=9010010  

# == define all constant variables
SECRET_MASTERY_BOOK = 2430144  # you can directly reference this value through ctx.ScriptItemID, im defining it for the example

# == adding a script entry point function is not required but its good for organization
#    and makes code writing easier because you can exit the script at any point which
#    isnt possible when outside of a function
def start():
    # no need to check if the player has the script item, it has already been validated before the script is executed
    
    if ctx.AddRandomMasteryBook():
        ctx.RemoveFrom(SECRET_MASTERY_BOOK, ctx.ScriptItemPOS)
    else:
        ctx.SystemMessage("Please make space in your consume inventory..")

# call script entry point -> this needs to be at the end
start()