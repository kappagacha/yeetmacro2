// @position=1
// Claim job rewards
const loopPatterns = [patterns.titles.home, patterns.titles.gifts];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimGifts.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('claimGifts: click gifts');
			macroService.ClickPattern(patterns.gifts);
			break;
		case 'titles.gifts':
			logger.info('claimGifts: click acceptAll');
			macroService.PollPattern(patterns.gifts.acceptAll, { DoClick: true, PredicatePattern: patterns.gifts.ok });
			macroService.PollPattern(patterns.gifts.ok, { DoClick: true, PredicatePattern: patterns.gifts.ok2 });
			macroService.PollPattern(patterns.gifts.ok2, { DoClick: true, PredicatePattern: patterns.titles.gifts });

			if (macroService.IsRunning) {
				daily.claimGifts.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}