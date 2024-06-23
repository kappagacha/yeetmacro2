// @position=1
const loopPatterns = [patterns.titles.home, patterns.stamina.prompt.recoverStamina];
const daily = dailyManager.GetCurrentDaily();

if (daily.useFriendBeefs.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('useFriendBeefs: stamina add');
			macroService.ClickPattern(patterns.stamina.add);
			break;
		case 'stamina.prompt.recoverStamina':
			logger.info('useFriendBeefs: check if disabled');

			const zeroFriendBeefPatternResult = macroService.FindPattern(patterns.stamina.zeroFriendBeef);
			if (zeroFriendBeefPatternResult.IsSuccess) {
				if (macroService.IsRunning) {
					daily.useFriendBeefs.done.IsChecked = true;
				}
				return;
			}

			macroService.PollPattern(patterns.stamina.friendBeef, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina2 });
			macroService.PollPattern(patterns.stamina.prompt.max, { DoClick: true, PredicatePattern: patterns.stamina.prompt.reset });
			macroService.PollPattern(patterns.stamina.prompt.recover, { DoClick: true, PredicatePattern: patterns.stamina.prompt.ok });
			macroService.PollPattern(patterns.stamina.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.home });

			if (macroService.IsRunning) {
				daily.useFriendBeefs.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}