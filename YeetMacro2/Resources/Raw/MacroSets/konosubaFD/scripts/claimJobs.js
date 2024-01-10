// @position=1
// Claim job rewards
const loopPatterns = [patterns.titles.home, patterns.titles.job];
const daily = dailyManager.GetDaily();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('claimJobs: click jobs');
			macroService.ClickPattern(patterns.jobs);
			break;
		case 'titles.job':
			logger.info('claimJobs: click acceptAll');
			macroService.PollPattern(patterns.jobs.acceptAll.enabled, {
				DoClick: true,
				ClickPattern: [patterns.jobs.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp],
				PredicatePattern: patterns.jobs.acceptAll.disabled
			});
			if (macroService.IsRunning) {
				daily.claimJobs.count.Count++;
			}
			return;
	}

	sleep(1_000);
}