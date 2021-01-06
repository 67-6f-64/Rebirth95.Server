# hair npc collection
# stat changing and item validation and removal are done in the AskAvatar/AskMembershipAvatar functions

HAIR_IDS = {
	"hair_henesys1" : [
						[ 30030, 30020, 30000, 30480, 30310, 30330, 30060, 30150, 30410, 30210, 30140, 30120, 30200 ], # male hair styles
						[ 31050, 31040, 31000, 31700, 31150, 31310, 31300, 31160, 31100, 31410, 31030, 31080, 31070 ], # female hair styles
					],
	"hair_henesys2" : [
						[ 30000, 30020, 30030, 30060, 30120, 30140, 30150, 30200, 30210, 30310, 30330, 30410 ], # male hair styles
						[ 31000, 31030, 31040, 31050, 31080, 31070, 31100, 31150, 31160, 31300, 31310, 31410 ], # female hair styles
					],
	"hair_kerning1" : [
						[ 30030, 30020, 30000, 30780, 30130, 30350, 30190, 30110, 30180, 30050, 30040, 30160 ], # male hair styles
						[ 31050, 31040, 31000, 31760, 31060, 31090, 31330, 31020, 31130, 31120, 31140, 31010 ], # female hair styles
					],
	"hair_kerning2" : [
						[ 30000, 30020, 30030, 30040, 30050, 30110, 30130, 30160, 30180, 30190, 30350, 30610, 30440, 30400 ], # male hair styles
						[ 31000, 31010, 31020, 31040, 31050, 31060, 31090, 31120, 31130, 31140, 31330, 31700, 31620, 31610 ], # female hair styles
					],
	"hair_orbis1" : [
						[ 30030, 30020, 30000, 30490, 30230, 30260, 30280, 30240, 30290, 30270, 30340 ], # male hair styles
						[ 31040, 31000, 31670, 31250, 31220, 31260, 31240, 31110, 31270, 31030, 31230 ], # female hair styles
					],
	"hair_orbis2" : [
						[ 30000, 30020, 30030, 30230, 30240, 30260, 30270, 30280, 30290, 30340, 30610, 30440, 30400 ], # male hair styles
						[ 31000, 31030, 31040, 31110, 31220, 31230, 31240, 31250, 31260, 31270, 31320, 31700, 31620, 31610 ], # female hair styles
					],
	"hair_ludi1" : [
						[ 30030, 30020, 30000, 30660, 30250, 30190, 30150, 30050, 30280, 30240, 30300, 30160 ], # male hair styles
						[ 31040, 31000, 31550, 31150, 31280, 31160, 31120, 31290, 31270, 31030, 31230, 31010 ], # female hair styles
					],
	"hair_ludi2" : [
						[ 30540, 30640, 30680, 30250, 30190, 30150, 30050, 30280, 30240, 30300, 30160, 30650 ], # male hair styles
						[ 31540, 31640, 31600, 31150, 31280, 31160, 31120, 31290, 31270, 31030, 31230, 31010, 31680 ], # female hair styles
					],
	"hair_wedding1" : [
						[ 30580, 30590, 30280, 30670, 30410, 30200, 30050, 30230, 30290, 30300, 30250 ], # male hair styles
						[ 31580, 31590, 31310, 31200, 31150, 31160, 31020, 31260, 31230, 31220, 31110 ], # female hair styles
					],
	"hair_wedding2" : [
						[ 30570, 30450, 30410, 30200, 30050, 30230, 30290, 30300, 30250, 30690 ], # male hair styles
						[ 31570, 31480, 31150, 31160, 31020, 31260, 31230, 31220, 31110, 31490 ], # female hair styles
					],
	"hair_wedding3" : [
						[ 30032, 30020, 30000, 30132, 30192, 30240, 30162, 30270, 30112 ], # male hair styles
						[ 31050, 31040, 31030, 31001, 31070, 31310, 31091, 31250, 31150 ], # female hair styles
					],
	"NLC_HairVip" : [
						[ 30730, 30280, 30220, 30410, 30200, 30050, 30230, 30160, 30110, 30250 ], # male hair styles
						[ 31730, 31310, 31470, 31150, 31160, 31300, 31260, 31220, 31410, 31270 ], # female hair styles
					],
	"NLC_HairExp" : [
						[ 30400, 30360, 30440, 30410, 30200, 30050, 30230, 30160, 30110, 30250 ], # male hair
						[ 31560, 31720, 31450, 31150, 31160, 31300, 31260, 31220, 31410, 31270 ], # female hair
					],
	"hair_mureung1" : [
						[ 30600, 30750, 30250, 30150, 30240, 30350, 30180, 30300, 30270, 30160 ], # male hair
						[ 31460, 31040, 31180, 31310, 31300, 31160, 31030, 31250, 31220, 31230 ], # female hair
					],
	"hair_mureung2" : [
						[ 30700, 30720, 30420, 30250, 30150, 30240, 30350, 30180, 30300, 30270, 30160 ], # male hair
						[ 31690, 31210, 31170, 31310, 31300, 31160, 31030, 31250, 31220, 31230 ], # female hair
					],
	"hair_shouwa1" : [
						[ 30000, 30120, 30140, 30190, 30210, 30270, 30290, 30360, 30220, 30370, 30400, 30440, 30510 ], # male hair
						[ 31030, 31050, 31070, 31100, 31120, 31130, 31250, 31340, 31210, 31350, 31400, 31440, 31520 ], # female hair
					],
	"hair_shouwa2" : [
						[ 30000, 30120, 30140, 30190, 30210, 30270, 30290, 30360, 30220, 30370, 30400, 30440, 30510 ], # male hair
						[ 31030, 31050, 31070, 31100, 31120, 31130, 31250, 31340, 31210, 31350, 31400, 31440, 31520 ], # female hair
					],
	# below are not in BMS
	"hair_royal" : [
						[ ], # male hair
						[ ], # female hair
					],
	"hair_ariant1" : [
						[ 30250, 33030, 30330, 30150, 30900, 30170, 30180, 30820, 30410, 30460 ], # male hair
						[ 31400, 31090, 31190, 31620, 31040, 31420, 31330, 31340, 31660, 31950 ], # female hair
					],
	"hair_ariant2" : [
						[ 30250, 33030, 30330, 30150, 30900, 30170, 30180, 30820, 30410, 30460, 30430, 30800, 30680 ], # male hair
						[ 31400, 31090, 31190, 31620, 31420, 31330, 31340, 31660, 31950, 31520, 31650, 31780, 34000 ], # female hair
					],
	"hair_edel1" : [
						[ 30350, 30480, 33030, 33190, 30760, 30330, 30560, 30730, 30370, 30470, 30460 ], # male hair
						[ 31310, 31490, 31260, 31130, 31160, 31510, 31230, 31320, 31560, 31950, 34190, 31530 ] # female hair
					],
	"hair_sg1" : [
						[ ], # male hair
						[ ], # female hair
					],
	"hair_sg2" : [
						[ ], # male hair
						[ ], # female hair
					],
	"hair_taiwan1" : [
						[ ], # male hair
						[ ], # female hair
					],
	"hair_taiwan2" : [
						[ ], # male hair
						[ ], # female hair
					]
}

