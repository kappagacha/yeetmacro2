let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.freeQuests];
const offset = macroService.calcOffset(patterns.titles.home);

while (state.isRunning() && !done) {
	const loopResult = macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('doFreeQuests: click tab quest');
			macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doFreeQuests: click quest free quests');
			macroService.clickPattern(patterns.quest.freeQuests);
			break;
		case 'titles.freeQuests':
			logger.info('doFreeQuests: target upgrade stone');
			const upgradeStoneTargetLevel = settings.freeQuests.upgradeStone.targetLevel.props.value || 'intermediate';
			if (upgradeStoneTargetLevel !== 'extreme') {
				macroService.pollPattern(patterns.freeQuests.upgradeStone, { doClick: true, predicatePattern: patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel] });
				macroService.pollPattern(patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel], { doClick: true, predicatePattern: patterns.tickets.add });
				// sample text capture: "25 x1" (it catches some of the words)
				let numTickets = (screenService.getText(patterns.tickets.numTickets)).split('x')[1];
				while (state.isRunning() && numTickets < 2) {
					macroService.clickPattern(patterns.tickets.add);
					sleep(500);
					numTickets = (screenService.getText(patterns.tickets.numTickets)).split('x')[1];
				}
				macroService.pollPattern(patterns.tickets.use, { doClick: true, clickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.tickets.prompt.ok });
				macroService.pollPattern(patterns.tickets.prompt.ok, { doClick: true, predicatePattern: patterns.titles.freeQuests });
			}
			
			logger.info('doFreeQuests: skip all');
			macroService.pollPattern(patterns.freeQuests.eris, { doClick: true, predicatePattern: patterns.skipAll.skipQuest });
			macroService.pollPattern(patterns.skipAll.skipQuest, { doClick: true, predicatePattern: patterns.skipAll.button });
			const filterOffResult = macroService.findPattern(patterns.skipAll.search.filter.off);
			if (filterOffResult.isSuccess) {
				macroService.pollPattern(patterns.skipAll.search.filter.off, { doClick: true, predicatePattern: patterns.skipAll.search.filter });
				sleep(500);
				const checkResult = macroService.findPattern(patterns.skipAll.search.filter.check, { limit: 5 });
				for (let point of checkResult.points) {
					logger.debug(JSON.stringify(point));
					if (point.x < offset.x + 300.0) continue;		// skip 4 stars
					screenService.doClick(point);
					sleep(250);
				}
				macroService.pollPattern(patterns.skipAll.search.filter.close, { doClick: true, predicatePattern: patterns.skipAll.title });
				sleep(500);
			}

			let maxNumSkips = screenService.getText(patterns.skipAll.maxNumSkips);
			while (state.isRunning() && maxNumSkips < 2) {
				macroService.clickPattern(patterns.skipAll.addMaxSkips);
				sleep(500);
				maxNumSkips = screenService.getText(patterns.skipAll.maxNumSkips);
			}
			macroService.pollPattern(patterns.skipAll.button, { doClick: true, predicatePattern: patterns.skipAll.prompt.ok });
			sleep(1_000);
			macroService.pollPattern(patterns.skipAll.prompt.ok, { doClick: true, predicatePattern: patterns.skipAll.skipComplete });
			macroService.pollPattern(patterns.skipAll.skipComplete, { doClick: true, clickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.skipAll.title });
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');