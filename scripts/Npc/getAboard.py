
ctx.SayNext("Welcome to orbis station! I am Isa the Station Guide.")

res = ctx.AskMenu("Where would you like to go?\r\n#b#L0#Victoria Station#l\r\n#L1#Ludibrium#l\r\n#L2#Edelstein#l\r\n#L3#Leafre#l\r\n#L4#Ariant#l\r\n#L5#Mu Lung#l\r\n#L6#Ereve#l")

maps = [104020110, 220000100, 310000010, 240000110, 260000100, 250000100, 130000210]

if res < len(maps) and res >= 0:
	warp = maps[res]
	
	if ctx.AskYesNo("Are you sure you would like to travel to #r#m{}##k?".format(warp)):
		ctx.Warp(warp)