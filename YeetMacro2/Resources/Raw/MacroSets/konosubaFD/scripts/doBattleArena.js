const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
let done = false;
while (state.isRunning() && !done) {
	const loopResult = macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('doBattleArena: click tab quest');
			macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doBattleArena: click battle arena');
			macroService.clickPattern(patterns.quest.battleArena);
			break;
		case 'titles.battleArena':
			logger.info('doBattleArena: start arena');
			macroService.pollPattern(patterns.battleArena.begin, { doClick: true, predicatePattern: patterns.battleArena.advanced });
			sleep(500);
			macroService.pollPattern(patterns.battleArena.advanced, { doClick: true, predicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			
			break;
		case 'titles.party':
			logger.info('doBattleArena: select party');
			const targetPartyName = settings.party.battleArena.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement();
			}
			else {
				if (!(selectParty(targetPartyName))) {
					result = `targetPartyName not found: ${targetPartyName}`;
					done = true;
					break;
				}
			}
			sleep(500);
			macroService.pollPattern(patterns.battle.begin, { doClick: true, clickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doBattleArena: restart');
			// 1st battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
			// OR         => battle.next (middle) => battle.replay => battle.replay.ok
			// 2nd battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
			// OR         => battle.next (middle) => battle.replay => battle.replay.ok
			// 3rd battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay.disabled
			// OR         => battle.next (middle) => battle.replay.disabled

			macroService.pollPattern(patterns.battle.next, { doClick: true, clickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], predicatePattern: [patterns.battle.replay, patterns.battle.next3] });
			sleep(500);
			const replayResult = macroService.findPattern(patterns.battle.replay);
			if (replayResult.isSuccess) {
				logger.debug('doBattleArena: found replay');
				macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.battle.replay.ok });
				macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			} else {
				logger.debug('doBattleArena: found next3');
				const next3Result = macroService.pollPattern(patterns.battle.next3, { doClick: true, clickPattern: [patterns.battleArena.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: [patterns.titles.battleArena, patterns.battle.replay] });
				if (next3Result.predicatePath === 'titles.battleArena') {
					done = true;
					break;
				}
				macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.battle.replay.ok });
				macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			}
			break;
	}

	sleep(1_000);
}
logger.info('Done...');