# dialog selection keys
DIALOG_MENU = 0
DIALOG_AVATAR = 1
DIALOG_ASK = 2
DIALOG_RESPONSE_CUT = 3
DIALOG_RESPONSE_COLOR = 4
DIALOG_OPTION_CUT = 5
DIALOG_OPTION_COLOR = 6
DIALOG_OPTION_MEMBER = 7
DIALOG_OPTION_NO = 8
ERROR = 9

# dialog attribute keys
NAME = 0
ROLE = 1
TOWN = 2
FUNCTION = 3

ASSISTANT = "assistant"
HEAD = "head"

REG_CUT_COUPON = 5150052
REG_DYE_COUPON = 5151035

VIP_CUT_COUPON = 5150053
VIP_DYE_COUPON = 5151036

# returns gender-specific hair styles based on current hair color and script name
def get_hair_styles():
	curHairColor = ctx.PlayerHair % 10
	index = 0 if ctx.PlayerGender == 0 else 1
	return [baseHairStyle + curHairColor for baseHairStyle in HAIR_IDS[script.ScriptName][index]]

# returns hair colors based on the script name
def get_hair_colors():
	currentBaseHair = ctx.PlayerHair - (ctx.PlayerHair % 10)
	return [currentBaseHair + hairColor for hairColor in [ 0, 1, 2, 3, 4, 5, 6, 7 ]] # all colors

