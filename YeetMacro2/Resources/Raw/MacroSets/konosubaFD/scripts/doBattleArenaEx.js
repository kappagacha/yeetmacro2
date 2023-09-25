const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
let done = false;
while (macroService.IsRunning && !done) {
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
			logger.info('doBattleArena: start arena');
			macroService.PollPattern(patterns.battleArena.tabs.arenaEX, { DoClick: true, ClickPattern: patterns.battleArena.prompt.noteOk, PredicatePattern: patterns.titles.battleArenaEX });
			sleep(500);
			macroService.PollPattern(patterns.battleArena.begin, { DoClick: true, PredicatePattern: patterns.battleArena.exRank });
			sleep(500);
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
				result = 'Could not detect score bonus...';
				done = true;
				break;
			}
			const scoreBonusPartyName = settings.party.arenaEX[scoreBonus]?.Value;
			if (!scoreBonusPartyName) {
				result = `Could not find scoreBonusPartyName for ${scoreBonusPartyName} in settings...`;
				done = true;
				break;
			}

			logger.debug(`scoreBonusPartyName: ${scoreBonusPartyName}`);
			if (!(selectParty(scoreBonusPartyName))) {
				result = `scoreBonusPartyName not found: ${scoreBonusPartyName}`;
				done = true;
				break;
			}
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doBattleArenaEx: restart');
			// 1st battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 2nd battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 3rd battle => battle.next (middle) => patterns.battle.replay.disabled

			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: [patterns.battle.replay, patterns.battle.next3] });
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
					done = true;
					break;
				}
				macroService.PollPattern(patterns.battle.replay, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.battle.replay.ok });
				macroService.PollPattern(patterns.battle.replay.ok, { DoClick: true, PredicatePattern: patterns.battle.report });
			}
			break;
	}

	sleep(1_000);
}
logger.info('Done...');