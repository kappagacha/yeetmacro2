// Sweep event story hard 2
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.eventStory];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const teamSlot = settings.sweepEventStoryHard2.teamSlot.Value;

if (daily.sweepEventStoryHard2.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetPartPattern = macroService.ClonePattern(settings.sweepEventStoryHard2.targetPartPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard2.targetPartPattern',
	OffsetCalcType: 'Center'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
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
		case 'titles.eventStory':
			logger.info('sweepEventStoryHard2: sweep event hard stages');
			macroService.PollPattern(patterns.event.story.enter, { DoClick: true, InversePredicatePattern: patterns.event.story.enter });
			const storyPartSwipeResult = macroService.SwipePollPattern(targetPartPattern, { Start: { X: 1000, Y: 800 }, End: { X: 500, Y: 800 } });

			if (!storyPartSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: settings.sweepEventStoryHard2.targetPartPattern');
			}

			macroService.PollPattern(targetPartPattern, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
			macroService.PollPattern(patterns.event.story.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot, true);

			if (macroService.IsRunning) {
				daily.sweepEventStoryHard2.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}