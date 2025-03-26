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
				//macroService.PollPattern(patterns.shop.normal, { DoClick: true, PredicatePattern: patterns.shop.normal.selected });
				const normalFreeResult = macroService.SwipePollPattern(patterns.shop.free, { MaxSwipes: 5, Start: { X: swipeRightStartX, Y: 500 }, End: { X: swipeRightEndX, Y: 500 } });
				if (normalFreeResult.IsSuccess) {
					sleep(1_000);
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, InversePredicatePattern: patterns.shop.free.ok });
				}
				if (macroService.IsRunning) {
					daily.claimFreeShop.normal.IsChecked = true;
				}
			}

			//if (!weekly.claimFreeShop.done.IsChecked) {
			//	logger.info('claimFreeShop: claim Normal weekly');
			//	//macroService.PollPattern(patterns.shop.normal.weekly, { DoClick: true, PredicatePattern: patterns.shop.normal.weekly.selected });
			//	const normalFreeResult = macroService.SwipePollPattern(patterns.shop.free, { MaxSwipes: 5, Start: { X: swipeRightStartX, Y: 500 }, End: { X: swipeRightEndX, Y: 500 } });
			//	if (normalFreeResult.IsSuccess) {
			//		sleep(1_000);
			//		macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
			//		macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, InversePredicatePattern: patterns.shop.free.ok });
			//	}
			//	if (macroService.IsRunning) {
			//		weekly.claimFreeShop.done.IsChecked = true;
			//	}
			//}

			logger.info('claimFreeShop: claim Resource');
			//const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			//if (!swipeResult.IsSuccess) {
			//	throw new Error('Unable to find resource shop');
			//}
			//sleep(1_000)
			//const shopResourceResult = macroService.PollPattern(patterns.shop.resource);
			//const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: shopResourceResult.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${shopResourceResult.Point.Y}` });
			//macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			if (!daily.claimFreeShop.resource.normal.IsChecked) {
				logger.info('claimFreeShop: claim Resource Normal');
				macroService.PollPattern(patterns.shop.resources.resources, { DoClick: true, PredicatePattern: patterns.shop.resources.resources.selected });

				let resourceFreeResult = macroService.FindPattern(patterns.shop.free);
				while (resourceFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
					sleep(500);
					resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
				}
				if (macroService.IsRunning) {
					daily.claimFreeShop.resource.normal.IsChecked = true;
				}
			}

			macroService.PollPattern(patterns.shop.contents, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints });
			if (!daily.claimFreeShop.resource.skywardTower.IsChecked) {
				logger.info('claimFreeShop: claim Resource Skyward Tower');
				const skywardTowerSwipeResult = macroService.SwipePollPattern(patterns.shop.contents.skywardTower, { MaxSwipes: 5, Start: { X: 350, Y: 800 }, End: { X: 350, Y: 400 } });
				if (!skywardTowerSwipeResult.IsSuccess) {
					throw new Error('Unable to find skyward tower');
				}
				macroService.PollPattern(patterns.shop.contents.skywardTower, { DoClick: true, PredicatePattern: patterns.shop.contents.skywardTower.selected });

				let skywardTowerFreeResult = macroService.FindPattern(patterns.shop.free);
				while (skywardTowerFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
					sleep(500);
					skywardTowerFreeResult = macroService.FindPattern(patterns.shop.resource.free);
				}
				if (macroService.IsRunning) {
					daily.claimFreeShop.resource.skywardTower.IsChecked = true;
				}
			}

			if (!daily.claimFreeShop.surveyHub.IsChecked) {
				logger.info('claimFreeShop: claim Survey Hub');
				//const swipeResult2 = macroService.SwipePollPattern(patterns.shop.surveyHub, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
				//if (!swipeResult2.IsSuccess) {
				//	throw new Error('Unable to find surveyhub shop');
				//}
				//sleep(1_000);
				//const shopSurveyHubResult = macroService.PollPattern(patterns.shop.surveyHub);
				//const selectedSurveyHubPattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: shopSurveyHubResult.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${shopSurveyHubResult.Point.Y}` });
				//macroService.PollPattern(patterns.shop.surveyHub, { DoClick: true, PredicatePattern: selectedSurveyHubPattern });

				const surveyHubSwipeResult = macroService.SwipePollPattern(patterns.shop.contents.surveyHub, { MaxSwipes: 5, Start: { X: 350, Y: 800 }, End: { X: 350, Y: 400 } });
				if (!surveyHubSwipeResult.IsSuccess) {
					throw new Error('Unable to find skyward tower');
				}
				macroService.PollPattern(patterns.shop.contents.surveyHub, { DoClick: true, PredicatePattern: patterns.shop.contents.surveyHub.selected });

				const surveyhubFreeResult = macroService.FindPattern(patterns.shop.free);
				if (surveyhubFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
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