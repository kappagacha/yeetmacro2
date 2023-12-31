﻿// Farm event in a loop. See options to select between level 11 and 12
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.party, patterns.titles.events, patterns.battle.report];
const result = { numBattles: 0 };
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp] });
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('farmEventLoop: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('farmEventLoop: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('farmEventLoop: start farm');
			const targetFarmLevel = settings.farmEvent.targetLevel.Value ?? 12;
			macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
			sleep(500);
			macroService.PollPattern(patterns.quest.events.quest.normal[targetFarmLevel], { DoClick: true, PredicatePattern: patterns.titles.events });
			sleep(500);
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			sleep(500);
			break;
		case 'titles.party':
			logger.info('farmEventLoop: select party');
			const targetPartyName = settings.party.farmEventLoop.Value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement();
			} else {
				selectParty(targetPartyName);
			}

			sleep(500);
			const beginResult = macroService.PollPattern(patterns.battle.begin, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			result.numBattles++;
			if (beginResult.PredicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				return result;
			}
			break;
		case 'battle.report':
			logger.info('farmEventLoop: replay battle');
			macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.battle.next, patterns.battle.affinityLevelUp, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.replay.prompt });
			sleep(500);
			const replayResult = macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: [patterns.battle.report, patterns.stamina.prompt.recoverStamina] });
			result.numBattles++;
			if (replayResult.PredicatePath === 'stamina.prompt.recoverStamina') {
				result.message = 'Out of stamina...';
				return result;
			}
			break;
	}

	sleep(1_000);
}

return result;