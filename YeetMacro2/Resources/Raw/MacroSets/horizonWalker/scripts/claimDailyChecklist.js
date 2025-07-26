// claim daily rewards
const loopPatterns = [patterns.event.title, patterns.checklist.title];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.claimDailyChecklist.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'event.title':
			logger.info('claimDailyChecklist: click checklist');
			macroService.ClickPattern(patterns.phone.checklist);
			break;
		case 'checklist.title':
			logger.info('claimDailyChecklist: redeem all');
			macroService.PollPattern(patterns.checklist.redeemAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.checklist.title });

			macroService.IsRunning && (daily.claimDailyChecklist.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}