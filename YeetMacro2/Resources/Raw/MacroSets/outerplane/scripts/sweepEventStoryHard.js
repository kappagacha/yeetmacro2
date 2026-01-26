// @tags=event
// Sweep event story hard
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.enter];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const teamSlot = settings.sweepEventStoryHard.teamSlot.Value;

if (daily.sweepEventStoryHard.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetEventPattern = macroService.ClonePattern(settings.sweepEventStoryHard.targetEventPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard.targetEventPattern',
	OffsetCalcType: 'DockLeft'
});

const targetPartPattern = macroService.ClonePattern(settings.sweepEventStoryHard.targetPartPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard.targetPartPattern',
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [targetEventPattern, patterns.adventure.doNotSeeFor3days] });
	switch (loopResult.Path) {
		case 'lobby.level':
			refillStamina(80);
			logger.info('sweepEventStoryHard: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('sweepEventStoryHard: click event');
			macroService.ClickPattern(patterns.adventure.event);
			sleep(500);
			break;
		case 'event.story.enter':
			logger.info('sweepEventStoryHard: claim rewards');
			let moveNotification = macroService.PollPattern(patterns.event.move.notification, { TimeoutMs: 2_000 });
			if (moveNotification.IsSuccess) {
				macroService.PollPattern(patterns.event.move, { DoClick: true, PredicatePattern: patterns.event.move.close });

				let notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.move.recieve, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.move.close });
					notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
				}

				macroService.PollPattern(patterns.event.move.close, { DoClick: true, PredicatePattern: patterns.event.story.enter });
			}

			logger.info('sweepEventStoryHard: sweep event hard stages');
			macroService.PollPattern(patterns.event.story.enter, { DoClick: true, InversePredicatePattern: patterns.event.story.enter });
			const storyPartSwipeResult = macroService.PollPattern(targetPartPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });

			if (!storyPartSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: settings.sweepEventStoryHard.targetPartPattern');
			}

			macroService.PollPattern(targetPartPattern, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
			macroService.PollPattern(patterns.event.story.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot);

			if (macroService.IsRunning) {
				daily.sweepEventStoryHard.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}