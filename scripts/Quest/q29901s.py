#function start(mode, type, selection) {
#	if (qm.getPlayer().getJob() % 100 > 0 && qm.getPlayer().getJob() < 1000) {
#		qm.forceStartQuest();
#	}
#	qm.dispose();
#}

# medal quest

if ctx.CanStart:
	ctx.StartQuest(True)