// @position=5
// Auto or skip arena EX attempts. See options for party select names.
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
const skipBattle = settings.doBattleArenaEx.skipBattle.Value;
const daily = dailyManager.GetCurrentDaily();
if (daily.doBattleArenaEx.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.prompt.noteOk] });
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doBattleArenaEx: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doBattleArenaEx: click battle arena');
			macroService.ClickPattern(patterns.quest.battleArena);
			break;
		case 'titles.battleArena':
			logger.info('doBattleArenaEx: start arena');
			macroService.PollPattern(patterns.battleArena.tabs.arenaEX, { DoClick: true, ClickPattern: patterns.battleArena.prompt.noteOk, PredicatePattern: patterns.titles.battleArenaEX });
			sleep(500);
			macroService.PollPattern(patterns.battleArena.begin, { DoClick: true, ClickPattern: patterns.battleArena.prompt.noteOk, PredicatePattern: patterns.battleArena.exRank });
			sleep(500);
			if (skipBattle) {
				macroService.PollPattern(patterns.battleArena.skip, { DoClick: true, PredicatePattern: patterns.battleArena.skip.max });
				macroService.PollPattern(patterns.battleArena.skip.max, { DoClick: true, PredicatePattern: patterns.battleArena.skip.max.disabled });
				macroService.PollPattern(patterns.battleArena.skip.skipButton, {
					DoClick: true,
					ClickPattern: [patterns.battleArena.skip.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp, patterns.skipAll.skipComplete, patterns.skipAll.prompt.ok],
					PredicatePattern: patterns.titles.battleArena
				});
				if (macroService.IsRunning) {
					daily.doBattleArenaEx.done.IsChecked = true;
				}
				return;
			}
			macroService.PollPattern(patterns.battleArena.exRank, { DoClick: true, PredicatePattern: patterns.battle.prepare });
			sleep(500);
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.titles.party });
			break;
		case 'titles.party':
			logger.info('doBattleArenaEx: select party');
			const scoreBonusPatterns = ['physicalDamage', 'lowRarity', 'magicDamage', 'defence', 'characterBonus'].map(sb => patterns.battleArena.exScoreBonus[sb]);
			const scoreBonusResult = macroService.PollPattern(scoreBonusPatterns);
			const scoreBonus = scoreBonusResult.Path?.split('.').pop();
			logger.debug(`scoreBonus: ${scoreBonus}`);
			if (!scoreBonusResult.IsSuccess) {
				throw new Error('Could not detect score bonus...');
			}
			const scoreBonusPartyName = settings.doBattleArenaEx[scoreBonus]?.Value;
			if (!scoreBonusPartyName) {
				throw new Error(`Could not find scoreBonusPartyName for ${scoreBonusPartyName} in settings...`);
			}

			logger.debug(`scoreBonusPartyName: ${scoreBonusPartyName}`);
			selectParty(scoreBonusPartyName);
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doBattleArenaEx: restart');
			// 1st battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 2nd battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 3rd battle => battle.next (middle) => patterns.battle.replay.disabled

			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: [patterns.battle.replay, patterns.battle.next3] });
			if (macroService.IsRunning) {
				daily.doBattleArenaEx.count.Count++;
			}
			sleep(500);
			const replayResult = macroService.FindPattern(patterns.battle.replay);
			if (replayResult.IsSuccess) {
				logger.debug('doBattleArenaEx: found replay');
				macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.replay.ok });
				macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: patterns.battle.report });
			} else {
				logger.debug('doBattleArenaEx: found next3');
				const next3Result = macroService.PollPattern(patterns.battle.next3, { DoClick: true, ClickPattern: [patterns.battleArena.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: [patterns.titles.battleArena, patterns.battle.replay] });
				if (next3Result.PredicatePath === 'titles.battleArena') {
					if (macroService.IsRunning) {
						daily.doBattleArenaEx.done.IsChecked = true;
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