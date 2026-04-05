// @tags=favorites
// @position=-1
// Do pursuit operation (normal or assist mode)

function doPursuitOperation(mode) {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.irregularExtermination.pursuitOperation, patterns.titles.pursuitOperation];
	const daily = dailyManager.GetCurrentDaily();
	if (!mode) mode = settings.doPursuitOperation.mode.Value;

	if (mode !== 'normal' && mode !== 'assist') {
		throw new Error(`Invalid mode: ${mode}. Expected 'normal' or 'assist'.`);
	}

	const isAssist = mode === 'assist';
	const teamSlot = settings.doPursuitOperation.teamSlot.Value;
	const targetOperation = isAssist ? settings.doPursuitOperation.assist.targetOperation.Value : settings.doPursuitOperation.targetOperation.Value;
	const refillStaminaAmount = isAssist ? settings.doPursuitOperation.assist.refillStaminaAmount.Value : null;
	const publishToPublic = !isAssist ? settings.doPursuitOperation.publishToPublic.Value : null;

	let teamRestored = false;
	let lastOperation;

	let isRotateOperation = !isAssist && targetOperation === 'rotate';
	let isAutoOperation = !isAssist && targetOperation === 'auto';

	const operations = ['blockbuster', 'mutatedWyvre', 'ironStretcher', 'irregularQueen'];
	const defaultOperation = 'irregularQueen';
	let operationToPoints = {
		blockbuster: { target: 6000, current: 0 },
		mutatedWyvre: { target: 6000, current: 0 },
		ironStretcher: { target: 6000, current: 0 },
		irregularQueen: { target: 6000, current: 0 },
	};

	if (isAssist && refillStaminaAmount) {
		goToLobby();
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				if (isAssist && refillStaminaAmount) {
					refillStamina(refillStaminaAmount);
					goToLobby();
				}

				logger.info(`doPursuitOperationPrime(${mode}): click adventure tab`);
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info(`doPursuitOperationPrime(${mode}): click irregular extermination`);
				macroService.ClickPattern(patterns.adventure.irregularExtermination);
				sleep(500);
				break;
			case 'irregularExtermination.pursuitOperation':
				if (!isAssist) {
					const pointExchangeNotificationResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange.notification, { TimeoutMs: 1_000 });
					if (pointExchangeNotificationResult.IsSuccess) {
						const pointExchangeResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll, patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll.disabled] });

						if (pointExchangeResult.PredicatePath === 'irregularExtermination.pursuitOperation.pointExchange.receiveAll') {
							logger.info('doPursuitOperationPrime: enabled receive all detected');
							macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll, { DoClick: true, PredicatePattern: [patterns.general.tapEmptySpace, patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll.disabled] });
							sleep(500);
							macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.general.back, patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll.disabled] });
							sleep(500);
							macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation, PrimaryClickPredicatePattern: [patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll, patterns.irregularExtermination.pursuitOperation.pointExchange.receiveAll.disabled] });
						}
					}

					const infiltrationOperationCompleteResult = macroService.PollPattern(patterns.irregularExtermination.infiltrationOperationComplete, { TimeoutMs: 1_000 });
					if (infiltrationOperationCompleteResult.IsSuccess) {
						operationToPoints.irregularQueen.target = 8000;
						operationToPoints.blockbuster.target = 8000;
						operationToPoints.mutatedWyvre.target = 8000;
						operationToPoints.ironStretcher.target = 8000;
					}
				}

				logger.info(`doPursuitOperationPrime(${mode}): click move`);
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

				if (isAssist) {
					handleAssistMode();
					return;
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

					const allComplete = operations.every(op => operationToPoints[op].current >= operationToPoints[op].target);
					if (allComplete) {
						lastOperation = defaultOperation;
					} else {
						lastOperation = Object.entries(operationToPoints).reduce((acc, [op, { target, current, cellRewardUp }]) => {
							if (cellRewardUp && current < target) {
								if (!acc || acc.priority > 1 || (acc.priority === 1 && acc.current > current)) return { priority: 1, op, current };
								return acc;
							}
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
					lastOperation = operations[(lastOperationIndex + 1) % operations.length];
				}

				if (lastOperation != targetOperation) teamRestored = false;

				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[lastOperation || targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
				const currentOperation = lastOperation || targetOperation;
				lastOperation = currentOperation;
				macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.createOperation, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.selectTeam });

				const sweepResult = macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.sweep, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.sweep.mustBattlePrompt, patterns.irregularExtermination.pursuitOperation.sweep.ok] });
				if (sweepResult.PredicatePath === 'irregularExtermination.pursuitOperation.sweep.ok') {
					macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.irregularExtermination.pursuitOperation.sweep.ok });
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
					if (nextResult.PredicatePath === 'battle.restore') {
						macroService.ClickPattern(patterns.general.back);
						sleep(1_000);
					} else {
						break;
					}
				}

				const battleExitResult = macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.irregularExtermination.pursuitOperation.selectTeam2, patterns.irregularExtermination.pursuitOperation[currentOperation]] });
				macroService.IsRunning && !isAssist && (settings.doPursuitOperation.lastOperation.Value = currentOperation);

				if (battleExitResult.PredicatePath === `irregularExtermination.pursuitOperation.${currentOperation}`) {
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

				while (macroService.IsRunning && !macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[currentOperation]).IsSuccess) {
					macroService.ClickPattern(patterns.general.back);
					sleep(1_000);
				}
				break;
		}
		sleep(1_000);
	}

	function handleAssistMode() {
		macroService.PollPattern(patterns.irregularExtermination.pursuitOperation[targetOperation], { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.createOperation });
		macroService.PollPattern(patterns.irregularExtermination.pursuitOperation.recruiting, { DoClick: true, PredicatePattern: patterns.irregularExtermination.pursuitOperation.recruiting.selected });
		sleep(1_000);
		const veryHardResult = macroService.FindPattern(patterns.irregularExtermination.pursuitOperation.recruiting.veryHard);
		if (!veryHardResult.IsSuccess) return;

		const enterPattern = macroService.ClonePattern(patterns.irregularExtermination.pursuitOperation.recruiting.enter, { CenterY: veryHardResult.Point.Y + 5, Padding: 10, Path: `irregularExtermination.pursuitOperation.recruiting.enter_y${veryHardResult.Point.Y}` });
		macroService.PollPattern(enterPattern, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.irregularExtermination.pursuitOperation.selectTeam2] });
		macroService.PollPattern([patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.irregularExtermination.pursuitOperation.selectTeam2], { DoClick: true, PredicatePattern: patterns.battle.enter });
		selectTeam(teamSlot);

		if (!teamRestored) {
			macroService.PollPattern(patterns.battle.battleRecord, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam });
			macroService.PollPattern(patterns.battle.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.battleRecord.restoreTeam.ok });
			macroService.PollPattern(patterns.battle.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.enter });
			setChainOrder();
			teamRestored = true;
			macroService.IsRunning && (settings.applyPreset.lastApplied.Value = teamSlot);
		}

		macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.next });
		macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
		macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.irregularExtermination.pursuitOperation.selectTeam2, patterns.irregularExtermination.pursuitOperation.selectTeam, patterns.titles.pursuitOperation] });
		while (macroService.IsRunning && !macroService.FindPattern(patterns.irregularExtermination.pursuitOperation[targetOperation]).IsSuccess) {
			macroService.ClickPattern(patterns.general.back);
			sleep(1_000);
		}
	}
}
