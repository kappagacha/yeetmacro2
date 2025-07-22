// @position=3
// Claim daily recruit
const loopPatterns = [patterns.lobby.level, patterns.titles.recruit];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimFreeRecruit.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimFreeRecruit: click recruit tab');
			const recruitNotificationResult = macroService.PollPattern(patterns.tabs.recruit.notification, { TimeoutMs: 2_000 });
			if (recruitNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.recruit);
			} else {	// already claimed
				return;
			}
			sleep(500);
			break;
		case 'titles.recruit':
			logger.info('claimFreeRecruit: claim recruit');
			for (let i = 0; i < 2; i++) {
				const swipeResult = macroService.PollPattern(patterns.recruit.notification, { SwipePattern: patterns.general.leftPanelSwipeDown, TimeoutMs: 5_000 });
				if (!swipeResult.IsSuccess) {
					throw new Error('Unable to find notification');
				}
				sleep(1_000);
				//macroService.PollPattern(patterns.recruit.normal, { DoClick: true, PredicatePattern: patterns.recruit.normal.ticket });
				macroService.PollPattern(patterns.recruit.notification, { DoClick: true, PredicatePattern: patterns.recruit.normal.free });
				macroService.PollPattern(patterns.recruit.normal.free, { DoClick: true, PredicatePattern: patterns.recruit.prompt.ok });
				macroService.PollPattern(patterns.recruit.prompt.ok, { DoClick: true, ClickPattern: patterns.recruit.skip, PredicatePattern: patterns.recruit.prompt.ok2 });
				macroService.PollPattern(patterns.recruit.prompt.ok2, { DoClick: true, InversePredicatePattern: patterns.recruit.prompt.ok2 });
			}
			
			if (macroService.IsRunning) {
				daily.claimFreeRecruit.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}