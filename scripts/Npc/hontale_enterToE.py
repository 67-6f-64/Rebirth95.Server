# Encrypted Slate of the Squad
# Warps parties to Entrance to Horntail's Cave

if ctx.PlayerPartyLeader:
	# nItem = inven.itemCount( 4001086 );
	if ctx.AskYesNo("The letters on the slate glitter and the backdoor opens. Do you want to go to the secret path?"):
		ctx.WarpPartyToInstance(ctx.PlayerFieldUID, 240050400, 1)
	else:
		ctx.SayOk("If you want to move, talk to me again.")
else:
	ctx.SayOk("Please proceed through the Party Leader.")


# //증표있을 경우 바로 E맵으로 입장. 결사대 암호 석판.
# script "hontale_enterToE" {
	# if ( target.isPartyBoss ==1 ) {
		# inven = target.inventory;
		# nItem = inven.itemCount( 4001086 );
		# if ( nItem > 0 ) {
			# v0 = self.askYesNo( "The letters on the slate glitter and the backdoor opens. Do you want to go to the secret path?" );
			# if ( v0 == 0 ) self.say( "If you want to move, talk to me again." );
			# else {
				# ret = target.transferParty( 240050400, "", 2 );
				# end;
			# }
		# } else {
			# self.say( "You can't read the words on the slate. You have no idea where to use it." );
			# end;
		# }
	# } else {
		# self.say( "Please proceed through the Party Leader." );
	# }
# }