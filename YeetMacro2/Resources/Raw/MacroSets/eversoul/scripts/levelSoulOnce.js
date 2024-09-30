// Level up a soul once
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetCurrentDaily();
if (daily.levelSoulOnce.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('levelSoulOnce: click souls');
			macroService.ClickPattern(patterns.lobby.souls);
			break;
		case 'general.back':
			logger.info('levelSoulOnce: level a soul');
			macroService.PollPattern(patterns.souls.star);
			const starResult = macroService.FindPattern(patterns.souls.star, { Limit: 5 });
			const maxXPoint = starResult.Points.reduce((maxXPoint, p) => (maxXPoint = maxXPoint.X >= p.X ? maxXPoint : p));
			macroService.PollPoint(maxXPoint, { PredicatePattern: patterns.souls.quickLevelUp });
			macroService.PollPattern(patterns.souls.quickLevelUp, { DoClick: true, PredicatePattern: patterns.souls.quickLevelUp.confirm });
			macroService.PollPattern(patterns.souls.quickLevelUp.confirm, { DoClick: true, PredicatePattern: patterns.souls.quickLevelUp.tapTheScreen });
			macroService.PollPattern(patterns.souls.quickLevelUp.tapTheScreen, { DoClick: true, PredicatePattern: patterns.general.back });

			logger.info('levelSoulOnce: done');
			if (macroService.IsRunning) {
				daily.levelSoulOnce.done.IsChecked = true;
			}

			return;
	}

	sleep(1_000);
}