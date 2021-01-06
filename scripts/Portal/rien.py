
# rienTutorX portals
res = {
	"enterport" : [140020300, 1],
	"riencaveenter" : [140030000, 1],
	"entermcave" : [140090000, 0],
	"1" : [140090200, 1],
	"2" : [140090300, 0],
	"3" : [140090400, 0],
	"4" : [140090500, 0],
	#"5" : [0, 0],
	#"6" : [0, 0],
	"7" : [140010100, 2],
	"8" : [140010000, 2],
}
if script.ScriptName in res:
	map = res[script.ScriptName][0]
	portal = res[script.ScriptName][1]
	
	ctx.Warp(map, portal)
elif script.ScriptName[9] in res:
	map = res[script.ScriptName[9]][0]
	portal = res[script.ScriptName[9]][1]
	
	ctx.Warp(map, portal)