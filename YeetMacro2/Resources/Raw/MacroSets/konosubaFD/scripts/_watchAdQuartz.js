// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home];
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
					ClickPattern: [patterns.ad.exit, patterns.ad.exitInstall, patterns.ad.prompt.youGot],
					PredicatePattern: patterns.titles.home
				});
				sleep(1_000);
			}
			return;
	}

	sleep(1_000);
}