// @position=6
// Battle in arena until out of arena tickets
// Will prioritize Memorial Match
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.arena];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doArena.teamSlot.Value;
const cpThresholdIsEnabled = settings.doArena.cpThreshold.IsEnabled;
const cpThreshold = settings.doArena.cpThreshold.Value;
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
			const leagueOfChallengeResult = macroService.FindPattern([patterns.arena.leagueOfChallenge, patterns.arena.leagueOfChallenge2, patterns.arena.leagueOfChallenge3]);
			if (leagueOfChallengeResult.IsSuccess) {
				macroService.ClickPattern([patterns.arena.leagueOfChallenge, patterns.arena.leagueOfChallenge2, patterns.arena.leagueOfChallenge3]);
				break;
			}
			
			const numTicketsZeroResult = macroService.PollPattern(patterns.arena.numTicketsZero, { TimeoutMs: 1_500 })
			if (numTicketsZeroResult.IsSuccess) {
				return;
			}

			//const memorialMatchNotificationResult = macroService.PollPattern(patterns.arena.memorialMatch.notification, { TimeoutMs: 1_500 });
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
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			} else {
				logger.info('doArena: normal match');
				macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, PredicatePattern: patterns.arena.matchOpponent.selected });
				if (cpThresholdIsEnabled) {
					const challenge1CP = macroService.GetText(patterns.arena.challenge1.cp);
					const challenge2CP = macroService.GetText(patterns.arena.challenge2.cp);
					const challenge3CP = macroService.GetText(patterns.arena.challenge3.cp);
					const challenges = [
						Number(challenge1CP.slice(0, -4).slice(1) + challenge1CP.slice(-3)),
						Number(challenge2CP.slice(0, -4).slice(1) + challenge2CP.slice(-3)),
						Number(challenge3CP.slice(0, -4).slice(1) + challenge3CP.slice(-3)),
					];

					const minIdx = challenges.reduce((minIdx, val, idx, arr) => arr[minIdx] && arr[minIdx] <= cpThreshold && val < arr[minIdx] ? idx : minIdx, 0);
					logger.info(`doArena: normal match challenge ${minIdx + 1}`);
					macroService.PollPattern(patterns.arena[`challenge${minIdx + 1}`], { DoClick: true, PredicatePattern: patterns.arena.enter });
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
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: patterns.arena.tapEmptySpace, PredicatePattern: patterns.titles.arena });
			}
			break;
	}
	sleep(1_000);
}
