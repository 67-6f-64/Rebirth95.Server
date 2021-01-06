from collections import OrderedDict
import random

# gachapon script collection

ITEMS =	{ # odds : [itemids]
	60 : [2010000], # red apple
	30 : [2010009], # green apple kek
	9  : [3010006, 3010005, 3010004, 3010003, 3010002, 3010000, 3010011, 3010015, 2340000, 2022282, 2012008, 2022179], # more chairs, then items
	1  : [3010184, 3010177, 3010175, 3010174, 3010173, 3010172, 3010171, 3010170, 3010169, 3010168, 3010161, 3010156, 3010155, 3010152] #chairs
}

GACHAPON_ITEM = 5220000

def canplay():
	if ctx.PlayerLevel < 15:
		ctx.SayOk("You need to be at least Level 15 in order to use Gachapon.")
		return False
	if not ctx.HasItem(GACHAPON_ITEM):
		ctx.SayOk("Here's Gachapon!")
		return False
	if ctx.CountFreeSlots(1) <= 0 or ctx.CountFreeSlots(2) <= 0 or ctx.CountFreeSlots(3) <= 0 or ctx.CountFreeSlots(4) <= 0 or ctx.CountFreeSlots(5) <= 0:
		ctx.SayOk("Please make room in your item inventory and then try again.")
		return False
	if ctx.AskYesNo("You may use Gachapon. Would you like to use your Gachapon ticket?"):
		return True
	else:
		ctx.SayOk("Please come again!")
		return False

def start():
	if canplay():
		rand = random.randint(0, 100)
		
		for key in OrderedDict(ITEMS):
			if rand > key:
				ret = random.choice(ITEMS[key])
				ctx.RemoveItem(GACHAPON_ITEM)
				ctx.AddItem(ret)
				ctx.SayOk("You have obtained #b#t" + str(ret) + "##k.")
				break
				
start()