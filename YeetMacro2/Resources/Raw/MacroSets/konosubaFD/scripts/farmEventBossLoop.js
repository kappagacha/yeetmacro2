// @position=8
// Farm event boss in a loop. (handles single and multiplayer boss)
// See options for party names
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report, patterns.titles.bossBattle, patterns.titles.bossMulti, patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets];
let isBossMulti = false;
const result = { numBattles: 0 };
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('farmEventBossLoop: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventBossLoop: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventBossLoop: start farm');
			const bossBattleResult = macroService.PollPattern(patterns.quest.events.bossBattle, { DoClick: true, PredicatePattern: [patterns.quest.events.bossBattle.prompt.chooseBattleStyle, patterns.titles.bossBattle] });
			if (bossBattleResult.PredicatePath === 'quest.events.bossBattle.prompt.chooseBattleStyle') {
				macroService.PollPattern(patterns.quest.events.bossBattle.multi, { DoClick: true, ClickPattern: patterns.general.next, PredicatePattern: patterns.titles.bossMulti });
			}
			break;
		case 'titles.bossBattle':
			const dailyAttemptResult = macroService.FindPattern(patterns.quest.events.bossBattle.dailyAttempt);
			if (dailyAttemptResult.IsSuccess) {
				macroService.PollPattern(patterns.quest.events.bossBattle.expert, { DoClick: true, PredicatePattern: patterns.battle.prepare });
				macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			} else {
				const hardResult = macroService.PollPattern(patterns.quest.events.bossBattle.hard, { DoClick: true, PredicatePattern: [patterns.battle.prepare, patterns.quest.events.bossBattle.notEnoughTickets] });
				if (hardResult.PredicatePath === 'quest.events.bossBattle.notEnoughTickets') {
					result.message = 'Not enough boss tickets...';
					return result;
				}
				let currentCost = macroService.GetText(patterns.quest.events.bossBattle.cost);
				for (let i = 0; macroService.IsRunning && i < 9; i++) {
					const addCostDisabledResult = macroService.FindPattern(patterns.quest.events.bossBattle.addCost.disabled);
					if (addCostDisabledResult.IsSuccess) {
						break;
					}
					macroService.ClickPattern(patterns.quest.events.bossBattle.addCost);
					sleep(500);
					currentCost = macroService.GetText(patterns.quest.events.bossBattle.cost);
				}
				logger.debug(`currentCost: ${currentCost}`);
				if (currentCost == 1) {
					result.message = 'Not enough boss tickets...';
					return result;
				}
				const prepareResult = macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: [patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets] });
				if (prepareResult.PredicatePath === 'events.bossBattle.prompt.notEnoughBossTickets') {
					result.message = 'Not enough boss tickets...';
					return result;
				}
				macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			}
			break;
		case 'titles.bossMulti':
			logger.info('farmEventBossLoop: max cost');
			isBossMulti = true;
			macroService.PollPattern(patterns.quest.events.bossBattle.extreme, { DoClick: true, PredicatePattern: patterns.battle.prepare });
			const notEnoughTicketsResult = macroService.FindPattern(patterns.quest.events.bossBattle.notEnoughTickets);
			if (notEnoughTicketsResult.IsSuccess) {
				result.message = 'Not enough boss tickets...';
				return result;
			}

			let currentCost = macroService.GetText(patterns.quest.events.bossBattle.cost);
			for (let i = 0; macroService.IsRunning && i < 9; i++) {
				const addCostDisabledResult = macroService.FindPattern(patterns.quest.events.bossBattle.addCost.disabled);
				if (addCostDisabledResult.IsSuccess) {
					break;
				}
				macroService.ClickPattern(patterns.quest.events.bossBattle.addCost);
				sleep(500);
				currentCost = macroService.GetText(patterns.quest.events.bossBattle.cost);
			}
			logger.debug(`currentCost: ${currentCost}`);
			if (currentCost == 1) {
				result.message = 'Not enough boss tickets...';
				return result;
			}
			const prepareResult = macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: [patterns.titles.party, patterns.quest.events.bossBattle.prompt.notEnoughBossTickets] });
			if (prepareResult.PredicatePath === 'events.bossBattle.prompt.notEnoughBossTickets') {
				result.message = 'Not enough boss tickets...';
				return result;
			}
			break;
		case 'titles.party':
			logger.info('farmEventBossLoop: select party');
			const targetPartyName = settings.farmEventBossLoop.party.Value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement(settings.farmEventBossLoop.recommendedElement);
			} else {
				selectParty(targetPartyName);
			}

			sleep(500);
			macroService.PollPattern([patterns.battle.joinRoom, patterns.battle.begin], { DoClick: true, PredicatePattern: patterns.battle.report });
			result.numBattles++;
			break;
		case 'battle.report':
			logger.info('farmEventBossLoop: leave room');
			const endResult = macroService.FindPattern([patterns.battle.next, patterns.battle.replay, patterns.battle.next2]);
			logger.debug('endResult.Path: ' + endResult.Path);
			switch (endResult.Path) {
				case 'battle.next':
					macroService.PollPattern(patterns.battle.leaveRoom, { DoClick: true, ClickPattern: patterns.battle.next, PredicatePattern: patterns.titles.bossMulti });
					break;
				case 'battle.replay':
					macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp], PredicatePattern: patterns.battle.replay.prompt });
					sleep(500);
					macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: [patterns.battle.report] });
					result.numBattles++;
					break;
				case 'battle.next2':
					macroService.PollPattern(patterns.battle.next2, { DoClick: true, PredicatePattern: patterns.titles.bossBattle });
					break;
			}
			break;
		case 'quest.events.bossBattle.prompt.notEnoughBossTickets':
			result.message = 'Not enough boss tickets...';
			return result;
	}

	sleep(1_000);
}

return result;