def get_attributes():
	res = {
		"hair_henesys1" : {
			NAME : "Natalie",
			ROLE : HEAD,
			TOWN : "Henesys",
		},
		"hair_henesys2" : {
			NAME : "Brittany",
			ROLE : ASSISTANT,
			TOWN : "Henesys",
		},
		"hair_kerning1" : {
			NAME : "Don Giovanni",
			ROLE : HEAD,
			TOWN : "Kerning City",
		},
		"hair_kerning2" : {
			NAME : "Andre",
			ROLE : ASSISTANT,
			TOWN : "Kerning City",
		},
		"hair_orbis1" : {
			NAME : "Mino",
			ROLE : "Owner",
			TOWN : "Orbis",
		},
		"hair_orbis2" : {
			NAME : "Rinz",
			ROLE : ASSISTANT,
			TOWN : "Orbis",
		},
		"hair_ludi1" : {
			NAME : "Miyu",
			ROLE : HEAD,
			TOWN : "Ludibrium",
		},
		"hair_ludi2" : {
			NAME : "Mini",
			ROLE : ASSISTANT,
			TOWN : "Ludibrium",
		},
		"hair_wedding1" : {
			NAME : "Julius Styleman",
			ROLE : HEAD,
			TOWN : "Amoria",
		},
		"hair_wedding2" : {
			NAME : "Seamus",
			ROLE : ASSISTANT,
			TOWN : "Amoria",
		},
		"hair_wedding3" : {
			NAME : "Claudia",
			ROLE : "Apprenctice",
			TOWN : "Amoria",
		},
		"NLC_HairVip" : {
			NAME : "Mani",
			ROLE : HEAD,
			TOWN : "NLC",
		},
		"NLC_HairExp" : {
			NAME : "Ari",
			ROLE : ASSISTANT,
			TOWN : "NLC",
		},
		"hair_mureung1" : {
			NAME : "Luo",
			ROLE : HEAD,
			TOWN : "Mu Lung",
		},
		"hair_mureung2" : {
			NAME : "Lilishu",
			ROLE : ASSISTANT,
			TOWN : "Mu Lung",
		},
		"hair_shouwa1" : {
			NAME : "Tepei",
			ROLE : HEAD,
			TOWN : "Showa",
		},
		"hair_shouwa2" : {
			NAME : "Midori",
			ROLE : ASSISTANT,
			TOWN : "Showa",
		},
		"hair_royal" : {
			NAME : "Big Headward",
			ROLE : HEAD,
			TOWN : "Royal",
		},
		"hair_ariant1" : {
			NAME : "Mazra",
			ROLE : HEAD,
			TOWN : "Ariant",
		},
		"hair_ariant2" : {
			NAME : "Shati",
			ROLE : ASSISTANT,
			TOWN : "Ariant",
		},
		"hair_edel1" : {
			NAME : "Fabio",
			ROLE : HEAD,
			TOWN : "Edelstein",
		},
		"hair_sg1" : {
			NAME : "Eric",
			ROLE : HEAD,
			TOWN : "CBD",
		},
		"hair_sg2" : {
			NAME : "Jimmy",
			ROLE : ASSISTANT,
			TOWN : "CBD",
		},
		"hair_taiwan1" : {
			NAME : "Niki",
			ROLE : HEAD,
			TOWN : "Taiwan",
		},
		"hair_taiwan2" : {
			NAME : "Julie",
			ROLE : ASSISTANT,
			TOWN : "Taiwan",
		}
	}
	return res[script.ScriptName]

def is_random_outcome():
	return "2" in script.ScriptName or script.ScriptName == "NLC_HairExp"

def get_coupons():
	return [ REG_CUT_COUPON, REG_DYE_COUPON ] if is_random_outcome() else [ VIP_CUT_COUPON, VIP_DYE_COUPON ]

