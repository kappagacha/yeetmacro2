// @position=4
// Do all battle arena attempts. See options for party select names.
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
const daily = dailyManager.GetDaily();
if (daily.doBattleArena.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank, patterns.battleArena.prompt.noteOk] });
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doBattleArena: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doBattleArena: click battle arena');
			macroService.ClickPattern(patterns.quest.battleArena);
			break;
		case 'titles.battleArena':
			logger.info('doBattleArena: start arena');
			macroService.PollPattern(patterns.battleArena.begin, { DoClick: true, ClickPattern: [patterns.battleArena.prompt.noteOk, patterns.battleArena.rankResult ], PredicatePattern: patterns.battleArena.advanced });
			sleep(500);
			macroService.PollPattern(patterns.battleArena.advanced, { DoClick: true, ClickPattern: patterns.battleArena.prompt.noteOk, PredicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			break;
		case 'titles.party':
			logger.info('doBattleArena: select party');
			const targetPartyName = settings.doBattleArena.party.Value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement(settings.doBattleArena.recommendedElement);
			} else {
				selectParty(targetPartyName);
			}
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doBattleArena: restart');
			// 1st battle => battle.next (middle) => battle.next2 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
			// OR         => battle.next (middle) => battle.replay => battle.replay.ok
			// 2nd battle => battle.next (middle) => battle.next2 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
			// OR         => battle.next (middle) => battle.replay => battle.replay.ok
			// 3rd battle => battle.next (middle) => battle.next2 (right) => battleArena.prompt.ok => battle.replay.disabled
			// OR         => battle.next (middle) => battle.replay.disabled

			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: [patterns.battle.replay, patterns.battle.next2] });
			if (macroService.IsRunning) {
				daily.doBattleArena.count.Count++;
			}
			sleep(500);
			const replayResult = macroService.FindPattern(patterns.battle.replay);
			if (replayResult.IsSuccess) {
				logger.debug('doBattleArena: found replay');
				macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.replay.ok });
				macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: patterns.battle.report });
			} else {
				logger.debug('doBattleArena: found next2');
				const next2Result = macroService.PollPattern(patterns.battle.next2, { DoClick: true, ClickPattern: [patterns.battleArena.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: [patterns.titles.battleArena, patterns.battle.replay] });
				if (next2Result.PredicatePath === 'titles.battleArena') {
					if (macroService.IsRunning) {
						daily.doBattleArena.done.IsChecked = true;
					}
					return;
				}
				macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.replay.ok });
				macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: patterns.battle.report });
			}
			break;
	}

	sleep(1_000);
}