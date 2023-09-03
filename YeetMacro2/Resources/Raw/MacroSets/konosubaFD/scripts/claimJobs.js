// patterns.jobs.notification
let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.job];
while (macroService.IsRunning && !done) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('farmEventLoop: click jobs');
			macroService.ClickPattern(patterns.jobs);
			break;
		case 'titles.job':
			logger.info('farmEventLoop: click acceptAll');
			const acceptAllResult = macroService.FindPattern([patterns.jobs.acceptAll.enabled, patterns.jobs.acceptAll.disabled]);
			if (acceptAllResult.IsSuccess && acceptAllResult.Path === 'jobs.acceptAll.enabled') {
				logger.info('farmEventLoop: jobs.acceptAll.enabled');
				macroService.PollPattern(patterns.jobs.acceptAll.enabled, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.jobs.prompt.ok });
				macroService.PollPattern(patterns.jobs.prompt.ok, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.titles.job });
			} else if (acceptAllResult.IsSuccess) {		// jobs.acceptAll.disabled
				logger.info('farmEventLoop: jobs.acceptAll.disabled');
				done = true;
			}
			break;
	}

	sleep(1_000);
}
logger.info('Done...');