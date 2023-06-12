const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report, patterns.titles.bossMulti, patterns.titles.party];
let done = false;
result = { numBattles: 0 };
while (state.isRunning && !done) {
	const loopResult = await macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
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
			logger.info('farmEventBossLoop: max cost');
			await macroService.pollPattern(patterns.quest.events.bossBattle.extreme, { doClick: true, predicatePattern: patterns.battle.prepare });
			let currentCost = await screenService.getText(patterns.quest.events.bossBattle.cost);
			while (state.isRunning && currentCost < 3) {
				const addCostDisabledResult = await macroService.findPattern(patterns.quest.events.bossBattle.addCost.disabled);
				if (addCostDisabledResult.isSuccess) {
					break;
				}
				await macroService.clickPattern(patterns.quest.events.bossBattle.addCost);
				await sleep(500);
				currentCost = await screenService.getText(patterns.quest.events.bossBattle.cost);
			}
			if (currentCost == 1) {
				result.message = 'Not enough boss tickets...';
				done = true;
				break;
			}
			const prepareResult = await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: [patterns.titles.party, patterns.events.bossBattle.prompt.notEnoughBossTickets] });
			if (prepareResult.predicatePath === 'events.bossBattle.prompt.notEnoughBossTickets') {
				result.message = 'Not enough boss tickets...';
				done = true;
				break;
			}
			break;
		case 'titles.party':
			logger.info('farmEventBossLoop: select party');
			const targetPartyName = settings.party.eventBoss.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				await selectPartyByRecommendedElement(-425);	// Recommended Element icons are shifted by 425 to the left of expected location
			}
			else {
				if (!(await selectParty(targetPartyName))) {
					result = `targetPartyName not found: ${targetPartyName}`;
					done = true;
					break;
				}
			}
			await sleep(500);
			await macroService.pollPattern(patterns.battle.joinRoom, { doClick: true, predicatePattern: patterns.battle.report });
			result.numBattles++;
			break;
		case 'battle.report':
			logger.info('farmEventBossLoop: leave room');
			await macroService.pollPattern(patterns.battle.leaveRoom, { doClick: true, clickPattern: patterns.battle.next, predicatePattern: patterns.titles.bossMulti });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');