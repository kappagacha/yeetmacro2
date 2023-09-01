const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
while (state.isRunning()) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('doEventStory: click tab quest');
			macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doEventStory: click quest events');
			macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('doEventStory: find NEW');
			macroService.pollPattern(patterns.quest.events.quest, { doClick: true, predicatePattern: patterns.titles.quest });
			sleep(500);
			macroService.pollPattern(patterns.quest.events.new, { doClick: true, clickPattern: patterns.prompt.watchLater, predicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			sleep(500);
			macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doEventStory: battle report');
			macroService.pollPattern(patterns.battle.next, { doClick: true, clickPattern: patterns.battle.next2, predicatePattern: patterns.titles.events });
			sleep(500);
			macroService.pollPattern(patterns.quest.events.new, { doClick: true, clickPattern: patterns.prompt.watchLater, predicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			sleep(500);
			macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: patterns.battle.report });
			break;
	}

	sleep(1_000);
}
logger.info('Done...');