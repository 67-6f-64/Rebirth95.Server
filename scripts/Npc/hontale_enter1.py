# horntail's schedule
# exists in several horntail maps

# TODO maybe implement this (if we do full quest)

if ctx.PlayerMapId == 240050000:
	ctx.SayOk("Schedule? what schedule..")
else: # 240050100, 240050300, 240050310
	ctx.SayOk("Uncoded, please report.") # inside PQ maps

# BMS script

# //혼테일 필드셋1 에 입장. 혼테일 이정표
# script "hontale_enter1" {
	# field = self.field;
	# inven = target.inventory;
	# quest = FieldSet( "Hontale1" );

	# if ( field.id == 240050000 ) {
		# if ( target.isPartyBoss == 1 ) {
			# v0 = self.askMenu( "Don't you dare to step up to the cave of life... Only those who find a hidden key will come to me. Do you want to challenge this reckless game?\r\n#b#L0# Yes, I do#l" );
			# if ( v0 == 0 ) {
				# setParty = FieldSet( "Hontale1" );
				# res = setParty.enter( target.nCharacterID, 0 );
				# if ( res == -1 ) self.say( "You can't try it now. Come back later." );
				# else if ( res == 1 ) self.say( "Don't show your reckless valor, fool... Challenge with the stronger ones." );
				# else if ( res == 2 ) self.say( "Don't show your reckless valor, fool... Challenge with the stronger ones." );
				# else if ( res == 3 ) self.say( "Who do you think you are! You are too weak. Go back." );
				# else if ( res == 4 ) self.say( "Another party is already challenging it. They're just as fool as you are." );
				# else {
					# hontale_takeawayitem;
				# }
			# }
		# } else {
			# self.say( "Are you the leader for this group of the foolish?" );
			# end;
		# }
	# } else if ( field.id == 240050100 ) {
		# if ( target.isPartyBoss == 1 ) {
			# nItem = inven.itemCount( 4001092 );//붉은열쇠
			# if ( nItem > 0 ) {
				# v0 = self.askMenu( "Do you want to use #r#t4001092##k and move to the cave of choice?\r\n#b#L0# Yes, I would like to move on.#l" );
				# if ( v0 == 0 ) {
					# cNum = Field( 240050101 ).getUserCount + Field( 240050102 ).getUserCount + Field( 240050103 ).getUserCount + Field( 240050104 ).getUserCount + Field( 240050105 ).getUserCount;
					# if ( cNum > 0 ) {
						# self.say( "Isn't  there someone in the maze yet?" );
						# end;
					# } else {
						# ret = inven.exchange( 0, 4001092, -1 );
						# if ( ret != 0 ) {
							# field.transferFieldAll( 240050200, "st00" );
						# } else {
							# self.say( "Check out if you have#t4001092#." );
							# end;
						# }
					# }
				# }			
			# } else {
				# self.say( "Only those who have #t4001092# can enter the cave of choice." );
				# end;
			# }
		# } else {
			# self.say( "Are you here on behalf of the foolish ones?" );		
			# end;
		# }
	# } else if ( field.id == 240050300 ) {
		# if ( target.isPartyBoss == 1 ) {
			# if ( quest.getVar( "B1" ) != "1" ) {
				# nItem = inven.itemCount( 4001093 );
				# if ( nItem >= 6 ) {
					# v0 = self.askMenu( "Do you want to leave this place using six #b#t4001093##k?\r\n#b#L0# Use the blue key.#l" );
					# if ( v0 == 0 ) {
						# ret = inven.exchange( 0, 4001093, -6 );
						# if ( ret == 0 ) self.say( "Check out if you have #b#t4001093##k." );
						# else {
							# quest.setVar( "B1", "1" );
							# field.setMobGen( 0, 0 );
							# self.say( "All devil spirits should be cleared to get out of here using the blue key. After eliminating all monsters, talk to me again." );
						# }
					# }
				# } else {
					# self.say( "Do you have six #b#t4001093##k?" );
					# end;
				# }
			# } else {
				# v0 = self.askMenu( "Did you clear all monsters in this room?\r\n#b#L0# All cleared.#l" );
				# if ( v0  == 0 ) {
					# mobNum = field.getMobCount( 9300077 ) + field.getMobCount( 9300076 );
					# if ( mobNum > 0 ) {
						# self.say( "This room hasn't cleared yet." );
						# end;
					# } else {
						# v1 = self.askYesNo( "Succeeded in purifying this room and collecting #b#t4001093##k. Do you want to move to Horntail's cave?" );
						# if ( v1 == 0 ) self.say( "Think again and talk to me." );
						# else {
							# hontale_takeawayitem;
							# field.transferFieldAll( 240050600, "st00" );
						# }
					# }
				# }
			# }
		# } else self.say( "Are you the leader to this group of foolish?" );	
	# } else if ( field.id == 240050310 ) {
		# if ( target.isPartyBoss == 1 ) {
			# if ( quest.getVar( "B2" ) != "1" ) {
				# nItem = inven.itemCount( 4001093 );
				# if ( nItem >= 6 ) {
					# v0 = self.askMenu( "Do you want to leave this place using six #b#t4001093##k?\r\n#b#L0#use the blue key.#l" );
					# if ( v0 == 0 ) {
						# ret = inven.exchange( 0, 4001093, -6 );
						# if ( ret == 0 ) self.say( "Check out if you have #b#t4001093##k." );
						# else {
							# quest.setVar( "B2", "1" );
							# field.setMobGen( 0, 0 );
							# self.say( "All devil spirits should be cleared to get out of here using the blue key. After eliminating all monsters, talk to me again." );
						# }
					# }
				# } else {
					# self.say( "Do you have six #b#t4001093##k?" );
					# end;
				# }
			# } else {
				# v0 = self.askMenu( "Did you clear all monsters in this room?\r\n#b#L0# All cleared.#l" );
				# if ( v0  == 0 ) {
					# mobNum = field.getMobCount( 9300078 ) + field.getMobCount( 9300079 );
					# if ( mobNum > 0 ) {
						# self.say( "This room hasn't cleared yet." );
						# end;
					# } else {
						# v1 = self.askYesNo( "Succeeded in purifying this room and collecting #b#t4001093##k. Do you want to move to Horntail's cave?" );
						# if ( v1 == 0 ) self.say( "Think again and talk to me." );
						# else {
							# hontale_takeawayitem;
							# field.transferFieldAll( 240050600, "st00" );
						# }
					# }
				# }
			# }
		# } else self.say( "Are you the leader to this group of foolish?" );	
	# }
# }
