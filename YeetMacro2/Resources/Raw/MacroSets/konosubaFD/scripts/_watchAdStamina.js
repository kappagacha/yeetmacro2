// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
const originalDensity = 1.5;	// density the patterns were captured in
const currentDensity = macroService.GetScreenDensity();
// scale calculation worked for density 2.0 and 2.7875. no clue if this will work for others
const scale = currentDensity / originalDensity * 150.0 / 223.0;
const adExitPattern = originalDensity === currentDensity ? patterns.ad.exit : macroService.ClonePattern(patterns.ad.exit, { Scale: scale });
const adExitInstallPattern = originalDensity === currentDensity ? patterns.ad.exitInstall : macroService.ClonePattern(patterns.ad.exitInstall, { Scale: scale });
const adCancelPattern = originalDensity === currentDensity ? patterns.ad.cancel : macroService.ClonePattern(patterns.ad.cancel, { Scale: scale });

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('watchAdStamina: stamina ad');
			macroService.ClickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('watchAdStamina: check if disabled');
			const watchResult = macroService.PollPattern(patterns.ad.stamina.watch, { TimeoutMs: 3_000 });
			if (!watchResult.IsSuccess) {
				return;
			}

			logger.info('watchAdStamina: watching ad');
			macroService.PollPattern(patterns.ad.stamina.watch, {
				DoClick: true,
				ClickPattern: [patterns.ad.prompt.ok, adExitPattern, adExitInstallPattern, patterns.ad.prompt.youGot, adCancelPattern],
				PredicatePattern: patterns.titles.home
			});
			return;
	}

	sleep(1_000);
}