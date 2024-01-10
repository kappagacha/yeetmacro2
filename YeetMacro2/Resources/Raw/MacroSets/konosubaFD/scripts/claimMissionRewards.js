// @position=9
// Claim mission rewards
const loopPatterns = [patterns.titles.home, patterns.titles.missions];
const daily = dailyManager.GetDaily();
if (daily.claimMissionRewards.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('claimMissionRewards: click missions');
			macroService.ClickPattern(patterns.missions);
			break;
		case 'titles.missions':
			logger.info('claimMissionRewards: click acceptAll');
			macroService.PollPattern(patterns.missions.acceptAll, {
				DoClick: true,
				ClickPattern: [patterns.jobs.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp],
				PredicatePattern: patterns.missions.acceptAll.disabled
			});
			if (macroService.IsRunning) {
				daily.claimMissionRewards.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}