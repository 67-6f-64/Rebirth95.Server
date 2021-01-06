'''function enter(pi) {
    pi.playPortalSound();
    
    switch (pi.getPlayer().getMapId()) {
	case 130000000:
	    pi.warp(130000100, 5); //or 130000101
	    break;
	case 130000200:
	    pi.warp(130000100, 4); //or 130000101
	    break;
	case 140010100:
            pi.warp(140010110, 1); //or 140010111
            break;
	case 120000101:
            pi.warp(120000105, 1);
            break;
	case 103000003:
            pi.warp(103000008, 1); //or 103000009
            break;
	case 100000201:
            pi.warp(100000204, 2); //or 100000205
            break;
        case 101000003: // portal warp fix thanks to Vcoc
            pi.warp(101000004, 2); //or 101000005
            break;
	default:
            pi.warp(pi.getMapId() + 1, 1); //or + 2
            break;
    }
	
    return true;
}'''

rooms = {
130000000 : [130000100, 5],
130000200 : [130000100, 4],
140010100 : [140010110, 1],
120000101 : [120000105, 1],
103000003 : [103000008, 1],
100000201 : [100000204, 2],
101000003 : [101000004, 2]
}

ctx.Warp(rooms[ctx.PlayerMapId][0], rooms[ctx.PlayerMapId][1])