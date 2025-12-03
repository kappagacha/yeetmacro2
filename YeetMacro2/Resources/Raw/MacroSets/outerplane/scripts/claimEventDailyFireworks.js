// @position=17
// @tags=dailies
// Claim daily event missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimEventDailyFireworks.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimEventDailyFireworks: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimEventDailyFireworks: claim rewards');
			macroService.PollPattern(patterns.event.fireworks, { SwipePattern: patterns.event.swipeDown });
			macroService.PollPattern(patterns.event.fireworks, { DoClick: true, PredicatePattern: patterns.event.fireworks.enter, IntervalDelayMs: 3_000 });
			sleep(2_000);

			let notificationResult = macroService.PollPattern(patterns.event.notifications, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.notifications, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				sleep(1_000);
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.fireworks.enter });

				notificationResult = macroService.PollPattern(patterns.event.notifications, { TimeoutMs: 3_000 });
			}
			
			if (macroService.IsRunning) {
				daily.claimEventDailyFireworks.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}
