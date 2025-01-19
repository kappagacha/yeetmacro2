// claim daily rewards
const loopPatterns = [patterns.lobby.stage, patterns.schedule];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.claimDailyRewards.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('claimDailyRewards: click schedule');
			macroService.ClickPattern(patterns.lobby.schedule);
			break;
		case 'schedule':
			logger.info('claimDailyRewards: redeem all');
			macroService.PollPattern(patterns.schedule.redeemAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.schedule });

			macroService.IsRunning && (daily.claimDailyRewards.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}