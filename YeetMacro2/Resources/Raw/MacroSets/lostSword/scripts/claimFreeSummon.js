const loopPatterns = [patterns.lobby, patterns.summon.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimFreeSummon.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('claimFreeSummon: click summon');
			macroService.ClickPattern(patterns.summon);
			break;
		case 'summon.title':
			logger.info('claimFreeSummon: claim free summons');

			if (!daily.claimFreeSummon.character.IsChecked) {
				logger.info('claimFreeSummon: claim free character summon');
				macroService.PollPattern(patterns.summon.characterSummon, { SwipePattern: patterns.summon.swipeDown });
				macroService.PollPattern(patterns.summon.characterSummon, { DoClick: true, PredicatePattern: patterns.summon.free });
				macroService.PollPattern(patterns.summon.free, { DoClick: true, ClickPattern: patterns.summon.skip, PredicatePattern: patterns.summon.viewAll });
				macroService.PollPattern(patterns.summon.viewAll, { DoClick: true, PredicatePattern: patterns.summon.confirm });
				macroService.PollPattern(patterns.summon.confirm, { DoClick: true, PredicatePattern: patterns.summon.title });

				if (macroService.IsRunning) daily.claimFreeSummon.character.IsChecked = true;
			}


			if (macroService.IsRunning) daily.claimFreeSummon.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}