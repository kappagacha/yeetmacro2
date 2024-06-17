// @isFavorite
// @position=1000
// Watches all ads in a loop

const loopPatterns = [patterns.lobby.level, patterns.stamina.purchase];
const daily = dailyManager.GetCurrentDaily();
if (daily.watchAds.done.IsChecked || daily.watchAds.count.Count >= 15) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const originalDensity = 1.5;	// density the patterns were captured in
const currentDensity = macroService.GetScreenDensity();
// scale calculation worked for density 2.0 and 2.7875. no clue if this will work for others
const scale = currentDensity / originalDensity * 150.0 / 223.0;
const adExitPattern = originalDensity === currentDensity ? patterns.ad.exit : macroService.ClonePattern(patterns.ad.exit, { Scale: scale });
const adExitInstallPattern = originalDensity === currentDensity ? patterns.ad.exitInstall : macroService.ClonePattern(patterns.ad.exitInstall, { Scale: scale });
const userClickPattern = macroService.ClonePattern(settings.watchAds.userClickPattern.Value, {
	Path: 'settings.watchAds.userClickPattern'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('watchAds: click stamina add');
			macroService.ClickPattern(patterns.stamina.add);
			sleep(500);
			break;
		case 'stamina.purchase':
			logger.info('watchAds: start ad');
			const playAdResult = macroService.PollPattern(patterns.stamina.playAd, { TimeoutMs: 2_000 });
			if (!playAdResult.IsSuccess) {
				if (macroService.IsRunning) {
					daily.watchAds.done.IsChecked = true;
				}
				return;
			}
			if (macroService.FindPattern(patterns.stamina.playAd.free).IsSuccess) {
				macroService.PollPattern(patterns.stamina.playAd, { DoClick: true, PredicatePattern: patterns.stamina.playAd.selected });
				macroService.PollPattern(patterns.stamina.purchase.button, { DoClick: true, ClickPattern: [adExitInstallPattern, adExitPattern, userClickPattern], PredicatePattern: patterns.stamina.playAd.rewardTap });
				if (macroService.IsRunning) {
					daily.watchAds.count.Count++;
				}
				macroService.PollPattern(patterns.stamina.playAd.rewardTap, { DoClick: true, PredicatePattern: patterns.lobby.level });
			}
			sleep(500);
			break;
	}
	sleep(1_000);
}