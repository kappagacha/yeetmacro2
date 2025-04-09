// @position=6
// Battle in arena until out of arena tickets
// Will prioritize Memorial Match
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.arena.calculationsInProgress, patterns.arena.challenge1, patterns.arena.matchOpponent, patterns.titles.arena];
const daily = dailyManager.GetCurrentDaily();
const weekly = weeklyManager.GetCurrentWeekly();
const teamSlot = settings.doArena.teamSlot.Value;
const cpThresholdIsEnabled = settings.doArena.cpThreshold.IsEnabled;
const autoDetectCpThreshold = settings.doArena.autoDetectCpThreshold.Value;
const clickPattern = [patterns.arena.defendReport.close, patterns.arena.newLeague, patterns.arena.tapEmptySpace];
const dayOfWeek = weeklyManager.GetDayOfWeek();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPattern });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doArena: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doArena: click arena');
			macroService.ClickPattern(patterns.adventure.arena);
			sleep(500);
			break;
		case 'arena.calculationsInProgress':
			return;
		case 'titles.arena':
			const numTicketsZeroResult1 = macroService.FindPattern(patterns.arena.numTicketsZero)
			if (numTicketsZeroResult1.IsSuccess) {
				return;
			} else {
				macroService.ClickPattern(patterns.arena.arena);
			}
			break;
		case 'arena.challenge1':
		case 'arena.matchOpponent':
			const numTicketsZeroResult2 = macroService.FindPattern(patterns.arena.numTicketsZero2)
			if (numTicketsZeroResult2.IsSuccess) {
				return;
			}

			// At least Thursday
			if (dayOfWeek >= 4 && !weekly.claimWeeklyPlayReward.done.IsChecked) {	
				const claimWeeklyPlayRewardNotificationResult = macroService.FindPattern(patterns.arena.claimWeeklyPlayReward.notification);
				if (claimWeeklyPlayRewardNotificationResult.IsSuccess) {
					macroService.PollPattern(patterns.arena.claimWeeklyPlayReward.notification, { DoClick: true, ClickPattern: clickPattern, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, ClickPattern: clickPattern, PredicatePattern: patterns.titles.arena });
					macroService.IsRunning && (weekly.claimWeeklyPlayReward.done.IsChecked = true);
				}
			}

			const memorialMatchNotificationResult = macroService.FindPattern(patterns.arena.memorialMatch.notification);
			if (memorialMatchNotificationResult.IsSuccess) {
				logger.info('doArena: memorial match');
				macroService.PollPattern(patterns.arena.memorialMatch.notification, { DoClick: true, ClickPattern: clickPattern, PredicatePattern: [patterns.arena.memorialMatch.sweepAll, patterns.arena.memorialMatch.sweepAll.disabled] });

				if (settings.doArena.skipMemorialMatch.Value) {
					macroService.PollPattern(patterns.arena.memorialMatch.sweepAll, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.sweepAll.title });
					macroService.PollPattern(patterns.arena.memorialMatch.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.sweepAll.ok });
					macroService.PollPattern(patterns.arena.memorialMatch.sweepAll.ok, { DoClick: true, InversePredicatePattern: patterns.arena.memorialMatch.sweepAll.ok });

					//macroService.PollPattern(patterns.arena.memorialMatch.sweepAll, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.sweepAll.title });
					//for (let i = 0; i < 3; i++) {
					//	let uncheckedResult = macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.unchecked, { Limit: 3 });
					//	if (uncheckedResult.IsSuccess) {
					//		for (const p of uncheckedResult.Points) {
					//			if (macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.negative).IsSuccess) break;
					//			macroService.DoClick(p);
					//			sleep(1_000);
					//		}
					//	}
					//	macroService.DoSwipe({ X: 1300, Y: 730 }, { X: 1300, Y: 230 });
					//	sleep(1_000);

					//	if (macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.negative).IsSuccess) break;
					//}

					//if (macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.negative).IsSuccess) {
					//	for (let i = 0; i < 3; i++) {
					//		let checkedResult = macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.checked, { Limit: 3 });
					//		if (checkedResult.IsSuccess) {
					//			for (const p of checkedResult.Points) {
					//				if (!macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.negative).IsSuccess) break;

					//				macroService.DoClick(p);
					//				sleep(1_000);
					//			}
					//		}
					//		macroService.DoSwipe({ X: 1300, Y: 230 }, { X: 1300, Y: 730 });
					//		sleep(1_000);

					//		if (!macroService.FindPattern(patterns.arena.memorialMatch.sweepAll.negative).IsSuccess) break;
					//	}
					//}

					//macroService.PollPattern(patterns.arena.memorialMatch.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.sweepAll.ok });
					//macroService.PollPattern(patterns.arena.memorialMatch.sweepAll.ok, { DoClick: true, PredicatePattern: [patterns.arena.memorialMatch.sweepAll, patterns.arena.memorialMatch.sweepAll.disabled] });
				} else {
					macroService.PollPattern(patterns.arena.challenge1, { DoClick: true, PredicatePattern: patterns.arena.enter });
					selectTeam(teamSlot, { applyPreset: true });

					macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
					if (macroService.IsRunning) {
						daily.doArena.count.Count++;
					}
					macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: [patterns.arena.tapEmptySpace, patterns.arena.defendReport.close], PredicatePattern: patterns.titles.arena });
				}
			} else {
				logger.info('doArena: normal match');
				macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, ClickPattern: clickPattern, PredicatePattern: patterns.arena.matchOpponent.selected });
				if (cpThresholdIsEnabled) {
					const cpThreshold = settings.doArena.cpThreshold.Value;
					const challenge1CP = macroService.FindText(patterns.arena.challenge1.cp);
					const challenge2CP = macroService.FindText(patterns.arena.challenge2.cp);
					const challenge3CP = macroService.FindText(patterns.arena.challenge3.cp);
					const challenges = [
						Number((challenge1CP.slice(0, challenge1CP.length - 4) + challenge1CP.slice(-3))?.replace(/[^0-9]/g, '')),
						Number((challenge2CP.slice(0, challenge2CP.length - 4) + challenge2CP.slice(-3))?.replace(/[^0-9]/g, '')),
						Number((challenge3CP.slice(0, challenge3CP.length - 4) + challenge3CP.slice(-3))?.replace(/[^0-9]/g, '')),
					];
					// choose the first challenge (most reward) that is equal to or under threshold
					const challengesToWithinThreshold = challenges.map(c => c && c <= cpThreshold);
					let targetIdx = challengesToWithinThreshold.findIndex(cwt => cwt);
					if (targetIdx === -1) targetIdx = 2;
					//return targetIdx;		// for testing
					//const maxIdx = challenges.reduce((maxIdx, val, idx) => val && val <= cpThreshold && val > challenges[maxIdx] ? idx : maxIdx, 2);
					
					logger.info(`doArena: normal match challenge ${targetIdx + 1}  [${cpThreshold}] ~${challenge1CP}~~${challenge2CP}~~${challenge3CP}~`);
					macroService.PollPattern(patterns.arena[`challenge${targetIdx + 1}`], { DoClick: true, ClickPattern: clickPattern, PredicatePattern: patterns.arena.enter });
				} else {
					logger.info('doArena: normal match challenge 3');
					macroService.PollPattern(patterns.arena.challenge3, { DoClick: true, ClickPattern: clickPattern, PredicatePattern: patterns.arena.enter });
				}

				selectTeam(teamSlot, { applyPreset: true });
				if (autoDetectCpThreshold) {
					const cpText = macroService.FindText(patterns.battle.cp);
					const currentCp = Number(cpText.slice(0, -4).slice(1) + cpText.slice(-3));
					
					if (currentCp) {
						logger.info(`doArena: set settings.doArena.cpThreshold to ${currentCp}`);
						settings.doArena.cpThreshold.Value = currentCp;
						settings.doArena.autoDetectCpThreshold.Value = false;
					}
				}

				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				//macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				//macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: [patterns.arena.tapEmptySpace, patterns.arena.defendReport.close], PredicatePattern: patterns.titles.arena });
			}
			break;
	}
	sleep(1_000);
}
