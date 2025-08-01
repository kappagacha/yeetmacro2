const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.colosseum.title];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.doColosseum.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

//if (isLastRunWithinHour && !settings.doColosseum.forceRun.Value) {
//	return 'Last run was within the hour. Use forceRun setting to override check';
//}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doColosseum: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doColosseum: click colosseum');
			macroService.ClickPattern(patterns.battle.colosseum);
			break;
		case 'colosseum.title':
			macroService.PollPattern(patterns.colosseum.challenge);

			if (macroService.FindPattern(patterns.colosseum.zeroTickets).IsSuccess) {
				return;
			}

			const challengeResult = macroService.FindPattern(patterns.colosseum.challenge, { Limit: 3 });
			const lowestChallengePoint = challengeResult.Points.reduce((pMaxY, p) => pMaxY.Y < p.Y ? p : pMaxY);
			macroService.PollPoint(lowestChallengePoint, { PredicatePattern: patterns.battle.enter });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.colosseum.title });

			if (macroService.IsRunning) {
				daily.doColosseum.count.Count++;
				settings.doColosseum.lastRun.Value = new Date().toISOString();
			}
			break;

	}
	sleep(1_000);
}