// @position=2
// Claim free shop items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const weekly = weeklyManager.GetCurrentWeekly();
const swipeRightStartX = resolution.Width - 500;
const swipeRightEndX = swipeRightStartX - 600;

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
			macroService.PollPattern(patterns.shop.resources, { DoClick: true, PredicatePattern: patterns.shop.resources.normal });

			if (!daily.claimFreeShop.normal.IsChecked) {
				logger.info('claimFreeShop: claim Normal');
				let normalFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				while (normalFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, InversePredicatePattern: patterns.shop.free.ok, TimeoutMs: 3_500 });
					normalFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				}

				if (macroService.IsRunning) {
					daily.claimFreeShop.normal.IsChecked = true;
				}
			}

			if (!daily.claimFreeShop.resource.normal.IsChecked) {
				logger.info('claimFreeShop: claim Resource Normal');
				macroService.PollPattern(patterns.shop.resources.resources, { DoClick: true, PredicatePattern: patterns.shop.resources.resources.selected });

				//let resourceFreeResult = macroService.FindPattern(patterns.shop.free);
				//while (resourceFreeResult.IsSuccess) {
				//	macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
				//	macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				//	macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
				//	sleep(500);
				//	resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
				//}

				let resourceFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				while (resourceFreeResult.IsSuccess) {
					//macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					//macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					//macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });


					macroService.PollPattern(patterns.shop.free, { DoClick: true, ClickPattern: patterns.shop.free.ok, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop, TimeoutMs: 3_500 });
					resourceFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				}

				if (macroService.IsRunning) {
					daily.claimFreeShop.resource.normal.IsChecked = true;
				}
			}

			macroService.PollPattern(patterns.shop.contents, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints });
			if (!daily.claimFreeShop.resource.skywardTower.IsChecked) {
				logger.info('claimFreeShop: claim Resource Skyward Tower');
				//const skywardTowerSwipeResult = macroService.SwipePollPattern(patterns.shop.contents.skywardTower, { MaxSwipes: 5, Start: { X: 350, Y: 800 }, End: { X: 350, Y: 400 } });
				const skywardTowerSwipeResult = macroService.PollPattern(patterns.shop.contents.skywardTower, { SwipePattern: patterns.shop.subTabSwipeDown, TimeoutMs: 7_000 });
				if (!skywardTowerSwipeResult.IsSuccess) {
					throw new Error('Unable to find skyward tower');
				}
				macroService.PollPattern(patterns.shop.contents.skywardTower, { DoClick: true, PredicatePattern: patterns.shop.contents.skywardTower.selected });

				let skywardTowerFreeResult = macroService.FindPattern(patterns.shop.free);
				while (skywardTowerFreeResult.IsSuccess) {
					//macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					//macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					//macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });

					macroService.PollPattern(patterns.shop.free, { DoClick: true, ClickPattern: patterns.shop.free.ok, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop, TimeoutMs: 3_500 });
					sleep(500);
					skywardTowerFreeResult = macroService.FindPattern(patterns.shop.resource.free);
				}
				if (macroService.IsRunning) {
					daily.claimFreeShop.resource.skywardTower.IsChecked = true;
				}
			}

			if (!daily.claimFreeShop.surveyHub.IsChecked) {
				logger.info('claimFreeShop: claim Survey Hub');
				//const surveyHubSwipeResult = macroService.SwipePollPattern(patterns.shop.contents.surveyHub, { MaxSwipes: 5, Start: { X: 350, Y: 800 }, End: { X: 350, Y: 400 } });
				const surveyHubSwipeResult = macroService.PollPattern(patterns.shop.contents.surveyHub, { SwipePattern: patterns.shop.subTabSwipeDown, TimeoutMs: 7_000 });
				if (!surveyHubSwipeResult.IsSuccess) {
					throw new Error('Unable to find skyward tower');
				}
				macroService.PollPattern(patterns.shop.contents.surveyHub, { DoClick: true, PredicatePattern: patterns.shop.contents.surveyHub.selected });

				const surveyhubFreeResult = macroService.FindPattern(patterns.shop.free);
				if (surveyhubFreeResult.IsSuccess) {
					//macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					//macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					//macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });

					macroService.PollPattern(patterns.shop.free, { DoClick: true, ClickPattern: patterns.shop.free.ok, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop, TimeoutMs: 3_500 });
				}
				if (macroService.IsRunning) {
					daily.claimFreeShop.surveyHub.IsChecked = true;
				}
			}
			
			if (macroService.IsRunning) {
				daily.claimFreeShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}