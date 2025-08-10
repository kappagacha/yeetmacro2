// claim inbox
const loopPatterns = [patterns.phone.battery, patterns.phone.inbox.title];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimInbox.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('claimInbox: click inbox');
			macroService.ClickPattern(patterns.phone.inbox);
			break;
		case 'phone.inbox.title':
			logger.info('claimInbox: redeem all');
			//sleep(1_000);
			//macroService.PollPattern(patterns.checklist.redeemAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			//sleep(1_000);
			//macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.checklist.title });

			macroService.IsRunning && (daily.claimInbox.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}