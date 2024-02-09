// Claim daily missions
const loopPatterns = [patterns.lobby.level, patterns.titles.mission];
const daily = dailyManager.GetDaily();

if (daily.claimDailyMissions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimDailyMissions: click mission');
			macroService.ClickPattern(patterns.tabs.mission);
			sleep(500);
			break;
		case 'titles.mission':
			logger.info('claimDailyMissions: claim final reward');
			const finalNotificationResult = macroService.PollPattern(patterns.mission.finalNotification, { TimeoutMs: 1_500, DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			if (!finalNotificationResult.IsSuccess) {
				return;
			}
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mission });
			if (macroService.IsRunning) {
				daily.claimDailyMissions.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}