// @position=6
// Battle in arena until out of arena tickets
// Will prioritize Memorial Match
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.arena.challenge1, patterns.arena.matchOpponent, patterns.titles.arena];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doArena.teamSlot.Value;
const cpThresholdIsEnabled = settings.doArena.cpThreshold.IsEnabled;
const autoDetectCpThreshold = settings.doArena.autoDetectCpThreshold.Value;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.arena.defendReport.close, patterns.arena.newLeague, patterns.arena.tapEmptySpace] });
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
		case 'titles.arena':
			macroService.ClickPattern([patterns.arena.arena]);
			sleep(500);
			break;
		case 'arena.challenge1':
		case 'arena.matchOpponent':
			const numTicketsZeroResult = macroService.PollPattern(patterns.arena.numTicketsZero, { TimeoutMs: 1_500 })
			if (numTicketsZeroResult.IsSuccess) {
				return;
			}

			const memorialMatchNotificationResult = macroService.FindPattern(patterns.arena.memorialMatch.notification);
			if (memorialMatchNotificationResult.IsSuccess) {
				logger.info('doArena: memorial match');
				macroService.PollPattern(patterns.arena.memorialMatch.notification, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.selected });
				macroService.PollPattern(patterns.arena.challenge1, { DoClick: true, PredicatePattern: patterns.arena.enter });

				if (autoDetectCpThreshold) {
					const currentCp = selectTeam(teamSlot, true);
					if (currentCp) {
						logger.info(`doArena: set settings.doArena.cpThreshold to ${currentCp}`);
						settings.doArena.cpThreshold.Value = currentCp;
						settings.doArena.autoDetectCpThreshold.Value = false;
					}
				} else {
					selectTeam(teamSlot);
				}

				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: [patterns.arena.tapEmptySpace, patterns.arena.defendReport.close], PredicatePattern: patterns.titles.arena });
			} else {
				logger.info('doArena: normal match');
				macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, PredicatePattern: patterns.arena.matchOpponent.selected });
				if (cpThresholdIsEnabled) {
					const cpThreshold = settings.doArena.cpThreshold.Value;
					const challenge1CP = macroService.GetText(patterns.arena.challenge1.cp);
					const challenge2CP = macroService.GetText(patterns.arena.challenge2.cp);
					const challenge3CP = macroService.GetText(patterns.arena.challenge3.cp);
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
					macroService.PollPattern(patterns.arena[`challenge${targetIdx + 1}`], { DoClick: true, PredicatePattern: patterns.arena.enter });
				} else {
					logger.info('doArena: normal match challenge 3');
					macroService.PollPattern(patterns.arena.challenge3, { DoClick: true, PredicatePattern: patterns.arena.enter });
				}
				
				if (autoDetectCpThreshold) {
					const currentCp = selectTeam(teamSlot, true);
					if (currentCp) {
						logger.info(`doArena: set settings.doArena.cpThreshold to ${currentCp}`);
						settings.doArena.cpThreshold.Value = currentCp;
						settings.doArena.autoDetectCpThreshold.Value = false;
					}
				} else {
					selectTeam(teamSlot);
				}

				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: [patterns.arena.tapEmptySpace, patterns.arena.defendReport.close], PredicatePattern: patterns.titles.arena });
			}
			break;
	}
	sleep(1_000);
}
