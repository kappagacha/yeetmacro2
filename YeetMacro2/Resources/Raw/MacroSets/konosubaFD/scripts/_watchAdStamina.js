// @isFavorite
// @position=-100
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
const adObjectScale = settings.watchAdStamina.adObjectScale.Value;
const adExitPattern = adObjectScale === 1 ? patterns.ad.exit : macroService.ClonePattern(patterns.ad.exit, { Scale: adObjectScale });
const adExitInstallPattern = adObjectScale === 1 ? patterns.ad.exitInstall : macroService.ClonePattern(patterns.ad.exitInstall, { Scale: adObjectScale });
const adCancelPattern = adObjectScale === 1 ? patterns.ad.cancel : macroService.ClonePattern(patterns.ad.cancel, { Scale: adObjectScale });
const soundLeftPattern = adObjectScale === 1 ? patterns.ad.soundLeft : macroService.ClonePattern(patterns.ad.soundLeft, { Scale: adObjectScale });

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
			macroService.PollPattern(patterns.ad.stamina.watch, { DoClick: true, PredicatePattern: patterns.ad.prompt.ok });
			macroService.PollPattern(patterns.ad.prompt.ok, { DoClick: true, InversePredicatePattern: patterns.ad.prompt.ok });
			macroService.PollPattern(patterns.ad.prompt.ok, {
				ClickPattern: [adExitPattern, adExitInstallPattern, adCancelPattern, soundLeftPattern]
			});
			macroService.PollPattern(patterns.ad.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.home });
			return;
	}

	sleep(1_000);
}