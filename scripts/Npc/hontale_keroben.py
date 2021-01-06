# warps in to horntail lobby
# NPC ID : 2081005
# MAP : Cave of Life - Entrance

ctx.SayOk("Oh, my Brother! Don't worry about human's invasion. I'll protect you all. Come in.")
ctx.Warp(240050000, 1) # st00


# BMS script below

# //케로벤 변신 체크. 케로벤.
# script "hontale_keroben" {
	# //변신 체크
	# morphID = target.getMorphState;
	# if ( morphID == 4 ) {
		# self.say( "Oh, my Brother! Don't worry about human's invasion. I'll protect you all. Come in." );
		# registerTransferField( 240050000, "st00" );
		# target.undoMorph;
	# } else {
		# cHP = target.nHP;
		# if ( cHP > 500 ) {
			# target.incHP( -500, 0 );
			# self.say( "That's far enough, human! No one is allowed beyond this point. Get away from here!" );
			# registerTransferField( 240040600, "st00" );
		# } else if ( cHP > 1 and cHP <= 500 ) {
			# damage = target.nHP - 1;
			# target.incHP( -damage, 0 );
			# self.say( "That's far enough, human! No one is allowed beyond this point. Get away from here!" );
			# registerTransferField( 240040600, "st00" );			
		# } else if ( cHP == 1 ) {
			# self.say( "That's far enough, human! No one is allowed beyond this point. Get away from here!" );
			# registerTransferField( 240040600, "st00" );			
		# }
	# }
# }