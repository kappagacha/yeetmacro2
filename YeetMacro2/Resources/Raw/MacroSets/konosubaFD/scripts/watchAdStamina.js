﻿// patterns.stamina.adNotification
let done = false;
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('watchAdStamina: stamina ad');
			await macroService.clickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('watchAdStamina: check for stamina adNotification');
			logger.info(JSON.stringify(patterns.stamina.adNotification));
			let staminaAdNotificationResult = await macroService.findPattern(patterns.stamina.adNotification);
			logger.info('staminaAdNotificationResult.isSuccess: ' + staminaAdNotificationResult.isSuccess);
			if (!staminaAdNotificationResult.isSuccess) {
				logger.info('watchAdStamina: stamina ad notification not detected');
				const watchAdResult = await macroService.pollPattern(patterns.stamina.watchAd, { doClick: true, intervalDelayMs: 1000, predicatePattern: [patterns.prompt.dailyRewardLimit, patterns.stamina.adNotification] });
				if (watchAdResult.predicatePath === 'prompt.dailyRewardLimit') {
					logger.info('watchAdStamina: daily reward limit');
					await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina });
					done = true;
					break;
				}
			}

			const errorResult = await macroService.findPattern(patterns.ad.prompt.error);
			if (errorResult.isSuccess) {
				await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.add });
			}

			logger.info('watchAdStamina: watching ad');
			logger.info('watchAdStamina: poll stamina.adNotification');
			const pollAdNotificationResult = await macroService.pollPattern(patterns.stamina.adNotification, { doClick: true, predicatePattern: [patterns.ad.prompt.ok, patterns.ad.prompt.error] });
			if (pollAdNotificationResult.predicatePath === 'ad.prompt.error') {
				logger.info('watchAdStamina: handle error');
				await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.adNotification });
				sleep(500);
				await macroService.pollPattern(patterns.stamina.adNotification, { doClick: true, predicatePattern: patterns.ad.prompt.ok });
			}
			await sleep(1_000);

			logger.info('watchAdStamina: poll ad.prompt.ok 1');
			await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.ad.done });
			await sleep(1_000);
			logger.info('watchAdStamina: poll ad.done');
			await macroService.pollPattern(patterns.ad.done, { doClick: true, predicatePattern: patterns.ad.prompt.ok });
			await sleep(1_000);
			logger.info('watchAdStamina: poll ad.prompt.ok 2');
			await macroService.pollPattern(patterns.ad.prompt.ok, { doClick: true, predicatePattern: patterns.stamina.add });

			done = true;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');