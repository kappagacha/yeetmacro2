const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
while (state.isRunning) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			await macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			const targetFarmLevel = settings.farmEvent.targetLevel.props.value ?? 12;
			await macroService.pollPattern(patterns.quest.events.quest, { doClick: true, predicatePattern: patterns.titles.quest });
			await macroService.pollPattern(patterns.quest.events.quest.normal[targetFarmLevel], { doClick: true, predicatePattern: patterns.titles.events });
			await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			await macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			await macroService.pollPattern(patterns.battle.replay, { doClick: true, touchPattern: patterns.battle.next, predicatePattern: patterns.battle.replay.prompt });
			await macroService.pollPattern(patterns.prompt.ok, { doClick: true, predicatePattern: patterns.battle.report });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');