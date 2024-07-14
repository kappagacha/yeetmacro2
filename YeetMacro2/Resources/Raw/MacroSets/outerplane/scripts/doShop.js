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
			sleep(2_000);
			const shopResourceResult = macroService.FindPattern(patterns.shop.resource);
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: shopResourceResult.Point.Y, Padding: 20 });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			const friendshipItems = ['stamina', 'gold', 'clearTicket', 'arenaTicket', 'hammer', 'stoneFragment', 'stonePiece'];
			macroService.PollPattern(patterns.shop.resource.friendship, { DoClick: true, PredicatePattern: patterns.shop.resource.friendship.currency });
			sleep(1000);
			doShopItems('doShop', 'friendshipPoint', friendshipItems);

			const arenaItems = ['gold', 'stamina', 'cakeSlice'];
			macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });
			sleep(1000);
			doShopItems('doShop', 'arena', arenaItems);

			if (macroService.IsRunning) {
				daily.doShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}