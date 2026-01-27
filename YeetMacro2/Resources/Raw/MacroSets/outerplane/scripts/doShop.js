// @position=2
// Claim free shop items
const loopPatterns = [patterns.lobby.level, patterns.titles.adventurerShop];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
if (daily.doShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			if (!daily.doShop.free.premiumShop.normal.daily.IsChecked) {
				macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.premium.move });
				macroService.PollPattern(patterns.shop.premium.move, { DoClick: true, PredicatePattern: patterns.shop.premium.title });
				macroService.PollPattern(patterns.shop.premium.normal, { DoClick: true, PredicatePattern: patterns.shop.premium.normal.selected });

				let normalFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				while (normalFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.shop.free.ok, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.shop.free.ok, { DoClick: true, InversePredicatePattern: patterns.shop.free.ok, TimeoutMs: 3_500 });
					normalFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				}

				daily.doShop.free.premiumShop.normal.daily.IsChecked = true;
				goToLobby();
				break;
			}

			if (!daily.doShop.free.adventurerShop.goldOrConsumables.IsChecked) {
				macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.adventurer.move });
				macroService.PollPattern(patterns.shop.adventurer.move, { DoClick: true, PredicatePattern: patterns.titles.adventurerShop });

				const goldOrConsumablesResult = macroService.PollPattern(patterns.shop.adventurer.goldOrConsumables, { SwipePattern: patterns.shop.mainTabSwipeDown, TimeoutMs: 7_000 });
				if (!goldOrConsumablesResult.IsSuccess) {
					throw new Error('Unable to find skyward Gold/Consumables');
				}
				macroService.PollPattern(patterns.shop.adventurer.goldOrConsumables, { DoClick: true, PredicatePattern: patterns.shop.adventurer.goldOrConsumables.selected });

				let goldOrConsumablesFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				while (goldOrConsumablesFreeResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.free, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, InversePredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
					goldOrConsumablesFreeResult = macroService.PollPattern(patterns.shop.free, { TimeoutMs: 3_500 });
				}

				daily.doShop.free.adventurerShop.goldOrConsumables.IsChecked = true;
				goToLobby();
				break;
			}

			logger.info('doShop: go to adventurerShop');
			macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.adventurer.move });
			macroService.PollPattern(patterns.shop.adventurer.move, { DoClick: true, PredicatePattern: patterns.titles.adventurerShop });
			sleep(500);
			break;
		case 'titles.adventurerShop':			
			if (settings.doShop.friendshipPoint.IsEnabled && !daily.doShop.friendshipPoint.done.IsChecked) {
				logger.info('doShop: friendship point');
				const friendshipItems = ['stamina', 'gold', 'arenaTicket', 'hammer', 'stoneFragment', 'stonePiece'];
				macroService.PollPattern(patterns.shop.adventurer.friendshipPoint, { DoClick: true, PredicatePattern: patterns.shop.adventurer.friendshipPoint.selected });
				sleep(1_000);
				doShopItems('doShop', 'friendshipPoint', friendshipItems);

				if (macroService.IsRunning) {
					daily.doShop.friendshipPoint.done.IsChecked = true;
				}
			}
			
			if (settings.doShop.arena.IsEnabled && !daily.doShop.arena.done.IsChecked) {
				logger.info('doShop: arena');
				const arenaItems = ['gold', 'stamina'];
				macroService.PollPattern(patterns.shop.adventurer.arena, { DoClick: true, PredicatePattern: patterns.shop.adventurer.arena.selected });
				macroService.PollPattern(patterns.shop.adventurer.arena.arena, { DoClick: true, PredicatePattern: patterns.shop.adventurer.arena.arena.selected });
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
				logger.info('doShop: jointChallenge');
				const jointChallengeItems = ['stamina'];
				macroService.PollPattern(patterns.shop.adventurer.event, { DoClick: true, PredicatePattern: patterns.shop.adventurer.event.selected });
				const jointChallengeSwipeResult = macroService.PollPattern(patterns.shop.adventurer.event.jointChallenge, { SwipePattern: patterns.shop.subsubTabSwipeRight, TimeoutMs: 4_000 });
				if (!jointChallengeSwipeResult.IsSuccess) {
					throw new Error('Unable to find joint challenge');
				}
				macroService.PollPattern(patterns.shop.adventurer.event.jointChallenge, { DoClick: true, PredicatePattern: patterns.shop.adventurer.event.jointChallenge.selected });
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