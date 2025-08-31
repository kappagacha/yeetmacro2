// @position=11
// Skip decoy operation
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepEventMiniGame.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const miniGamePattern = macroService.ClonePattern(settings.sweepEventMiniGame.miniGamePattern.Value, {
	X: 10,
	Y: 130,
	Width: 240,
	Height: 930,
	Path: 'settings.sweepEventMiniGame.miniGamePattern',
	OffsetCalcType: 'DockLeft'
});


while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepEventMiniGame: click attendance check');
			const attendanceCheckSwipeResult = macroService.PollPattern(patterns.lobby.attendanceCheck, { SwipePattern: patterns.lobby.attendanceCheck.swipe, TimoutMs: 10_000 });
			if (!attendanceCheckSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: patterns.lobby.attendanceCheck');
			}

			macroService.PollPattern(patterns.lobby.attendanceCheck, { DoClick: true, PredicatePattern: patterns.general.back });
			break;
		case 'general.back':
			const miniGameSwipeResult = macroService.PollPattern(miniGamePattern, { SwipePattern: patterns.event.leftPanelSwipe, TimoutMs: 10_000 });
			if (!miniGameSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: settings.sweepEventMiniGame.miniGamePattern');
			}

			macroService.PollPattern(patterns.event.miniGame, { DoClick: true, PredicatePattern: patterns.event.miniGame.sweep, ClickOffset: { Y: -60 } });
			macroService.PollPattern(patterns.event.miniGame.sweep, { DoClick: true, PredicatePattern: patterns.event.miniGame.sweep.sweep });
			macroService.PollPattern(patterns.event.miniGame.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.event.miniGame.sweep.disabled });

			if (macroService.IsRunning) daily.sweepEventMiniGame.done.IsChecked = true;
			return;
	}

	sleep(1_000);
}