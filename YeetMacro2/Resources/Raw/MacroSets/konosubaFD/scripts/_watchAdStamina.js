// patterns.stamina.adNotification
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('watchAdStamina: stamina ad');
			macroService.ClickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('watchAdStamina: check for stamina adNotification');
			//logger.info(JSON.stringify(patterns.stamina.adNotification));
			let staminaAdNotificationResult = macroService.FindPattern(patterns.stamina.adNotification);
			logger.info('staminaAdNotificationResult.IsSuccess: ' + staminaAdNotificationResult.IsSuccess);
			if (!staminaAdNotificationResult.IsSuccess) {
				logger.info('watchAdStamina: stamina ad notification not detected');
				const watchAdResult = macroService.PollPattern(patterns.stamina.watchAd, { DoClick: true, IntervalDelayMs: 1000, PredicatePattern: [patterns.prompt.dailyRewardLimit, patterns.stamina.adNotification] });
				if (watchAdResult.PredicatePath === 'prompt.dailyRewardLimit') {
					logger.info('watchAdStamina: daily reward limit');
					macroService.PollPattern(patterns.ad.prompt.ok, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina });
					return;
				}
			}

			logger.info('watchAdStamina: watching ad');
			logger.info('watchAdStamina: poll stamina.adNotification');
			macroService.PollPattern(patterns.stamina.adNotification, { DoClick: true, ClickPattern: patterns.ad.prompt.ok, PredicatePattern: patterns.ad.done });
			sleep(1_000);
			logger.info('watchAdStamina: poll ad.done');
			macroService.PollPattern(patterns.ad.done, { DoClick: true, PredicatePattern: patterns.ad.prompt.ok });
			sleep(1_000);
			logger.info('watchAdStamina: poll ad.prompt.ok 2');
			macroService.PollPattern(patterns.ad.prompt.ok, { DoClick: true, PredicatePattern: patterns.stamina.add });
			return;
	}

	sleep(1_000);
}