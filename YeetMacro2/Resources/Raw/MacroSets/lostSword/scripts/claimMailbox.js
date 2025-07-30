const loopPatterns = [patterns.lobby, patterns.lobby.menu.mailbox.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimMailbox.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('claimMailbox: click mailbox');
			macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.lobby.menu.close });
			if (!macroService.FindPattern(patterns.lobby.menu.mailbox.notification).IsSuccess) {
				if (macroService.IsRunning) daily.claimMailbox.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.lobby.menu.mailbox, { DoClick: true, PredicatePattern: patterns.lobby.menu.mailbox.title });
			break;
		case 'lobby.menu.mailbox.title':
			logger.info('claimMailbox: claim mailbox');
			macroService.PollPattern(patterns.lobby.menu.mailbox.claimAll, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
			macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.lobby.menu.mailbox.claimAll });


			if (macroService.IsRunning) daily.claimMailbox.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}