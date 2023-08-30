const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report, patterns.titles.bossBattle, patterns.titles.bossMulti, patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets];
let done = false, isBossMulti = false;
result = { numBattles: 0 };
while (state.isRunning && !done) {
	const loopResult = macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('farmEventBossLoop: click tab quest');
			macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventBossLoop: click quest events');
			macroService.clickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventBossLoop: start farm');
			const bossBattleResult = macroService.pollPattern(patterns.quest.events.bossBattle, { doClick: true, predicatePattern: [patterns.quest.events.bossBattle.prompt.chooseBattleStyle, patterns.titles.bossBattle] });
			if (bossBattleResult.predicatePath === 'quest.events.bossBattle.prompt.chooseBattleStyle') {
				macroService.pollPattern(patterns.quest.events.bossBattle.multi, { doClick: true, predicatePattern: patterns.titles.bossMulti });
			}
			break;
		case 'titles.bossBattle':
			const dailyAttemptResult = macroService.findPattern(patterns.quest.events.bossBattle.dailyAttempt);
			if (dailyAttemptResult.isSuccess) {
				macroService.pollPattern(patterns.quest.events.bossBattle.expert, { doClick: true, predicatePattern: patterns.battle.prepare });
				macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			} else {
				const hardResult = macroService.pollPattern(patterns.quest.events.bossBattle.hard, { doClick: true, predicatePattern: [patterns.battle.prepare, patterns.quest.events.bossBattle.notEnoughTickets] });
				if (hardResult.predicatePath === 'quest.events.bossBattle.notEnoughTickets') {
					result.message = 'Not enough boss tickets...';
					done = true;
					break;
				}
				let currentCost = screenService.getText(patterns.quest.events.bossBattle.cost);
				for (let i = 0; state.isRunning && i < 2; i++) {
					const addCostDisabledResult = macroService.findPattern(patterns.quest.events.bossBattle.addCost.disabled);
					if (addCostDisabledResult.isSuccess) {
						break;
					}
					macroService.clickPattern(patterns.quest.events.bossBattle.addCost);
					sleep(500);
					currentCost = screenService.getText(patterns.quest.events.bossBattle.cost);
				}
				logger.debug(`currentCost: ${currentCost}`);
				if (currentCost == 1) {
					result.message = 'Not enough boss tickets...';
					done = true;
					break;
				}
				const prepareResult = macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: [patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets] });
				if (prepareResult.predicatePath === 'events.bossBattle.prompt.notEnoughBossTickets') {
					result.message = 'Not enough boss tickets...';
					done = true;
					break;
				}
				macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			}
			break;
		case 'titles.bossMulti':
			logger.info('farmEventBossLoop: max cost');
			isBossMulti = true;
			macroService.pollPattern(patterns.quest.events.bossBattle.extreme, { doClick: true, predicatePattern: patterns.battle.prepare });
			let currentCost = screenService.getText(patterns.quest.events.bossBattle.cost);
			for (let i = 0; state.isRunning && i < 2; i++) {
				const addCostDisabledResult = macroService.findPattern(patterns.quest.events.bossBattle.addCost.disabled);
				if (addCostDisabledResult.isSuccess) {
					break;
				}
				macroService.clickPattern(patterns.quest.events.bossBattle.addCost);
				sleep(500);
				currentCost = screenService.getText(patterns.quest.events.bossBattle.cost);
			}
			logger.debug(`currentCost: ${currentCost}`);
			if (currentCost == 1) {
				result.message = 'Not enough boss tickets...';
				done = true;
				break;
			}
			const prepareResult = macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: [patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets] });
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
				selectPartyByRecommendedElement(isBossMulti ? -425: 0);	// Recommended Element icons are shifted by 425 to the left of expected location
			}
			//else if (!(selectParty(targetPartyName))) {
			//	result = `targetPartyName not found: ${targetPartyName}`;
			//	done = true;
			//	break;
			//}
			sleep(500);
			macroService.pollPattern([patterns.battle.joinRoom, patterns.battle.begin], { doClick: true, predicatePattern: patterns.battle.report });
			result.numBattles++;
			break;
		case 'battle.report':
			logger.info('farmEventBossLoop: leave room');
			const endResult = macroService.findPattern([patterns.battle.next, patterns.battle.replay, patterns.battle.next3]);
			logger.debug('endResult.path: ' + endResult.path);
			switch (endResult.path) {
				case 'battle.next':
					macroService.pollPattern(patterns.battle.leaveRoom, { doClick: true, clickPattern: patterns.battle.next, predicatePattern: patterns.titles.bossMulti });
					break;
				case 'battle.replay':
					macroService.pollPattern(patterns.battle.replay, { doClick: true, clickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp], predicatePattern: patterns.battle.replay.prompt });
					sleep(500);
					macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: [patterns.battle.report] });
					result.numBattles++;
					break;
				case 'battle.next3':
					macroService.pollPattern(patterns.battle.next3, { doClick: true, predicatePattern: patterns.titles.bossBattle });
					break;
			}
			break;
		case 'quest.events.bossBattle.prompt.notEnoughBossTickets':
			result.message = 'Not enough boss tickets...';
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');