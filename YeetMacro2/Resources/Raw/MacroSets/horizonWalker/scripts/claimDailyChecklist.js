// claim daily rewards
const loopPatterns = [patterns.phone.battery, patterns.checklist.title];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimDailyChecklist.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('claimDailyChecklist: click checklist');
			macroService.ClickPattern(patterns.phone.checklist);
			break;
		case 'checklist.title':
			logger.info('claimDailyChecklist: redeem all');
			sleep(1_000);
			macroService.PollPattern(patterns.checklist.redeemAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			sleep(1_000);
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.checklist.title });

			macroService.IsRunning && (daily.claimDailyChecklist.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}