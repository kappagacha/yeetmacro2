// patterns.ad.quartz.notification
let done = false;
const loopPatterns = [patterns.titles.home];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('watchAdQuartz: quartz ad');
			const quartzAdNotificationResult = await macroService.findPattern(patterns.ad.quartz.notification);
			if (quartzAdNotificationResult.isSuccess) {
				logger.info('watchAdQuartz: watching ad');
				logger.info('watchAdQuartz: ad.quartz.notification');
				await macroService.pollPattern(patterns.ad.quartz.notification, { doClick: true, predicatePattern: patterns.ad.prompt.ok });
				await sleep(1_000);
				logger.info('watchAdQuartz: poll ad.prompt.ok 1');
				await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.ad.done });
				await sleep(1_000);
				logger.info('watchAdQuartz: poll ad.done');
				await macroService.pollPattern(patterns.ad.done, { doClick: true, clickPattern: patterns.ad.prompt.youGot, predicatePattern: patterns.titles.home });
			}
			done = true;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');