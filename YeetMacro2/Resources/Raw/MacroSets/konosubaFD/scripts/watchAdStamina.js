// patterns.stamina.adNotification
let done = false;
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
while (state.isRunning && !done) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('watchAdStamina: stamina ad');
			macroService.clickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('watchAdStamina: check for stamina adNotification');
			logger.info(JSON.stringify(patterns.stamina.adNotification));
			let staminaAdNotificationResult = macroService.findPattern(patterns.stamina.adNotification);
			logger.info('staminaAdNotificationResult.isSuccess: ' + staminaAdNotificationResult.isSuccess);
			if (!staminaAdNotificationResult.isSuccess) {
				logger.info('watchAdStamina: stamina ad notification not detected');
				const watchAdResult = macroService.pollPattern(patterns.stamina.watchAd, { doClick: true, intervalDelayMs: 1000, predicatePattern: [patterns.prompt.dailyRewardLimit, patterns.stamina.adNotification] });
				if (watchAdResult.predicatePath === 'prompt.dailyRewardLimit') {
					logger.info('watchAdStamina: daily reward limit');
					macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina });
					done = true;
					break;
				}
			}

			logger.info('watchAdStamina: watching ad');
			logger.info('watchAdStamina: poll stamina.adNotification');
			macroService.pollPattern(patterns.stamina.adNotification, { doClick: true, clickPattern: patterns.ad.prompt.ok, predicatePattern: patterns.ad.done });
			sleep(1_000);
			logger.info('watchAdStamina: poll ad.done');
			macroService.pollPattern(patterns.ad.done, { doClick: true, predicatePattern: patterns.ad.prompt.ok });
			sleep(1_000);
			logger.info('watchAdStamina: poll ad.prompt.ok 2');
			macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.add });

			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');