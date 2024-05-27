// @position=2
// Claim free shop items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const weekly = weeklyManager.GetCurrentWeekly();

if (daily.claimFreeShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimFreeShop: click shop tab');
			const shopNotificationResult = macroService.PollPattern(patterns.tabs.shop.notification, { TimeoutMs: 2_000 });
			if (shopNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.shop);
			} else {	// already claimed
				return;
			}
			sleep(500);
			break;
		case 'titles.shop':
			logger.info('claimFreeShop: claim Normal');
			macroService.PollPattern(patterns.shop.normal, { DoClick: true, PredicatePattern: patterns.shop.normal.selected });
			const normalFreeResult = macroService.PollPattern(patterns.shop.normal.free, { TimeoutMs: 2_000 });
			if (normalFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.normal.free, { DoClick: true, PredicatePattern: patterns.shop.normal.free.confirm });
				macroService.PollPattern(patterns.shop.normal.free.confirm, { DoClick: true, InversePredicatePattern: patterns.shop.normal.free.confirm });
			}

			if (!weekly.claimFreeShop.done.IsChecked) {
				macroService.PollPattern(patterns.shop.normal.weekly, { DoClick: true, PredicatePattern: patterns.shop.normal.weekly.selected });
				const normalFreeResult = macroService.PollPattern(patterns.shop.normal.free, { TimeoutMs: 2_000 });
				if (normalFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.normal.free, { DoClick: true, PredicatePattern: patterns.shop.normal.free.confirm });
					macroService.PollPattern(patterns.shop.normal.free.confirm, { DoClick: true, InversePredicatePattern: patterns.shop.normal.free.confirm });
				}
			}

			logger.info('claimFreeShop: claim Resource');
			const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find resource shop');
			}
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: swipeResult.Point.Y, Padding: 20 });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			let resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			while (resourceFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.free, { DoClick: true, PredicatePattern: patterns.shop.resource.free.confirm });
				macroService.PollPattern(patterns.shop.resource.free.confirm, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
				sleep(500);
				resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			}

			const swipeResult2 = macroService.SwipePollPattern(patterns.shop.surveyHub, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult2.IsSuccess) {
				throw new Error('Unable to find surveyhub shop');
			}

			const selectedSurveyHubPattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: swipeResult2.Point.Y, Padding: 20 });
			macroService.PollPattern(patterns.shop.surveyHub, { DoClick: true, PredicatePattern: selectedSurveyHubPattern });

			const surveyhubFreeResult = macroService.FindPattern(patterns.shop.free);
			if (surveyhubFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
				macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			}

			if (macroService.IsRunning) {
				daily.claimFreeShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}