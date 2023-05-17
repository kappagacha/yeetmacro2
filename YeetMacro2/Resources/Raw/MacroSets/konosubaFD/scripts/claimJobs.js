// patterns.jobs.notification
let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.job];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('farmEventLoop: click jobs');
			await macroService.clickPattern(patterns.jobs);
			break;
		case 'titles.job':
			logger.info('farmEventLoop: click acceptAll');
			const acceptAllResult = await macroService.findPattern([patterns.jobs.acceptAll.enabled, patterns.jobs.acceptAll.disabled]);
			if (acceptAllResult.isSuccess && acceptAllResult.path === 'jobs.acceptAll.enabled') {
				logger.info('farmEventLoop: jobs.acceptAll.enabled');
				await macroService.pollPattern(patterns.jobs.acceptAll.enabled, { doClick: true, predicatePattern: patterns.jobs.prompt.ok });
				await macroService.pollPattern(patterns.jobs.prompt.ok, { doClick: true, predicatePattern: patterns.titles.job });
			} else if (acceptAllResult.isSuccess) {		// jobs.acceptAll.disabled
				logger.info('farmEventLoop: jobs.acceptAll.disabled');
				done = true;
			}
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');