#
# NPC Name: Cody
# NPC ID: 9200000
# All in one NPC
#

chars = [
	"#dXavier",
	"#bTonblader",
	"#rDetritus",
	"#dMori",
]

text = "Hey there,\r\nI'm Cody, the NPC representative for the Rebirth developers!\r\n\r\nI just want to give a shoutout to the following players for being absolutely fantastic during the alpha and helping us out immensely with quality assurance testing.\r\n"

for name in chars:
	text += "\r\n #k- #e{} #n".format(name)

ctx.SayOk(text)