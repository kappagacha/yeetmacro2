// Claim daily free
const loopPatterns = [patterns.lobby.message, patterns.titles.shop];
const daily = dailyManager.GetDaily();
if (daily.claimFreeShop.done) {
	return;
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.message':
			logger.info('claimFreeShop: click shop tab');
			const shopNotificationResult = macroService.PollPattern(patterns.tabs.shop.notification, { TimoutMs: 1_000 });
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
			const normalFreeResult = macroService.PollPattern(patterns.shop.normal.free, { TimoutMs: 1_000 });
			if (normalFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.normal.free, { DoClick: true, PredicatePattern: patterns.shop.normal.free.confirm });
				macroService.PollPattern(patterns.shop.normal.free.confirm, { DoClick: true, InversePredicatePattern: patterns.shop.normal.free.confirm });
			}
			logger.info('claimFreeShop: claim Resource');
			const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 1, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find resource shop');
			}
			let resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			while (resourceFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.free, { DoClick: true, PredicatePattern: patterns.shop.resource.free.confirm });
				macroService.PollPattern(patterns.shop.resource.free.confirm, { DoClick: true, InversePredicatePattern: patterns.shop.resource.free.confirm });
				resourceFreeResult = macroService.FindPattern(patterns.shop.resource.free);
			}
			macroService.PollPattern(patterns.shop.resource.free, { DoClick: true, ClickPattern: [patterns.shop.resource.free.confirm, patterns.shop.purchaseComplete], InversePredicatePattern: patterns.shop.resource.free });
			daily.claimFreeShop.done = true;
			dailyManager.UpdateDaily(daily);
			return;
	}
	sleep(1_000);
}