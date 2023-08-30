const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.party, patterns.titles.events, patterns.battle.report];
let done = false;
result = { numBattles: 0 };
while (state.isRunning && !done) {
	const loopResult = macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('farmEventLoop: click tab quest');
			macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventLoop: click quest events');
			macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventLoop: start farm');
			const targetFarmLevel = settings.farmEvent.targetLevel.props.value ?? 12;
			macroService.pollPattern(patterns.quest.events.quest, { doClick: true, predicatePattern: patterns.titles.quest });
			sleep(500);
			macroService.pollPattern(patterns.quest.events.quest.normal[targetFarmLevel], { doClick: true, predicatePattern: patterns.titles.events });
			sleep(500);
			macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			sleep(500);
			break;
		case 'titles.party':
			logger.info('farmEventLoop: select party');
			const targetPartyName = settings.party.farmEventLoop.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement();
			}
			else if (targetPartyName) {
				if (!(selectParty(targetPartyName))) {
					result = `targetPartyName not found: ${targetPartyName}`;
					done = true;
					break;
				}
			}

			sleep(500);
			const beginResult = macroService.pollPattern(patterns.battle.begin, { doClick: true, clickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			if (beginResult.predicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				done = true;
			}
			result.numBattles++;
			break;
		case 'battle.report':
			logger.info('farmEventLoop: replay battle');
			macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.battle.replay.prompt });
			sleep(500);
			const replayResult = macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			if (replayResult.predicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				done = true;
				break;
			}
			result.numBattles++;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');