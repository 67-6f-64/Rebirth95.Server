
FACE_IDS = { # item IDs required for the given selection
	"hair_henesys1" : [],
	"hair_henesys2" : [],
	"hair_kerning1" : [],
	"hair_kerning2" : [],
	"hair_orbis1" : [],
	"hair_orbis2" : [],
	"hair_ludi1" : [],
	"hair_ludi2" : [],
	"hair_wedding1" : [],
	"hair_wedding2" : [],
	"hair_wedding3" : [],
	"NLC_HairVip" : [5150031, 5151026, 5420001],
	"NLC_HairExp" : [5150030, 5151025],
	"hair_mureung1" : [],
	"hair_mureung2" : [],
	"hair_shouwa1" : [],
	"hair_shouwa2" : [],
	"hair_royal" : [],
	"hair_ariant1" : [],
	"hair_ariant2" : [],
	"hair_edel1" : [],
	"hair_sg1" : [],
	"hair_sg2" : [],
	"hair_taiwan1" : [],
	"hair_taiwan2" : []
}

def get_face_styles():
	

def get_eye_colors():
	
	
def get_eye_types():
	
def is_random_outcome():
	return "2" in script.ScriptName or script.ScriptName == "NLC_FaceExp"

def get_coupon(index):
	res = { # item IDs required for the given selection
		"hair_henesys1" : [],
		"hair_henesys2" : [],
		"hair_kerning1" : [],
		"hair_kerning2" : [],
		"hair_orbis1" : [],
		"hair_orbis2" : [],
		"hair_ludi1" : [],
		"hair_ludi2" : [],
		"hair_wedding1" : [],
		"hair_wedding2" : [],
		"hair_wedding3" : [],
		"NLC_HairVip" : [],
		"NLC_HairExp" : [],
		"hair_mureung1" : [],
		"hair_mureung2" : [],
		"hair_shouwa1" : [],
		"hair_shouwa2" : [],
		"hair_royal" : [],
		"hair_ariant1" : [],
		"hair_ariant2" : [],
		"hair_edel1" : [],
		"hair_sg1" : [],
		"hair_sg2" : [],
		"hair_taiwan1" : [],
		"hair_taiwan2" : []
	}
	
	if script.ScriptName in res:
		if len(res[script.ScriptName]) > index and index >= 0:
			return res[script.ScriptName][index]
	return 0

# dialog selection keys
DIALOG_MENU = 0
DIALOG_AVATAR = 1
DIALOG_RESPONSE = 2
# dialog option keys
OPTION_CUT = 0
OPTION_COLOR = 1
OPTION_MEMBER = 2

def get_dialog(selection, option):
	res = {
		"TEMPLATE" : 
			{
				DIALOG_MENU : "",
				DIALOG_AVATAR : 
					{
						OPTION_CUT : "",
						OPTION_COLOR : "",
						OPTION_MEMBER : ""
					},
				DIALOG_RESPONSE : # indexed by number since its the response code from the server
					[
						"",
						""
					]
			}
	}
	if option is None: return res[script.ScriptName][selection]
	else: return res[script.ScriptName][selection][option]

def error():
	return "NPC error, please take a screenshot of this message and post it on the discord. Thanks!"