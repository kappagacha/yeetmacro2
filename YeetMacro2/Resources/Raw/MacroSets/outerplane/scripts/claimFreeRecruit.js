// Claim daily recruit
const loopPatterns = [patterns.lobby.level, patterns.titles.recruit];
const daily = dailyManager.GetDaily();
if (daily.claimFreeRecruit.done.IsChecked) {
	return;
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimFreeRecruit: click recruit tab');
			const recruitshopNotificationResult = macroService.PollPattern(patterns.tabs.recruit.notification, { TimoutMs: 1_000 });
			if (recruitshopNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.recruit);
			} else {	// already claimed
				return;
			}
			sleep(500);
			break;
		case 'titles.recruit':
			logger.info('claimFreeRecruit: claim Normal');
			const swipeResult = macroService.SwipePollPattern(patterns.recruit.normal, { Start: { X: 650, Y: 950 }, End: { X: 250, Y: 950 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find normal recruit');
			}
			macroService.PollPattern(patterns.recruit.normal, { DoClick: true, PredicatePattern: patterns.recruit.normal.ticket });
			macroService.PollPattern(patterns.recruit.normal.free, { DoClick: true, PredicatePattern: patterns.recruit.prompt.ok });
			macroService.PollPattern(patterns.recruit.prompt.ok, { DoClick: true, ClickPattern: patterns.recruit.skip, PredicatePattern: patterns.titles.recruit });

			if (macroService.IsRunning) {
				daily.claimFreeRecruit.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}