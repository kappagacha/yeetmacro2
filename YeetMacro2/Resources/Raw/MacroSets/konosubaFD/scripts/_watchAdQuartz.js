// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home];
const daily = dailyManager.GetCurrentDaily();
const adObjectScale = settings.watchAdQuartz.adObjectScale.Value;
const adExitPattern = adObjectScale === 1 ? patterns.ad.exit : macroService.ClonePattern(patterns.ad.exit, { Scale: adObjectScale });
const adExitInstallPattern = adObjectScale === 1 ? patterns.ad.exitInstall : macroService.ClonePattern(patterns.ad.exitInstall, { Scale: adObjectScale });
const adCancelPattern = adObjectScale === 1 ? patterns.ad.cancel : macroService.ClonePattern(patterns.ad.cancel, { Scale: adObjectScale });
const soundLeftPattern = adObjectScale === 1 ? patterns.ad.soundLeft : macroService.ClonePattern(patterns.ad.soundLeft, { Scale: adObjectScale });
const userClickPattern = macroService.ClonePattern(settings.watchAdQuartz.userClickPattern.Value, {
	Path: 'settings.watchAdQuartz.userClickPattern'
});

if (daily.watchAdQuartz.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

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
					ClickPattern: [adExitInstallPattern, adExitPattern, adCancelPattern, soundLeftPattern, userClickPattern],
					PredicatePattern: patterns.ad.prompt.youGot
				});
				macroService.PollPattern(patterns.ad.prompt.youGot, { DoClick: true, PredicatePattern: patterns.titles.home });
				if (macroService.IsRunning) {
					daily.watchAdQuartz.done.IsChecked = true;
				}
			}
			return;
	}

	sleep(1_000);
}