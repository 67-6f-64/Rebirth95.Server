
TOWN = 0
NAME = 1
FUNCTION = 2

res = {
	"skin_orbis1" : 
		{
			TOWN : "Orbis",
			NAME : "Romi",
		},
	"skin_ludi1" :
		{
			TOWN : "Ludibrium",
			NAME : "Gina",
		},
	"skin_henesys1" :
		{
			TOWN : "Henesys",
			NAME : "Ms. Tan",
		},
	"skin_mureung1" :
		{
			TOWN : "Mu Lung",
			NAME : "Naran",
		},
	"skin_ariant1" :
		{
			TOWN : "Ariant",
			NAME : "Lila",
		},
	"skin_edel1" :
		{
			TOWN : "Edelstein",
			NAME : "Mel Nomie",
		},
	"NLC_Skin" :
		{
			TOWN : "New Leaf City",
			NAME : "Miranda",
		},
	"skin_sg1" :
		{
			TOWN : "CBD",
			NAME : "Xan",
		},
	"skin_taiwan1" :
		{
			TOWN : "Taiwan",
			NAME : "Dr. Lee",
		},
}

options = [0, 1, 2, 3, 4]

coupon = 5153015

sayMsg = "Well, hello! Come in, welcome to the {} clinic! I'm {} and I'm in charge of the skin care here. Would you like to have a firm and healthy-looking skin like mine? With #r#t{}##k, you can let us take care of everything and have the skin you've always wanted~! ".format(res[script.ScriptName][0], res[script.ScriptName][1], str(coupon))

avatarMsg = "With our specialized machine, you can see in advance how it will look after treatment. What type of skin treatment would you like to do? Choose the style of your preference..."

successMsg = "Here is the mirror, look at it! What do you think? Is your skin not as beautiful and resplendent as mine? Hehehehe~ I'm sure you are enjoying it. Come back again~"

internalFailMsg = "Sorry, there seems to be a problem with the procedure. Please check back later."

itemMissingFailMsg = "Um... you don't have the skin care coupon you need to get the treatment. Sorry, but we can't do it."

ctx.SayNext(sayMsg)
ret = ctx.AskAvatar(avatarMsg, coupon, options)

if ret == 0:
	ctx.SayOk(successMsg)
elif ret == 1:
	ctx.SayOk(internalFailMsg)
elif ret == 2:
	ctx.SayOk(itemMissingFailMsg)