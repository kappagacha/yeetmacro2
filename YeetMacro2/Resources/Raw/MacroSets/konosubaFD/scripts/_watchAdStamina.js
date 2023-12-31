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
			sleep(500);
			const watchResult = macroService.FindPattern([patterns.ad.stamina.watch, patterns.ad.stamina.watch.disabled]);
			if (watchResult.IsSuccess && watchResult.Path === 'ad.stamina.watch.disabled') {
				return;
			}

			logger.info('watchAdStamina: watching ad');
			macroService.PollPattern(patterns.ad.stamina.watch, { DoClick: true, PredicatePattern: patterns.ad.prompt.ok });
			sleep(1_000);

			logger.info('watchAdStamina: poll ad.prompt.ok');
			macroService.PollPattern(patterns.ad.prompt.ok, {
				DoClick: true,
				ClickPattern: [patterns.ad.exit, patterns.ad.exitInstall, patterns.ad.prompt.youGot, patterns.ad.cancel],
				PredicatePattern: patterns.titles.home
			});
			return;
	}

	sleep(1_000);
}