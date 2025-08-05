// @position=16
const loopPatterns = [patterns.lobby, patterns.menu.mission.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimDailyMissions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('claimDailyMissions: click mission');
			macroService.PollPattern(patterns.menu, { DoClick: true, PredicatePattern: patterns.menu.close });
			if (!macroService.FindPattern(patterns.menu.mission.notification).IsSuccess) {
				if (macroService.IsRunning) daily.claimDailyMissions.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.menu.mission, { DoClick: true, PredicatePattern: patterns.menu.mission.title });
			break;
		case 'menu.mission.title':
			logger.info('claimDailyMissions: claim daily missions');
			macroService.PollPattern(patterns.menu.mission.claimAll, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
			macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.menu.mission.title });

			if (macroService.IsRunning) daily.claimDailyMissions.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}