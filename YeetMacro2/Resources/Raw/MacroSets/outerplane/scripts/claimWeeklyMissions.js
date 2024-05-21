// Claim weekly missions
const loopPatterns = [patterns.lobby.level, patterns.titles.mission];
const weekly = weeklyManager.GetCurrentWeekly();
const dayOfWeek = weeklyManager.GetDayOfWeek();

// dayOfWeek is UTC which is a day forward; 0 is UTC Sunday, which is local Saturday
if (dayOfWeek !== 0 && dayOfWeek < 5) return;	// Needs to be at least Friday

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
			const finalNotificationResult = macroService.PollPattern(patterns.mission.finalNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
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