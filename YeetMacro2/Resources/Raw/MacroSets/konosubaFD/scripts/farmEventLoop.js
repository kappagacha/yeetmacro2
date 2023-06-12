const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
let done = false;
result = { numBattles: 0 };
while (state.isRunning && !done) {
	const loopResult = await macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
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
			const beginResult = await macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			if (beginResult.predicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				done = true;
			}
			result.numBattles++;
			break;
		case 'battle.report':
			logger.info('farmEventLoop: replay battle');
			await macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp], predicatePattern: patterns.battle.replay.prompt });
			await sleep(500);
			const replayResult = await macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			if (replayResult.predicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				done = true;
				break;
			}
			result.numBattles++;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');