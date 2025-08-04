const loopPatterns = [patterns.lobby, patterns.menu.event.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.doRoulette.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doRoulette: click roulette');
			macroService.PollPattern(patterns.menu, { DoClick: true, PredicatePattern: patterns.menu.close });
			if (!macroService.FindPattern(patterns.menu.event.notification).IsSuccess) {
				if (macroService.IsRunning) daily.doRoulette.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.menu.event, { DoClick: true, PredicatePattern: patterns.menu.event.title });
			break;
		case 'menu.event.title':
			logger.info('doRoulette: spin roulette');
			macroService.PollPattern(patterns.menu.event.roulette.push, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
			macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.event.title });

			if (macroService.IsRunning) daily.doRoulette.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}