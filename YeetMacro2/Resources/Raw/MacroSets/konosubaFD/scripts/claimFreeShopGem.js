// @position=1
// Claim free gem from shop
const loopPatterns = [patterns.titles.home, patterns.shop.purchaseItems];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimFreeShopGem.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('claimFreeShopGem: click shop');
			macroService.ClickPattern(patterns.shop);
			break;
		case 'shop.purchaseItems':
			logger.info('claimFreeShopGem: claim free gems');
			macroService.PollPattern(patterns.shop.accept, { DoClick: true, PredicatePattern: patterns.shop.accept.ok });
			macroService.PollPattern(patterns.shop.accept.ok, { DoClick: true, PredicatePattern: patterns.shop.accept.ok2 });
			macroService.PollPattern(patterns.shop.accept.ok2, { DoClick: true, PredicatePattern: patterns.shop.purchaseItems });

			if (macroService.IsRunning) {
				daily.claimFreeShopGem.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}