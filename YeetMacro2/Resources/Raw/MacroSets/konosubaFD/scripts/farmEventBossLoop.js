const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report, patterns.titles.bossMulti];
while (state.isRunning) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('farmEventBossLoop: click tab quest');
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventBossLoop: click quest events');
			await macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventBossLoop: start farm');
			const bossBattleResult = await macroService.pollPattern(patterns.quest.events.bossBattle, { doClick: true, predicatePattern: [patterns.quest.events.bossBattle.prompt.chooseBattleStyle] });
			if (bossBattleResult.predicatePath === 'quest.events.bossBattle.prompt.chooseBattleStyle') {
				await macroService.pollPattern(patterns.quest.events.bossBattle.multi, { doClick: true, predicatePattern: patterns.titles.bossMulti });
			}
			break;
		case 'titles.bossMulti':
			await macroService.pollPattern(patterns.quest.events.bossBattle.extreme, { doClick: true, predicatePattern: patterns.battle.prepare });
			let currentCost = await screenService.getText(patterns.quest.events.bossBattle.cost);
			while (currentCost < 3) {
				await macroService.clickPattern(patterns.quest.events.bossBattle.addCost);
				await sleep(500);
				currentCost = await screenService.getText(patterns.quest.events.bossBattle.cost);
			}
			await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.joinRoom, { doClick: true, predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('farmEventBossLoop: leave room');
			await macroService.pollPattern(patterns.battle.leaveRoom, { doClick: true, clickPattern: patterns.battle.next, predicatePattern: patterns.titles.bossMulti });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');