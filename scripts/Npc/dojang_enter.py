# So Gong 
# NPC ID: 2091005
# Exists in the dojo starting map (Mu Lung Dojo Hall) and
#   periodically throughout the dojo run.

# Template: https://bbb.hidden-street.net/party-quest/mu-lung-dojo

DOJO_ENTRANCE_MAP = 925020001
DOJO_EXIT_MAP = 925020002
DOJO_WIN_MAP = 925020003
DOJO_TRAINING_MAP = 925020010
DOJO_FIRST_STAGE_MAP_NORMAL = 925020100
# DOJO_FIRST_STAGE_MAP_HARD = 925030100  # invalid map.. idk
DOJO_POINTS_QID = 8000
WEATHER_EFFECT_ID = 5120024
DOJO_EMBLEM_ID = 4001620
DOJO_MIN_LEVEL = 25

def start():
    global DOJO_ENTRANCE_MAP
    
    if ctx.PlayerMapId == DOJO_ENTRANCE_MAP:
        proc_main_entrance()
    elif is_dojo_rest_map():
        proc_dojo_rest_map()
    else:
        proc_dojo_boss_map()


def proc_main_entrance():
    ret = ctx.AskMenu("My master is the strongest person in Mu Lung, and YOU wish to challenge HIM? Just don't regret it later.\r\n"\
    "#b#L0#I'll challenge myself to Mu Lung Dojo.#l\r\n"\
    "#L1#I'll challenge my party to Mu Lung Dojo.#l\r\n"\
    "#L2#I want to receive Mu Gong's Belt.\r\n"\
    "#L3#I want to see what rewards I can get from Mu Lung Dojo.\r\n"\
    "#L4#What's Mu Lung Dojo?\r\n")
    
    switch = [
        challenge_dojo_solo,
        challenge_dojo_party,
        try_give_belt,
        give_belt_info,
        dojo_info
    ]
    
    if ret < 0 or ret >= len(switch): return
    
    switch[ret]()
    
    
def challenge_dojo_solo():
    global DOJO_FIRST_STAGE_MAP_NORMAL, DOJO_MIN_LEVEL
    if ctx.PlayerLevel < DOJO_MIN_LEVEL:
        ctx.SayOk(f"Uh oh, it looks like you're not high enough level :(\r\n"\
                    "Come back when you're at least level {DOJO_MIN_LEVEL}.")
    elif ctx.PlayerInParty:
        ctx.SayOk("You can't enter in a party! Stand on your own strength!")
    elif ctx.AskYesNo("Are you sure you want to challenge the dojo?"):
        ctx.WarpToInstance(DOJO_FIRST_STAGE_MAP_NORMAL, 0)
    else:
        ctx.SayOk("Come and talk to me when you are ready.")


def challenge_dojo_party():
    global DOJO_FIRST_STAGE_MAP_NORMAL, DOJO_MIN_LEVEL
    if ctx.PlayerLevel < DOJO_MIN_LEVEL:
        ctx.SayOk(f"Uh oh, it looks like you're not high enough level :(\r\n"\
                    "Come back when you're at least level {DOJO_MIN_LEVEL}.")
    elif ctx.PlayerPartyMembersInMap < 2:
        ctx.SayOk("Make sure you're in a party of at least two.")
    elif not ctx.PlayerPartyLeader:
        ctx.SayOk("Have your leader talk to me..")
    elif ctx.PlayerPartyLevelDifference > 30:
        ctx.SayOk("Make sure the level difference between party members is at most 30 levels.")
    elif ctx.AskYesNo("Are you ready? Your party will be escorted immediately."):
        ctx.WarpToInstance(DOJO_FIRST_STAGE_MAP_NORMAL, 0, True)
    else:
        ctx.SayOk("Talk to me when your party is ready to challenge the master.")
    
    
def try_give_belt():
    global DOJO_EMBLEM_ID
    if not ctx.AskYesNo(
        "I'll trade your hard earned Dojo Points for training belts."):
        ctx.SayOk("Come back when you wish to trade.")
        return
    
    base_belt = 1132000
    dojo_belt_prices = [200, 1800, 4000, 9200, 17000]
    
    response = "Which belt do you want?"
    
    for i in range(len(dojo_belt_prices)):
        response += f"\r\n#L{i}##i{base_belt + i}:# #b#t{base_belt + i}##k #r({dojo_belt_prices[i]} Dojo Points)#k#l"
    
    ret = ctx.AskMenu(response)
    
    if ret < 0 or ret > len(dojo_belt_prices): return
    
    desired_belt = base_belt + ret
    required_points = dojo_belt_prices[ret]
    
    if get_dojo_points() < required_points:
        ctx.SayOk("You don't have enough dojo points. Do try getting the weaker belts first.")
    elif not ctx.CanHoldItem(desired_belt):
        ctx.SayOk("Please make sure you have space in your equip inventory.")
    else:
        ctx.AddItem(desired_belt)
        add_dojo_points(-required_points)


def give_belt_info():
    ctx.SayOk("Under construction.")


def dojo_info():
    ctx.SayOk("Under construction.")


def proc_dojo_rest_map():
    if ctx.AskYesNo("Do you want to proceed to the next level?\r\nIn the future I will sell you items, but until then I wish you well.."):
        if ctx.PlayerPartyLeader:
            ctx.WarpToInstance(ctx.PlayerMapId + 100, 0, True)
        elif ctx.PlayerInParty:  # not party leader
            ctx.SayOk("Have your party leader speak to me..")
        else:
            ctx.WarpToInstance(ctx.PlayerMapId + 100, 0, False)
    else:
        ctx.SayOk("Talk to you soon.")


def proc_dojo_boss_map():
    global DOJO_EXIT_MAP
    if ctx.AskYesNo("Do you want to leave? Haha, I knew you weren't up for the challenge"):
        ctx.Warp(DOJO_EXIT_MAP, 1)
    else:
        ctx.SayOk("We'll see...")
    

def add_dojo_points(amount):
    global DOJO_POINTS_QID
    ctx.UpdateQuestRecordInternal(DOJO_POINTS_QID, str(get_dojo_points() + amount))


def get_dojo_points():
    global DOJO_POINTS_QID
    return int(ctx.GetQuestRecord(DOJO_POINTS_QID, str(0)))


def is_dojo_rest_map():
    stage = (ctx.PlayerMapId - 925020000) / 100
    return stage in [6, 12, 18, 24, 30, 36]
    '''
    return ctx.PlayerMapId in [
            925020600,
            925021200,
            925021800,
            925022400,
            925023000,
            925023600,
            
            925030600,
            925031200,
            925031800,
            925032400,
            925033000,
            925033600,
        ]
    '''

start()