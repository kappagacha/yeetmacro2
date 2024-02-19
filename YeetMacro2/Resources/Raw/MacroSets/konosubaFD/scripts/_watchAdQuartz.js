// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home];
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
			logger.info('watchAdQuartz: quartz ad');
			const quartzAdNotificationResult = macroService.FindPattern(patterns.ad.quartz.notification);
			if (quartzAdNotificationResult.IsSuccess) {
				logger.info('watchAdQuartz: watching ad');
				logger.info('watchAdQuartz: ad.quartz.notification');
				macroService.PollPattern(patterns.ad.quartz.notification, { DoClick: true, PredicatePattern: patterns.ad.prompt.ok });
				sleep(1_000);
				logger.info('watchAdQuartz: poll ad.prompt.ok');
				macroService.PollPattern(patterns.ad.prompt.ok, {
					DoClick: true,
					ClickPattern: [patterns.ad.prompt.ok, adExitPattern, adExitInstallPattern, patterns.ad.prompt.youGot, adCancelPattern],
					PredicatePattern: patterns.titles.home
				});
				sleep(1_000);
			}
			return;
	}

	sleep(1_000);
}