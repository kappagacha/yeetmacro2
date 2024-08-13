// @position=2
// Claim free shop items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
if (daily.doShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doShop: click shop tab');
			macroService.ClickPattern(patterns.tabs.shop);
			sleep(500);
			break;
		case 'titles.shop':
			logger.info('doShop: claim Resource');
			const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find resource shop');
			}
			sleep(1_000);
			const shopResourceResult = macroService.PollPattern(patterns.shop.resource);
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: shopResourceResult.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${shopResourceResult.Point.Y}` });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			if (!daily.doShop.friendshipPoint.done.IsChecked) {
				const friendshipItems = ['stamina', 'gold', 'clearTicket', 'arenaTicket', 'hammer', 'stoneFragment', 'stonePiece'];
				macroService.PollPattern(patterns.shop.resource.friendship, { DoClick: true, PredicatePattern: patterns.shop.resource.friendship.currency });
				sleep(1_000);
				doShopItems('doShop', 'friendshipPoint', friendshipItems);

				if (macroService.IsRunning) {
					daily.doShop.friendshipPoint.done.IsChecked = true;
				}
			}
			
			if (!daily.doShop.arena.done.IsChecked) {
				const arenaItems = ['gold', 'stamina', 'cakeSlice'];
				macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });
				sleep(1_000);
				doShopItems('doShop', 'arena', arenaItems);
				
				if (macroService.IsRunning) {
					daily.doShop.arena.done.IsChecked = true;
				}
			}

			if (!daily.doShop.festival.done.IsChecked) {
				const festivalItems = ['stamina', 'gold'];
				const selectedEventPattern = macroService.PollPattern(patterns.shop.event);
				const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: selectedEventPattern.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${selectedEventPattern.Point.Y}` });
				macroService.PollPattern(patterns.shop.event, { DoClick: true, PredicatePattern: selectedResourcePattern });
				sleep(1_000);

				const festivalSwipeResult = macroService.SwipePollPattern(patterns.shop.event.festival, { MaxSwipes: 5, Start: { X: 1100, Y: 160 }, End: { X: 700, Y: 160 } });
				if (!festivalSwipeResult.IsSuccess) {
					throw new Error('Unable to find festival');
				}
				macroService.PollPattern(patterns.shop.event.festival, { DoClick: true, PredicatePattern: patterns.shop.event.festival.currency });

				doShopItems('doShop', 'festival', festivalItems);

				if (macroService.IsRunning) {
					daily.doShop.festival.done.IsChecked = true;
				}
			}

			if (!daily.doShop.jointChallenge.done.IsChecked) {
				const jointChallengeItems = ['stamina'];
				const selectedEventPattern = macroService.PollPattern(patterns.shop.event);
				const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: selectedEventPattern.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${selectedEventPattern.Point.Y}` });
				macroService.PollPattern(patterns.shop.event, { DoClick: true, PredicatePattern: selectedResourcePattern });
				sleep(1_000);

				const jointChallengeSwipeResult = macroService.SwipePollPattern(patterns.shop.event.jointChallenge, { MaxSwipes: 5, Start: { X: 1100, Y: 160 }, End: { X: 700, Y: 160 } });
				if (!jointChallengeSwipeResult.IsSuccess) {
					throw new Error('Unable to find joint challenge');
				}
				macroService.PollPattern(patterns.shop.event.jointChallenge, { DoClick: true, PredicatePattern: patterns.shop.event.jointChallenge.currency });

				doShopItems('doShop', 'jointChallenge', jointChallengeItems);

				if (macroService.IsRunning) {
					daily.doShop.jointChallenge.done.IsChecked = true;
				}
			}

			if (macroService.IsRunning) {
				daily.doShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}