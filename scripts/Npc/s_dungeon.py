
ctx.SayOk("I'm unscripted :)")

'''
//NPC9120201 보스맵 입장
script "s_dungeon"{
  
    qr = target.questRecord;
    val = qr.get( 8001 );
    
    quest = FieldSet( "shouwaBoss" );
    users = quest.getUserCount;
    result = quest.getVar( "shouwaBoss" );
  	inven = target.inventory;
  	
  	count=inven.itemCount(4000141);
  	if ( count >= 1 ) {
  	   inven.exchange(0, 4000141, -count);
  	   self.say("Hey, hey! It's dangerous to carry around a flashlight like that! It's going to cause a fire! I'll take care of it. cant' be too careful around here...");
  	   }
  	if ( inven.itemCount(4000138) >= 1 ) {
			cTime = currentTime;
			aTime = compareTime( cTime, val );
			// 하루가 지난 경우
			if ( aTime >= 1440 ) {
			if ( result == "yes" ) self.say( "I'm sorry, but the battle has already begun, and for your safety, I must ask that you remain outside for now." );
			   else{ 			    
			    nRet1 = self.askYesNo("Hey hey! That item you have there...isn't that our boss's comb!? Holy cow! I knew it! As soon as I saw you, I knew you would be the one defiant enough to take on the Boss. Are you sure? He's not going to give up without a fight--the evil spirit within him will ensure that. Do you want to take on the Boss?");
			    if(nRet1 !=0){
			      
			      nRet2 = self.askYesNo("Are you sure you're going to enter this room? Just remember, you can't stay here forever, and if you place our boss's comb on top of the treasure chest, the thugs will pounce on you, so be careful! ");
			        if(nRet2 !=0){
			      
					    if ( users < 20 ) {
						  qr.set( 8001, cTime );
						  registerTransferField( 801040100, "" );
					    }
					    else self.say( "A lot of brave fighters are currently inside battling the evil spirits who've possessed our leaders. The room can only hold 20 people at once. You'll have to wait your turn for now." );
            }
            else self.say("Really? Then let me know if you ever change your mind. ");
            
            }
          else self.say("Really? Then let me know if you ever change your mind. ");
      }
}
			  else self.say( "Our enemies are outside! Let's wait here!" );
	  }
	  else self.say("So you've made it here. Not bad. You'll be taking on the boss now! I'm concerned as to whether you take on the mighty boss with your abilities ... don't get me wrong, our Boss couldn't handle her either. If you, by any chance, take down the boss and bring back her comb with you, then I'll take you to the next stage. ");
	  
	  }'''