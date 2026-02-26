// @tags=favorites
// @position=-1
// Do pursuit operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.irregularExtermination.pursuitOperation, patterns.titles.pursuitOperation];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doPursuitOperation.teamSlot.Value;
const publishToPublic = settings.doPursuitOperation.publishToPublic.Value;
let targetOperation = settings.doPursuitOperation.targetOperation.Value;
let lastOperation;

let teamRestored = false;
let isRotateOperation = targetOperation === 'rotate';
let isAutoOperation = targetOperation === 'auto';

const operations = ['blockbuster', 'mutatedWyvre', 'ironStretcher', 'irregularQueen'];
const defaultOperation = 'irregularQueen';
let operationToPoints = {
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
	},
	irregularQueen: {
		target: 6000,
		current: 0
	},
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
			const pointExchangeNotificationResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange.notification, { TimeoutMs: 1_000 });
			if (pointExchangeNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll });
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.general.back });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation, PrimaryClickPredicatePattern: patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll });
			}

			const infiltrationOperationCompleteResult = macroService.PollPattern(patterns.irregularExtermination.infiltrationOperationComplete, { TimeoutMs: 1_000 });
			if (infiltrationOperationCompleteResult.IsSuccess) {
				operationToPoints.irregularQueen.target = 8000;
				operationToPoints.blockbuster.target = 8000;
				operationToPoints.mutatedWyvre.target = 8000;
				operationToPoints.ironStretcher.target = 8000;
			}

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
				macroService.IsRunning && daily.doPursuitOperation.count.Count++;
				return;
			}

			if (isAutoOperation) {
				operations.forEach(op => {
					operationToPoints[op].cellRewardUp = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[op].cellRewardUp).IsSuccess;
					operationToPoints[op].current = Number(macroService.FindText(patterns.irregularExtermination.pursuitOperation[op].currentPoints).replace(/[, ]/g, ''));
				});

				// If all operations meet target, use default
				const allComplete = operations.every(op => operationToPoints[op].current >= operationToPoints[op].target);
				if (allComplete) {
					targetOperation = defaultOperation;
				} else {
					targetOperation = Object.entries(operationToPoints).reduce((acc, [op, { target, current, cellRewardUp }]) => {
						// Priority 1: reward available on incomplete operation
						if (cellRewardUp && current < target) {
							if (!acc || acc.priority > 1 || (acc.priority === 1 && acc.current > current)) return { priority: 1, op, current };
							return acc;
						}
						// Priority 2: incomplete (only if no priority 1 found)
						if (current < target) {
							if (!acc || acc.priority === 2 && acc.current > current) return { priority: 2, op, current };
							return acc;
						}
						return acc;
					}, null).op;
				}
			}

			if (isRotateOperation) {
				const lastOperationIndex = operations.findIndex(o => o === settings.doPursuitOperation.lastOperation.Value);
				targetOperation = operations[(lastOperationIndex + 1) % operations.length];
			}

			if (lastOperation != targetOperation) teamRestored = false;

			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
			lastOperation = targetOperation;
			macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.createOperation, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });

			const sweepResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.auto, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.auto.mustBattlePrompt, patterns.irregularExtermination.pursuitOperation.auto.ok] });
			if (sweepResult.PredicatePath === 'irregularExtermination.pursuitOperation.auto.ok') {
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.auto.ok, { DoClick: true, PredicatePattern: patterns.titles.pursuitOperation });
				break;
			}
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