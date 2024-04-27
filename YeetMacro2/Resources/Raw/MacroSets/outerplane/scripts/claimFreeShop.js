// @position=2
// Claim free shop items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
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
			logger.info('claimFreeShop: claim Resource');
			const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find resource shop');
			}
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.resource.selected, { CenterY: swipeResult.Point.Y, Padding: 10 });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			let resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			while (resourceFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.free, { DoClick: true, PredicatePattern: patterns.shop.resource.free.confirm });
				macroService.PollPattern(patterns.shop.resource.free.confirm, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
				sleep(500);
				resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			}

			const friendshipItems = ['stamina', 'gold', 'clearTicket', 'arenaTicket', 'hammer', 'stoneFragment', 'stonePiece'];
			for (const friendshipItem of friendshipItems) {
				if (settings.claimFreeShop.useFriendshipPoints[friendshipItem].Value && !daily.claimFreeShop.useFriendshipPoints[friendshipItem].IsChecked) {
					logger.info(`claimFreeShop: purchase friendshipItem ${friendshipItem}`);
					macroService.PollPattern(patterns.shop.resource.friendship, { DoClick: true, PredicatePattern: patterns.shop.resource.friendship.currency });

					const friendshipItemPattern = macroService.ClonePattern(patterns.shop.resource.friendship[friendshipItem], {
						X: 350,
						Y: 230,
						Width: resolution.Width - 550,
						Height: 800,
						Path: `patterns.shop.resource.friendship.${friendshipItem}`,
						OffsetCalcType: 'DockLeft'
					});
					const friendshipItemResult = macroService.PollPattern(friendshipItemPattern);
					const friendshipItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
						X: friendshipItemResult.Point.X - 70,
						Y: friendshipItemResult.Point.Y + 200,
						Width: 350,
						Height: 100,
						Path: `patterns.shop.resource.friendship.${friendshipItem}.purchase`,
						OffsetCalcType: 'None'
					});

					macroService.PollPattern(friendshipItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
					const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
					if (maxSliderResult.IsSuccess) {
						macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
					}
					macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
					if (macroService.IsRunning) {
						daily.claimFreeShop.useFriendshipPoints[friendshipItem].IsChecked = true;
					}
				}
			}

			const arenaItems = ['gold', 'stamina', 'cakeSlice'];
			for (const arenaItem of arenaItems) {
				if (settings.claimFreeShop.useArenaMedals[arenaItem].Value && !daily.claimFreeShop.useArenaMedals[arenaItem].IsChecked) {
					logger.info(`claimFreeShop: purchase arenaItem ${arenaItem}`);
					macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });

					const arenaItemPattern = macroService.ClonePattern(patterns.shop.resource.arena[arenaItem], {
						X: 350,
						Y: 230,
						Width: resolution.Width - 550,
						Height: 800,
						Path: `patterns.shop.resource.arena.${arenaItem}`,
						OffsetCalcType: 'DockLeft'
					});
					const arenaItemResult = macroService.PollPattern(arenaItemPattern);
					const arenaItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
						X: arenaItemResult.Point.X - 70,
						Y: arenaItemResult.Point.Y + 200,
						Width: 350,
						Height: 100,
						Path: `patterns.shop.resource.arena.${arenaItem}.purchase`,
						OffsetCalcType: 'None'
					});

					macroService.PollPattern(arenaItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
					const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
					if (maxSliderResult.IsSuccess) {
						macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
					}
					macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
					if (macroService.IsRunning) {
						daily.claimFreeShop.useArenaMedals[arenaItem].IsChecked = true;
					}
				}
			}
			
			if (macroService.IsRunning) {
				daily.claimFreeShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}