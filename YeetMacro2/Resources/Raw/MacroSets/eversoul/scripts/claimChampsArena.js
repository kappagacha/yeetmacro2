// Claim champs arena rewards
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure];
const daily = dailyManager.GetDaily();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimChampsArena: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('claimChampsArena: click arena');
			macroService.PollPattern(patterns.adventure.tabs.arena, { DoClick: true, PredicatePattern: patterns.adventure.arena });
			sleep(500);

			logger.info('claimChampsArena: claim reward');
			macroService.PollPattern(patterns.adventure.champsArena.claimReward, { DoClick: true, PredicatePattern: patterns.adventure.champsArena.tapTheScreen });
			macroService.PollPattern(patterns.adventure.champsArena.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.adventure });

			logger.info('claimChampsArena: done');
			if (macroService.IsRunning) {
				daily.claimChampsArena.count.Count++;
			}
			return;
	}

	sleep(1_000);
}