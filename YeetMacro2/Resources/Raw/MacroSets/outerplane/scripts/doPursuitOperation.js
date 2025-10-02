// @isFavorite
// @position=-1
// Do pursuit operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.irregularExtermination.pursuitOperation, patterns.titles.pursuitOperation];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doPursuitOperation.teamSlot.Value;
const publishToPublic = settings.doPursuitOperation.publishToPublic.Value;
let targetOperation = settings.doPursuitOperation.targetOperation.Value;

//if (daily.doPursuitOperation.done.IsChecked) {
//	return "Script already completed. Uncheck done to override daily flag.";
//}

let teamRestored = false;
let isRotateOperation = targetOperation === 'rotate';
let isAutoOperation = targetOperation === 'auto';

//const operations = ['ironStretcher', 'irregularQueen', 'blockbuster', 'mutatedWyvre'];
const operations = ['irregularQueen', 'blockbuster', 'mutatedWyvre', 'ironStretcher'];
let operationToPoints = {
	irregularQueen: {
		target: 6000,
		current: 0
	},
	blockbuster: {
		target: 6000,
		current: 0
	},
	mutatedWyvre: {
		target: 6000,
		current: 0
	},
	ironStretcher: {
		target: 6000,
		current: 0
	}
};

goToLobby();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
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
			const exterminationRecordNotificationResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.notification, { TimeoutMs: 1_000 });
			if (exterminationRecordNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.notification, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.exterminationRecords.close });
				sleep(1_000);
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.close, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.exterminationRecords.reward });
				sleep(2_000);
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.exterminationRecords.reward, { DoClick: true, ClickOffset: { X: -100 }, PredicatePattern: patterns.titles.pursuitOperation });
			}

			const zeroOperationsResult = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation.zeroOperations);
			if (zeroOperationsResult.IsSuccess) {
				//macroService.IsRunning && (daily.doPursuitOperation.done.IsChecked = true);
				macroService.IsRunning && daily.doPursuitOperation.count.Count++;
				return;
			}

			if (isAutoOperation) {
				operations.forEach(op => operationToPoints[op].current = Number(macroService.FindText(patterns.irregularExtermination.pursuitOperation[op].currentPoints).replace(/[, ]/g, '')));
				targetOperation = Object.entries(operationToPoints).reduce((targetOperation, [op, { target, current }]) => {
					if (targetOperation || current > target) return targetOperation;
					return op;
				}, null) || 'ironStretcher';
			}

			if (isRotateOperation) {
				const lastOperationIndex = operations.findIndex(o => o === settings.doPursuitOperation.lastOperation.Value);
				targetOperation = operations[(lastOperationIndex + 1) % operations.length];
			}

			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.createOperation, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam(teamSlot);

			if (isRotateOperation || !teamRestored || settings.applyPreset.lastApplied.Value !== teamSlot) {
				macroService.PollPattern(patterns.battle.battleRecord, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam });
				macroService.PollPattern(patterns.battle.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.battle.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.enter });
				setChainOrder();

				teamRestored = true;
				macroService.IsRunning && (settings.applyPreset.lastApplied.Value = teamSlot);
			}

			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.next });
			while (macroService.IsRunning) {
				const nextResult = macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: [patterns.battle.exit, patterns.battle.restore] });
				// Accidentally hit retry
				if (nextResult.PredicatePath === 'battle.restore') {
					macroService.ClickPattern(patterns.general.back);
					sleep(1_000);
				} else {
					break;
				}
			}

			const battleExitResult = macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.irregularExtermination.pursuitOperation.selectTeam2, patterns.irregularExtermination.pursuitOperation[targetOperation]] });
			macroService.IsRunning && (settings.doPursuitOperation.lastOperation.Value = targetOperation);

			if (battleExitResult.PredicatePath === `irregularExtermination.pursuitOperation.${targetOperation}`) {
				continue;
			}

			macroService.PollPattern([patterns.irregularExtermination.pursuitOperation.friendsOrGuild, patterns.irregularExtermination.pursuitOperation.selectTeam2, patterns.irregularExtermination.pursuitOperation.selectTeam]);
			const friendsOrGuildResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.friendsOrGuild, { TimeoutMs: 3_000 });
			if (friendsOrGuildResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.friendsOrGuild, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.ok });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.ok, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });
				sleep(1_000);
			}

			macroService.PollPattern([patterns.irregularExtermination.pursuitOperation.friendsOrGuild, patterns.irregularExtermination.pursuitOperation.selectTeam2, patterns.irregularExtermination.pursuitOperation.selectTeam]);
			const publicResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.public, { TimeoutMs: 3_000 });
			if (publishToPublic && publicResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.public, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.ok });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.ok, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam2 });
				sleep(1_000);
			}

			while (macroService.IsRunning && !macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[targetOperation]).IsSuccess) {
				macroService.ClickPattern(patterns.general.back);
				sleep(1_000);
			}
			break;
	}
	sleep(1_000);
}