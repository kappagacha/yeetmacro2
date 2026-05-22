// @tags=favorites
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.monadGate.gateEntryDevice, patterns.battle.enter];
const clickPatterns = [patterns.monadGate.singularityRepel, patterns.general.tapEmptySpace, patterns.monadGate.singularityRepel.teamsSetup];

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doDimensionalSingularity: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doDimensionalSingularity: click monad gate');
			macroService.ClickPattern(patterns.adventure.monadGate);
			sleep(500);
			break
		case 'battle.enter':
			logger.info('doDimensionalSingularity: do dimensional singularity');

			if (macroService.FindPattern(patterns.monadGate.singularityRepel.zeroAttemptsLeft).IsSuccess)
				return;

			selectTeam(9);
			// restore lineup
			macroService.PollPattern(patterns.battle.lineupRecord, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord.restoreTeam });
			macroService.PollPattern(patterns.battle.lineupRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord.restoreTeam.ok });
			macroService.PollPattern(patterns.battle.lineupRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord });
			// do battle
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.saveAndExit });
			macroService.PollPattern(patterns.battle.saveAndExit, { DoClick: true, PredicatePattern: patterns.battle.saveAndExit.confirm });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.singularityRepel.teamsSetup });
			break;
	}
	sleep(1_000);
}
