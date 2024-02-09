// @position=3
// Claim free shop item
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetDaily();
if (daily.claimFreeShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimFreeShop: click cash shop');
			const notificationResult = macroService.PollPattern(patterns.lobby.cashShop.notification, { TimeoutMs: 1_500 });
			if (notificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.lobby.cashShop);
			} else {	// already claimed
				return;
			}
			break;
		case 'general.back':
			logger.info('claimFreeShop: regular pack');
			const regularPackResult = macroService.PollPattern(patterns.cashShop.regularPack);
			const regularPackSelected = macroService.ClonePattern(patterns.cashShop.regularPack.selected, { CenterY: regularPackResult.Point.Y })
			macroService.PollPattern(patterns.cashShop.regularPack, { DoClick: true, PredicatePattern: regularPackSelected });
			macroService.PollPattern(patterns.cashShop.regularPack.dailyFree, { DoClick: true, PredicatePattern: patterns.prompt.tapTheScreen });
			macroService.PollPattern(patterns.prompt.tapTheScreen, { DoClick: true, PredicatePattern: patterns.general.back });

			logger.info('claimFreeShop: done');
			if (macroService.IsRunning) {
				daily.claimFreeShop.done.IsChecked = true;
			}

			return;
	}

	sleep(1_000);
}