const loopPatterns = [patterns.titles.home, patterns.titles.quest];
while (state.isRunning) {
	const result = await macroService.pollPattern(loopPatterns);
	if (!result.isSuccess) continue;
	switch (result.path) {
		case 'titles.home':
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			await macroService.clickPattern(patterns.quest.mainQuests);
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');