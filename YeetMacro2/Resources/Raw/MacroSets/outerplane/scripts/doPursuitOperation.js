// @isFavorite
// @position=-1
// Do pursuit operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.irregularExtermination.pursuitOperation, patterns.titles.pursuitOperation];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doPursuitOperation.teamSlot.Value;
const targetOperation = settings.doPursuitOperation.targetOperation.Value;
if (daily.doPursuitOperation.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doPursuitOperation: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doPursuitOperation: click irregular extermination');
			macroService.ClickPattern(patterns.adventure.irregularExtermination);
			sleep(500);
			break;
		case 'irregularExtermination.pursuitOperation':
			logger.info('doPursuitOperation: click move');
			macroService.ClickPattern(patterns.irregularExtermination.pursuitOperation.move);
			sleep(500);
			break;
		case 'titles.pursuitOperation':
			const zeroOperationsResult = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation.zeroOperations);
			if (zeroOperationsResult.IsSuccess) {
				macroService.IsRunning && (daily.doPursuitOperation.done.IsChecked = true);
				return;
			}

			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.createOperation, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam(teamSlot);
			macroService.PollPattern(patterns.battle.battleRecord, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam });
			macroService.PollPattern(patterns.battle.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam.ok });
			macroService.PollPattern(patterns.battle.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.enter });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });

			const friendsOrGuildResult = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation.friendsOrGuild);
			if (friendsOrGuildResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.friendsOrGuild, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.ok });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.ok, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });
			}
			while (macroService.IsRunning && !macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[targetOperation]).IsSuccess) {
				macroService.ClickPattern(patterns.general.back);
				sleep(1_000);
			}
			break;
	}
	sleep(1_000);
}