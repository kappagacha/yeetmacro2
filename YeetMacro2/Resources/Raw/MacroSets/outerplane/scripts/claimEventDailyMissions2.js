// @position=17
// Claim daily event missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const dailyMissionPattern = macroService.ClonePattern(settings.claimEventDailyMissions2.dailyMissionPattern.Value, {
	X: 90,
	Y: 200,
	Width: 400,
	Height: 800,
	Path: 'settings.claimEventDailyMissions2.dailyMissionPattern2',
	OffsetCalcType: 'Default'
});

if (daily.claimEventDailyMissions2.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimEventDailyMissions2: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimEventDailyMissions2: claim rewards');
			const topLeft = macroService.GetTopLeft();
			const xLocation = topLeft.X + 300 + (resolution.Width - 1920) / 2.0;
			macroService.SwipePollPattern(dailyMissionPattern, { MaxSwipes: 3, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
			macroService.PollPattern(dailyMissionPattern, { DoClick: true, PredicatePattern: patterns.event.eventBulletPoint, IntervalDelayMs: 3_000 });
			sleep(2_000);

			let notificationResult = macroService.PollPattern(patterns.event.event2Notification, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.event2Notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.eventBulletPoint });
				notificationResult = macroService.PollPattern(patterns.event.event2Notification, { TimeoutMs: 3_000 });
			}
			
			if (macroService.IsRunning) {
				daily.claimEventDailyMissions2.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}
