// patterns.ad.quartz.notification
let done = false;
const loopPatterns = [patterns.titles.home];
while (macroService.IsRunning && !done) {
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
				logger.info('watchAdQuartz: poll ad.prompt.ok 1');
				macroService.PollPattern(patterns.ad.prompt.ok, { DoClick: true, PredicatePattern: patterns.ad.done });
				sleep(1_000);
				logger.info('watchAdQuartz: poll ad.done');
				macroService.PollPattern(patterns.ad.done, { DoClick: true, ClickPattern: patterns.ad.prompt.youGot, PredicatePattern: patterns.titles.home });
			}
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');