const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('doEventStory: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doEventStory: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('doEventStory: find NEW');
			macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
			sleep(500);
			macroService.PollPattern(patterns.quest.events.new, { DoClick: true, ClickPattern: patterns.prompt.watchLater, PredicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, PredicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doEventStory: battle report');
			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: patterns.battle.next2, PredicatePattern: patterns.titles.events });
			sleep(500);
			macroService.PollPattern(patterns.quest.events.new, { DoClick: true, ClickPattern: patterns.prompt.watchLater, PredicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, PredicatePattern: patterns.battle.report });
			break;
	}

	sleep(1_000);
}
logger.info('Done...');