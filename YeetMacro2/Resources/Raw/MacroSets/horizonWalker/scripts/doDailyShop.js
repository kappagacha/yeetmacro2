// do daily shop
const loopPatterns = [patterns.phone.battery, patterns.mall];
const daily = dailyManager.GetCurrentDaily();

if (daily.doDailyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('doDailyShop: click store');
			macroService.ClickPattern(patterns.phone.store, { ClickOffset: { Y: 200 } });
			break;
		case 'mall':
			logger.info('doDailyShop: purchase brilliant gift box');
			macroService.PollPattern(patterns.mall.dailyLimited, { DoClick: true, PredicatePattern: patterns.mall.dailyLimited.selected });
			macroService.PollPattern([patterns.mall.dailyLimited.giftBox, patterns.mall.dailyLimited.brilliantGiftBox, patterns.mall.dailyLimited.luxuriousGiftBox], { DoClick: true, PredicatePattern: patterns.mall.purchase });
			const sliderEndResult = macroService.FindPattern(patterns.mall.sliderEnd);
			if (sliderEndResult.IsSuccess) {
				macroService.PollPattern(patterns.mall.sliderEnd, { DoClick: true, InversePredicatePattern: patterns.mall.sliderEnd });
			}
			macroService.PollPattern(patterns.mall.purchase, { DoClick: true, PredicatePattern: patterns.mall.purchase.confirm });
			macroService.PollPattern(patterns.mall.purchase.confirm, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.mall });

			logger.info('doDailyShop: claim free fund');
			macroService.PollPattern(patterns.mall.cashShop, { DoClick: true, PredicatePattern: patterns.mall.cashShop.selected });
			macroService.PollPattern(patterns.mall.cashShop.fairyNetSupportFund, { DoClick: true, PredicatePattern: patterns.mall.purchase });
			macroService.PollPattern(patterns.mall.purchase, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.mall });

			macroService.IsRunning && (daily.doDailyShop.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}