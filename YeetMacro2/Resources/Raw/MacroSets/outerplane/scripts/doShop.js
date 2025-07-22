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
			macroService.PollPattern(patterns.shop.contents, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints });

			if (settings.doShop.friendshipPoint.IsEnabled && !daily.doShop.friendshipPoint.done.IsChecked) {
				const friendshipItems = ['stamina', 'gold', 'arenaTicket', 'hammer', 'stoneFragment', 'stonePiece'];
				macroService.PollPattern(patterns.shop.contents.friendshipPoints, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints.selected });
				sleep(1_000);
				doShopItems('doShop', 'friendshipPoint', friendshipItems);

				if (macroService.IsRunning) {
					daily.doShop.friendshipPoint.done.IsChecked = true;
				}
			}
			
			if (settings.doShop.arena.IsEnabled && !daily.doShop.arena.done.IsChecked) {
				const arenaItems = ['gold', 'stamina'];
				macroService.PollPattern(patterns.shop.contents.arena, { DoClick: true, PredicatePattern: patterns.shop.contents.arena.selected });
				sleep(1_000);
				doShopItems('doShop', 'arena', arenaItems);
				
				if (macroService.IsRunning) {
					daily.doShop.arena.done.IsChecked = true;
				}
			}

			//if (settings.doShop.festival.IsEnabled && !daily.doShop.festival.done.IsChecked) {
			//	const festivalItems = ['stamina', 'gold'];
			//	const selectedEventPattern = macroService.PollPattern(patterns.shop.event);
			//	const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: selectedEventPattern.Point.Y, Padding: 20, Path: `patterns.shop.selected_Y${selectedEventPattern.Point.Y}` });
			//	macroService.PollPattern(patterns.shop.event, { DoClick: true, PredicatePattern: selectedResourcePattern });
			//	sleep(1_000);

			//	const festivalPattern = macroService.ClonePattern(patterns.shop.event.festival, { X: 400, Y: 120, Width: resolution.Width - 420, Height: 90, OffsetCalcType: 'None' });
			//	const festivalSwipeResult = macroService.PollPattern(festivalPattern, { SwipePattern: ???, TimeoutMs: 7_000 });
			//	if (!festivalSwipeResult.IsSuccess) {
			//		throw new Error('Unable to find festival');
			//	}
			//	macroService.PollPattern(festivalPattern, { DoClick: true, PredicatePattern: patterns.shop.event.festival.currency });

			//	doShopItems('doShop', 'festival', festivalItems);

			//	if (macroService.IsRunning) {
			//		daily.doShop.festival.done.IsChecked = true;
			//	}
			//}

			if (settings.doShop.jointChallenge.IsEnabled && !daily.doShop.jointChallenge.done.IsChecked) {
				const jointChallengeItems = ['stamina'];
				macroService.PollPattern(patterns.shop.contents.event, { DoClick: true, PredicatePattern: patterns.shop.contents.event.selected });
				macroService.PollPattern(patterns.shop.contents.event.jointChallenge, { DoClick: true, PredicatePattern: patterns.shop.contents.event.jointChallenge.selected });
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