

res = ctx.AskMenu("What do you want from me? \r\n#b#L0#Gather up some information on the hideout.  #l\r\n#L1#Take me to the hideout.#l\r\n#L2#Nothing. #l#k")

if res == 0:
	ctx.SayOk("I can take you to the hideout, but the place is infested with thugs looking for trouble. You'll need to be both incredibly strong and brave to enter the premise. At the hideaway, you'll find the Boss that controls all the other bosses around this area. It's easy to get to the hideout, but the room on the top floor of the place can only be entered ONCE a day. The Boss's Room is not a place to mess around. I suggest you don't stay there for too long; you'll need to swiftly take care of the business once inside. The boss himself is a difficult foe, but you'll run into some incredibly powerful enemies on your way to meeting the boss! It ain't going to be easy.")
elif res == 1:
	ctx.SayOk("Oh, the brave one. I've been awaiting your arrival. If these thugs are left unchecked, there's no telling what going to happen in this neighborhood. Before that happens, I hope you take care of all of them and beat the boss, who resides on the 5th floor. You'll need to be on alert at all times, since the boss is too tough for even the wisemen to handle. Looking at your eyes, however, I can see that eye of the tiger, the eyes that tell me you can do this. Let's go!")
	ctx.Warp(801040000)
else:
	ctx.SayOk("I'm a busy person! Leave me alone if that's all you need!")