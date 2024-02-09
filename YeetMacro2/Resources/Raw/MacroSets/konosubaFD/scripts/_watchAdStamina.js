// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('watchAdStamina: stamina ad');
			macroService.ClickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('watchAdStamina: check if disabled');
			const watchResult = macroService.PollPattern(patterns.ad.stamina.watch, { TimeoutMs: 3_000 });
			if (!watchResult.IsSuccess) {
				return;
			}

			logger.info('watchAdStamina: watching ad');
			macroService.PollPattern(patterns.ad.stamina.watch, {
				DoClick: true,
				ClickPattern: [patterns.ad.prompt.ok, patterns.ad.exit, patterns.ad.exitInstall, patterns.ad.prompt.youGot, patterns.ad.cancel],
				PredicatePattern: patterns.titles.home
			});
			return;
	}

	sleep(1_000);
}