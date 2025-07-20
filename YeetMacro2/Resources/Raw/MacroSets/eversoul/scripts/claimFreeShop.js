// @position=3
// Claim free shop item
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetCurrentDaily();
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
			//const regularPackSwipeResult = macroService.SwipePollPattern(patterns.cashShop.regularPack, { Start: { X: 100, Y: 650 }, End: { X: 100, Y: 200 } });
			const regularPackSwipeResult = macroService.PollPattern(patterns.cashShop.regularPack, { SwipePattern: patterns.cashShop.leftPanelSwipe, TimeoutMs: 10_000 });

			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find normal summon');
			}

			if (!regularPackSwipeResult.IsSuccess) {
				throw new Error('Unable to find regular pack');
			}
			sleep(1_000);
			//const regularPackResult = macroService.FindPattern(patterns.cashShop.regularPack);
			//const regularPackSelected = macroService.ClonePattern(patterns.cashShop.regularPack.selected, { CenterY: regularPackResult.Point.Y, Padding: 10 })
			//macroService.PollPattern(patterns.cashShop.regularPack, { DoClick: true, PredicatePattern: regularPackSelected });
			macroService.PollPattern(patterns.cashShop.regularPack, { DoClick: true, PredicatePattern: patterns.cashShop.regularPack.dailyFree });
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