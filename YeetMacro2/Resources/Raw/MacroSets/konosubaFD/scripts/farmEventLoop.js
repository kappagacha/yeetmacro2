﻿const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
while (state.isRunning) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('farmEventLoop: click tab quest');
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventLoop: click quest events');
			await macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventLoop: start farm');
			const targetFarmLevel = settings.farmEvent.targetLevel.props.value ?? 12;
			await macroService.pollPattern(patterns.quest.events.quest, { doClick: true, predicatePattern: patterns.titles.quest });
			await sleep(500);
			await macroService.pollPattern(patterns.quest.events.quest.normal[targetFarmLevel], { doClick: true, predicatePattern: patterns.titles.events });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('farmEventLoop: replay battle');
			await macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: patterns.battle.next, predicatePattern: patterns.battle.replay.prompt });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');