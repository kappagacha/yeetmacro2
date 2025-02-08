// do daily shop
const loopPatterns = [patterns.lobby.stage, patterns.phone.battery, patterns.mall];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.doDailyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const brilliantGiftBoxPattern = macroService.ClonePattern(patterns.mall.dailyLimited.brilliantGiftBox, {
	X: 400,
	Y: 230,
	Width: resolution.Width - 420,
	Height: 790,
	OffsetCalcType: 'DockLeft'
});

const luxuriousGiftBoxPattern = macroService.ClonePattern(patterns.mall.dailyLimited.luxuriousGiftBox, {
	X: 400,
	Y: 230,
	Width: resolution.Width - 420,
	Height: 790,
	OffsetCalcType: 'DockLeft'
});

const giftBoxPattern = macroService.ClonePattern(patterns.mall.dailyLimited.giftBox, {
	X: 400,
	Y: 230,
	Width: resolution.Width - 420,
	Height: 790,
	OffsetCalcType: 'DockLeft'
});

const fairyNetSupportFundPattern = macroService.ClonePattern(patterns.mall.specialDeals.fairyNetSupportFund, {
	X: 400,
	Y: 230,
	Width: resolution.Width - 420,
	Height: 790,
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('doDailyShop: click phone');
			macroService.ClickPattern(patterns.lobby.phone);
			break;
		case 'phone.battery':
			logger.info('doDailyShop: click mall');
			macroService.ClickPattern(patterns.phone.mall);
			break;
		case 'mall':
			logger.info('doDailyShop: purchase brilliant gift box');
			macroService.PollPattern(patterns.mall.dailyLimited, { DoClick: true, PredicatePattern: patterns.mall.dailyLimited.selected });
			macroService.PollPattern([giftBoxPattern, brilliantGiftBoxPattern, luxuriousGiftBoxPattern], { DoClick: true, PredicatePattern: patterns.mall.purchase });
			const sliderEndResult = macroService.FindPattern(patterns.mall.sliderEnd);
			if (sliderEndResult.IsSuccess) {
				macroService.PollPattern(patterns.mall.sliderEnd, { DoClick: true, InversePredicatePattern: patterns.mall.sliderEnd });
			}
			macroService.PollPattern(patterns.mall.purchase, { DoClick: true, PredicatePattern: patterns.mall.purchase.confirm });
			macroService.PollPattern(patterns.mall.purchase.confirm, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.mall });

			logger.info('doDailyShop: claim free fund');
			macroService.PollPattern(patterns.mall.cashShop, { DoClick: true, PredicatePattern: patterns.mall.cashShop.selected });
			macroService.PollPattern(fairyNetSupportFundPattern, { DoClick: true, PredicatePattern: patterns.mall.purchase });
			macroService.PollPattern(patterns.mall.purchase, { DoClick: true, PredicatePattern: patterns.mall.purchase.confirm });
			macroService.PollPattern(patterns.mall.purchase.confirm, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.mall });

			macroService.IsRunning && (daily.doDailyShop.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}