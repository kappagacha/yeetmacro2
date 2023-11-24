const loopPatterns = [patterns.lobby.message, patterns.stamina.purchase];
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.message':
			logger.info('watchAds: click stamina add');
			macroService.ClickPattern(patterns.stamina.add);
			sleep(500);
			break;
		case 'stamina.purchase':
			logger.info('watchAds: start ad');
			if (macroService.FindPattern(patterns.stamina.playAd.free).IsSuccess) {
				macroService.PollPattern(patterns.stamina.playAd, { DoClick: true, PredicatePattern: patterns.stamina.playAd.selected });
				macroService.PollPattern(patterns.stamina.purchase.button, { DoClick: true, ClickPattern: [patterns.ad.exit, patterns.ad.exitInstall], PredicatePattern: patterns.stamina.playAd.rewardTap });
				macroService.PollPattern(patterns.stamina.playAd.rewardTap, { DoClick: true, PredicatePattern: patterns.lobby.message });
			}
			sleep(500);
			break;
	}
	sleep(1_000);
}