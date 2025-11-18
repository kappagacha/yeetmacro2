// @tags=favorites,event
// Sweep event story hard 2
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.enter];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const teamSlot = settings.sweepEventStoryHard2.teamSlot.Value;

if (daily.sweepEventStoryHard2.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetEventPattern = macroService.ClonePattern(settings.sweepEventStoryHard2.targetEventPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard2.targetEventPattern',
	OffsetCalcType: 'DockLeft'
});

const targetPartPattern = macroService.ClonePattern(settings.sweepEventStoryHard2.targetPartPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard2.targetPartPattern',
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [targetEventPattern, patterns.adventure.doNotSeeFor3days] });
	switch (loopResult.Path) {
		case 'lobby.level':
			refillStamina(80);
			logger.info('sweepEventStoryHard2: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('sweepEventStoryHard2: click event');
			macroService.ClickPattern(patterns.adventure.event);
			sleep(500);
			break;
		case 'event.story.enter':
			logger.info('sweepEventStoryHard2: sweep event hard stages');
			macroService.PollPattern(patterns.event.story.enter, { DoClick: true, InversePredicatePattern: patterns.event.story.enter });
			const storyPartSwipeResult = macroService.PollPattern(targetPartPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });

			if (!storyPartSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: settings.sweepEventStoryHard2.targetPartPattern');
			}

			macroService.PollPattern(targetPartPattern, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
			macroService.PollPattern(patterns.event.story.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot);

			if (macroService.IsRunning) {
				daily.sweepEventStoryHard2.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}