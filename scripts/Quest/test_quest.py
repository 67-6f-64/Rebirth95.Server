# Quest Script Template
# These are different from other script types because they require at least one
#   of two functions called start() and end().
# Some quests only have start(), some quests only have end(), and some quests have both.
# This is dictated by the WZ files. In each data point in Quest.wz, there is a node
#   called Check. This node contains two more nodes, 0 and 1 - if the quest requires a 
#   start script there will be a node called startscript inside the 0 node, and if it
#   requires an end script there will be a node called endscript in node 1. 
# The client will send a scripted quest request for the scripts it needs, and we interpret
#   those requests and filter them to the correct python script file.

# For quest script documentation, 

# For code review purposes, include both a start() and end() function in every script,
#   even if the quest only requires one of the two functions.
# Here is an example of a quest with only one script:

# quest doesnt have a start script, so we include an empty start() function
def start():
    pass  # pass because our function is empty
    
# quest does have an end script, this is where our code will go
def end():
    # code here