def get_dialog(selection, option):
	attr = get_attributes()
	coupons = get_coupons()
	res = {
		DIALOG_MENU : "I'm {name}, the {role} of the {town} hair salon. If you have #r#t{cut}##k or #r#t{dye}##k, allow me to take care of your hairdo. Please choose the one you want.\r\n#b#L0#Cut your hair with a #t{cut}##l\r\n#L1#Dye your hair with a #t{dye}##l\r\n".format(name=attr[NAME], role=attr[ROLE], town=attr[TOWN], cut=coupons[0], dye=coupons[1]),
		DIALOG_ASK : "If you use the regular coupon your hair will change RANDOMLY. Are you going to use the coupon and really change your hairstyle?",
		DIALOG_OPTION_CUT : "I can totally change up your hairstyle and make it look so good. Why don't you change it up a bit? If you have #b#t{cut}##k I'll change it for you. Choose the one to your liking~".format(cut=coupons[0]),
		DIALOG_OPTION_COLOR : "I can totally change your haircolor and make it look so good. Why don't you change it up a bit? With #b#t{dye}##k I'll change it for you. Choose the one to your liking.".format(dye=coupons[1]),
		DIALOG_OPTION_NO : "I understand...think about it, and if you still feel like changing come talk to me.",
		DIALOG_RESPONSE_CUT : [
						"Check it out!! What do you think? Even I think this one is a work of art! AHAHAHA. Please let me know when you want another haircut, because I'll make you look good each time!",
						"I'm sorry. Looks like we have a slight problem changing your hairdo. Please come back in a little bit.",
						"Hmmm...it looks like you don't have our designated coupon...I'm afraid I can't give you a haircut without it. Please come back when you have a #r#t{cut}##k.\r\nI'm sorry.".format(cut=coupons[0])
					],
		DIALOG_RESPONSE_COLOR : 
					[
						"Check it out!! What do you think? Even I think this one is a work of art! AHAHAHA. Please let me know when you want to dye your hair again, because I'll make you look good each time!",
						"I'm sorry. Looks like we have a slight problem dying your hair. Please come back in a little bit.",
						"Hmmm...it looks like you don't have our designated coupon...I'm afraid I can't dye your hair without it. Please come back when you have a #r#t{dye}##k.\r\nI'm sorry.".format(dye=coupons[1])
					],
		ERROR : "NPC error, please take a screenshot of this message and post it on the discord. Thanks!"
	}
	if option is None: return res[selection]
	else: return res[selection][option]

def start():
	if script.ScriptName is "hair_wedding3": return wedding_hair()
	if script.ScriptName is "hair_royal": return "Soon you'll be able to see me to get the freshest looks from the future!"
	
	dialog = get_dialog(DIALOG_MENU, None)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
		
	selectHair = ctx.AskMenu(dialog)
	if selectHair > 1 or selectHair < 0: return get_dialog(ERROR, None)
	
	if not is_random_outcome():
		res = {
			0 : haircut,
			1 : dye_hair
		}
	else:
		res = {
			0 : random_haircut,
			1 : random_dye_hair
		}
	
	if selectHair == 1 and (ctx.PlayerHair == 30430 or ctx.PlayerHair == 31430): 
		return "You won't be able to use the Hair Salon Coupon for Hair Color if you are sporting a Skin Head."
	
	return res[selectHair](get_coupons()[selectHair])

def haircut(coupon):
	dialog = get_dialog(DIALOG_OPTION_CUT, None)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	res = ctx.AskAvatar(dialog, coupon, get_hair_styles())
	
	dialog = get_dialog(DIALOG_RESPONSE_CUT, res)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	return dialog

def dye_hair(coupon):
	dialog = get_dialog(DIALOG_OPTION_COLOR, None)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	res = ctx.AskAvatar(dialog, coupon, get_hair_colors())
	
	dialog = get_dialog(DIALOG_RESPONSE_COLOR, res)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	return dialog

def random_haircut(coupon):
	if not ctx.AskYesNo(get_dialog(DIALOG_ASK, None)) > 0: return get_dialog(DIALOG_OPTION_NO, None)
	
	res = ctx.MakeRandAvatar(coupon, get_hair_styles())
	
	dialog = get_dialog(DIALOG_RESPONSE_CUT, res)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	return dialog
	
def random_dye_hair(coupon):
	if not ctx.AskYesNo(get_dialog(DIALOG_ASK, None)) > 0: return get_dialog(DIALOG_OPTION_NO, None)

	res = ctx.MakeRandAvatar(coupon, get_hair_colors())
	
	dialog = get_dialog(DIALOG_RESPONSE_COLOR, res)
	if len(dialog) <= 0: return get_dialog(ERROR, None)
	
	return dialog

def wedding_hair():
	if ctx.PlayerGender == 0:
		styles = HAIR_IDS[script.ScriptName][0]
	else:
		styles = HAIR_IDS[script.ScriptName][1]
	
	res = ctx.AskYesNo("Are you ready, ready for us to do an amazing hairstyle? I think you are! Just say the right word and we'll start!")
	
	if res > 0:
		ctx.SayOk("Here we go!")
	else:
		return "Okay, I'll give you a minute."
	
	res = [
		"Not bad, I would say ... I knew that the books I studied would come in handy ...",
		"I'm sorry. It looks like we have a little problem changing your hairstyle. Please come back soon.",
		"Um ... Are you sure you have the free coupon right? Sorry, but no haircut without it."
	]
	
	# uses free wedding hair coupon
	return res[ctx.MakeRandAvatar(4031528, styles)]
	
ctx.SayOk(start())








