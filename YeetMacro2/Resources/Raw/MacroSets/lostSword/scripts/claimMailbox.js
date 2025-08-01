const loopPatterns = [patterns.lobby, patterns.menu.mailbox.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimMailbox.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('claimMailbox: click mailbox');
			macroService.PollPattern(patterns.menu, { DoClick: true, PredicatePattern: patterns.menu.close });
			if (!macroService.FindPattern(patterns.menu.mailbox.notification).IsSuccess) {
				if (macroService.IsRunning) daily.claimMailbox.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.menu.mailbox, { DoClick: true, PredicatePattern: patterns.menu.mailbox.title });
			break;
		case 'menu.mailbox.title':
			logger.info('claimMailbox: claim mailbox');
			macroService.PollPattern(patterns.menu.mailbox.claimAll, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
			macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.mailbox.claimAll });


			if (macroService.IsRunning) daily.claimMailbox.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}