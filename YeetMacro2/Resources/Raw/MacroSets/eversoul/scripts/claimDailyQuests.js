// Claim daily quests
const loopPatterns = [patterns.lobby.level, patterns.titles.quest];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimDailyQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimDailyQuests: click quest');
			macroService.ClickPattern(patterns.lobby.quest);
			break;
		case 'titles.quest':
			logger.info('claimDailyQuests: receive all');
			macroService.PollPattern(patterns.quest.receiveAll, { DoClick: true, PredicatePattern: patterns.quest.tapTheScreen });
			macroService.PollPattern(patterns.quest.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.quest });

			logger.info('claimDailyQuests: done');
			if (macroService.IsRunning) {
				daily.claimDailyQuests.done.IsChecked = true;
			}

			return;
	}

	sleep(1_000);
}