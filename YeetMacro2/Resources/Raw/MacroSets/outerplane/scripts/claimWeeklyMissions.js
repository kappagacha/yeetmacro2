// Claim weekly missions
const loopPatterns = [patterns.lobby.level, patterns.titles.mission];
//const weekly = weeklyManager.GetWeekly();
const weekly = weeklyManager.GetCurrentWeelky();

if (weekly.claimWeeklyMissions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimWeeklyMissions: click mission');
			macroService.ClickPattern(patterns.tabs.mission);
			sleep(500);
			break;
		case 'titles.mission':
			logger.info('claimWeeklyMissions: claim final reward');
			macroService.PollPattern(patterns.mission.weekly, { DoClick: true, PredicatePattern: patterns.mission.weekly.selected });
			const finalNotificationResult = macroService.PollPattern(patterns.mission.finalNotification, { TimeoutMs: 2_000, DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			if (!finalNotificationResult.IsSuccess) {
				return;
			}
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mission });
			if (macroService.IsRunning) {
				weekly.claimWeeklyMissions.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}