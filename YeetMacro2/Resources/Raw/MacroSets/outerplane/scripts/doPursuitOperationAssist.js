// @isFavorite
// @position=-1
// Do pursuit operation assist
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.irregularExtermination.pursuitOperation, patterns.titles.pursuitOperation];
const teamSlot = settings.doPursuitOperationAssist.teamSlot.Value;
const targetOperation = settings.doPursuitOperationAssist.targetOperation.Value;
const refillStaminaAmount = settings.doPursuitOperationAssist.refillStaminaAmount.Value;

let teamRestored = false;

goToLobby();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			if (refillStaminaAmount) {
				refillStamina(refillStaminaAmount);
				goToLobby();
			}

			logger.info('doPursuitOperationAssist: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doPursuitOperationAssist: click irregular extermination');
			macroService.ClickPattern(patterns.adventure.irregularExtermination);
			sleep(500);
			break;
		case 'irregularExtermination.pursuitOperation':
			logger.info('doPursuitOperationAssist: click move');
			macroService.ClickPattern(patterns.irregularExtermination.pursuitOperation.move);
			sleep(500);
			break;
		case 'titles.pursuitOperation':
			const exterminationRecordNotificationResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.notification, { TimeoutMs: 1_000 });
			if (exterminationRecordNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.notification, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.exterminationRecords.close });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.close, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.exterminationRecords.reward });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.reward, { DoClick: true, ClickOffset: { X: -100 }, PredicatePattern: patterns.titles.pursuitOperation });
			}

			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.recruiting, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.recruiting.selected });
			sleep(1_000);
			const veryHardResult = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation.recruiting.veryHard);
			if (veryHardResult.IsSuccess) {
				const enterPattern = macroService.ClonePattern(patterns.irregularExtermination.pursuitOperation.recruiting.enter, { CenterY: veryHardResult.Point.Y + 5, Padding: 10, Path: `irregularExtermination.pursuitOperation.recruiting.enter_y${veryHardResult.Point.Y}` });
				macroService.PollPattern(enterPattern, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
				selectTeam(teamSlot);

				if (!teamRestored) {
					macroService.PollPattern(patterns.battle.battleRecord, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam });
					macroService.PollPattern(patterns.battle.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam.ok });
					macroService.PollPattern(patterns.battle.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.enter });
					teamRestored = true;
					macroService.IsRunning && (settings.applyPreset.lastApplied.Value = teamSlot);
				}

				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.next });
				macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
				macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.titles.pursuitOperation] });
				while (macroService.IsRunning && !macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[targetOperation]).IsSuccess) {
					macroService.ClickPattern(patterns.general.back);
					sleep(1_000);
				}
			} else {
				return;
			}
	}
	sleep(1_000);
}