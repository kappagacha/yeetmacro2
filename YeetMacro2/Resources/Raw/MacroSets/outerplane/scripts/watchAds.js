// Watches all adds in a loop
const loopPatterns = [patterns.lobby.message, patterns.stamina.purchase];
const daily = dailyManager.GetDaily();
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
			const playAdResult = macroService.PollPattern(patterns.stamina.playAd, { TimoutMs: 2_000 });
			if (!playAdResult.IsSuccess) {
				return;
			}
			if (macroService.FindPattern(patterns.stamina.playAd.free).IsSuccess) {
				macroService.PollPattern(patterns.stamina.playAd, { DoClick: true, PredicatePattern: patterns.stamina.playAd.selected });
				macroService.PollPattern(patterns.stamina.purchase.button, { DoClick: true, ClickPattern: [patterns.ad.exit, patterns.ad.exitInstall], PredicatePattern: patterns.stamina.playAd.rewardTap });
				daily._watchAds.count++;
				dailyManager.UpdateDaily(daily);
				macroService.PollPattern(patterns.stamina.playAd.rewardTap, { DoClick: true, PredicatePattern: patterns.lobby.message });
			}
			sleep(500);
			break;
	}
	sleep(1_000);
}