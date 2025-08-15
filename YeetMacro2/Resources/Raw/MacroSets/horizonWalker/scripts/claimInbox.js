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
			const notificationResult = macroService.PollPattern(patterns.phone.inbox.notification, { TimeoutMs: 3_000 });
			if (notificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.phone.inbox);
			} else {	// nothing new in inbox
				return;
			}
			break;
		case 'phone.inbox.title':
			logger.info('claimInbox: claim all');
			sleep(1_000);
			macroService.PollPattern(patterns.phone.inbox.claimAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.phone.inbox.title });

			macroService.IsRunning && (daily.claimInbox.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}