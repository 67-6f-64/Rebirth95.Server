# attribute indexes
MENU = 0
MENU_0 = 1
MENU_1 = 2
WARP = 3
ISMENU = 4

CAVE_EXIT = 240050500
MENU_TEXT_DEFAULT = "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l"

# attribute dictionary
ATTRIBUTES = {
	240050100 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "Only those who have the crystal key can enter the maze room.\r\nOnly those left can open the door of maze room.\r\ncan get what you want from glittering tree hole.\r\nThe key made on the flame shines in the cave.",
		WARP : CAVE_EXIT,
	},
	240050101 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "You have to discard to earn something.\r\nYou can drop what you wish to drop in the glittering tree hole.",
		WARP : CAVE_EXIT,
	},
	240050102 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l",
		WARP : CAVE_EXIT
	},
	240050103 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l",
		WARP : CAVE_EXIT
	},
	240050104 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l",
		WARP : CAVE_EXIT
	},
	240050105 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "When you find the key made on the flame, you'll see the end of the maze.",
		WARP : CAVE_EXIT
	},
	240050200 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "Darkness is connected to darkness, and light to light.\r\nYour choice always makes results.",
		WARP : CAVE_EXIT
	},
	240050300 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "The key made in the ice shines the cave.",
		WARP : CAVE_EXIT
	},
	240050310 : {
		ISMENU : True,
		MENU : MENU_TEXT_DEFAULT,
		MENU_0 : "The key made in the ice shines the cave.",
		WARP : CAVE_EXIT
	},
	240050400 : {
		ISMENU : False,
		MENU : "Do you want to go back to #m240050000#?",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050000
	},
	240050500 : {
		ISMENU : True,
		MENU : "The entrance of the cave is reflected on the crystal. It seems you can get there when touching it.\r\n#b#L1# touch the crystal#l",
		WARP : 240050000
	},
	240060000 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
	240060001 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
	240060100 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
	240060101 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
	240060200 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
	240060201 : {
		ISMENU : False,
		MENU : "Do you want to give up squad and quit? ",
		MENU_0 : "Think again and talk to me.",
		WARP : 240050400
	},
}


def is_menu():
	return ATTRIBUTES[ctx.PlayerMapId][ISMENU]


def menu_text():
	return ATTRIBUTES[ctx.PlayerMapId][MENU]


def menu_0_text():
	return ATTRIBUTES[ctx.PlayerMapId][MENU_0]


def menu_warp_map():
	return ATTRIBUTES[ctx.PlayerMapId][WARP]


def start():
	if is_menu():
		ret = ctx.AskMenu(menu_text())
		if ret == 0:
			ctx.SayOk(menu_0_text())
		else:
			ctx.Warp(menu_warp_map())
	else:
		if ctx.AskYesNo(menu_text()):
			ctx.Warp(menu_warp_map())
		else:
			ctx.SayOk(menu_0_text())


start()

# BMS script

# //수정
# script "hontale_out" {
	# field = self.field;

	# if ( field.id == 240050100 ) {
		# v0 = self.askMenu( "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l" );
		# if ( v0 == 0 ) {
			# self.say( "Only those who have the crystal key can enter the maze room.\r\nOnly those left can open the door of maze room.\r\ncan get what you want from glittering tree hole.\r\nThe key made on the flame shines in the cave." );
		# } else {
			# registerTransferField( 240050500, "st00" );			
		# }
	# } else if ( field.id == 240050101 or field.id == 240050102 or field.id == 240050103 or field.id == 240050104 ) {
		# v0 = self.askMenu( "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l" );
		# if ( v0 == 0 ) {
			# self.say( "You have to discard to earn something.\r\nYou can drop what you wish to drop in the glittering tree hole." );
		# } else {
			# registerTransferField( 240050500, "st00" );			
		# }
	# } else if ( field.id == 240050105 ) {
		# v0 = self.askMenu( "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l" );
		# if ( v0 == 0 ) {
			# self.say( "When you find the key made on the flame, you'll see the end of the maze." );
		# } else {
			# registerTransferField( 240050500, "st00" );			
		# }
	# } else if ( field.id == 240050200 ) {
		# v0 = self.askMenu( "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l" );
		# if ( v0 == 0 ) {
			# self.say( "Darkness is connected to darkness, and light to light.\r\nYour choice always makes results." );
		# } else {
			# registerTransferField( 240050500, "st00" );			
		# }
	# } else if ( field.id == 240050300 or field.id == 240050310 ) {
		# v0 = self.askMenu( "Words are revealed on the surface of crystal tangled roots.\r\n#b#L0# read the words carefully.#l\r\n#L1# Would you like to give up and get out?#l" );
		# if ( v0 == 0 ) {
			# self.say( "The key made in the ice shines the cave." );
		# } else {
			# registerTransferField( 240050500, "st00" );			
		# }
	# } else if ( field.id == 240050400 ) {
		# v0 = self.askYesNo( "Do you want to go back to #m240050000#" );
		# if ( v0 == 0 ) {
			# self.say( "Think again and talk to me." );
			# end;
		# } else {
			# registerTransferField( 240050000, "st00" );
		# }
	# } else if ( field.id == 240050500 ) {
		# v0 = self.askMenu( "The entrance of the cave is reflected on the crystal. It seems you can get there when touching it.\r\n#b#L0# touch the crystal#l" );
		# if ( v0 == 0 ) {
			# hontale_takeawayitem;
			# registerTransferField( 240050000, "st00" );
		# }
	# } else if ( field.id == 240060000 or field.id == 240060100 or field.id == 240060200 ) {
		# qr = target.questRecord;
		# eNum = qr.get( 7311 );

# #//		if ( serverType == 1 ) { //개발서버
		# if ( serverType == 2 ) {
			# v0 = self.askYesNo( "Do you want to give up squad and quit? " );
		# } else {
			# if ( eNum == "1" ) 	v0 = self.askYesNo( "Do you want to give up squad and quit?  You can only enter twice a day. If you quit now, you can enter only once today." );
			# if ( eNum == "2" ) 	v0 = self.askYesNo( "Do you want to give up squad and quit? You can only enter twice a day. When you quit now, you can't enter again today." );
		# }
		# if ( v0 == 0 ) {
			# self.say( "Think again and talk to me." );
			# end;			
		# } else {
			# registerTransferField( 240050400, "st00" );
		# }		
	# }
# }