// @position=11
// Skip dual gate
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.dualGate];
const daily = dailyManager.GetDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.skipDualGate.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('skipDualGate: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('skipDualGate: click dual gate');
			macroService.PollPattern(patterns.adventure.challenge.dualGate, { DoClick: true, ClickOffset: { Y: -100 }, PredicatePattern: patterns.titles.dualGate });
			break;
		case 'titles.dualGate':
			logger.info('skipDualGate: skip dual gate');
			const sweepResult = macroService.PollPattern([patterns.adventure.challenge.dualGate.sweep.disabled, patterns.adventure.challenge.dualGate.sweep.enabled]);
			if (sweepResult.Path === 'adventure.challenge.dualGate.sweep.disabled') {
				const targetX = resolution.Width - 200;
				const swipeResult = macroService.SwipePollPattern(patterns.adventure.challenge.dualGate.sweepAvailable, { Start: { X: targetX, Y: 200 }, End: { X: targetX, Y: 650 } });
				if (!swipeResult.IsSuccess) {
					throw new Error('Unable to find sweep available');
				}
				macroService.PollPattern(patterns.adventure.challenge.dualGate.sweepAvailable, { DoClick: true, PredicatePattern: patterns.adventure.challenge.dualGate.sweep.enabled });
			}
			macroService.PollPattern(patterns.adventure.challenge.dualGate.sweep.enabled, { DoClick: true, PredicatePattern: patterns.adventure.challenge.dualGate.sweep.sweep });
			macroService.PollPattern(patterns.adventure.challenge.dualGate.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.dualGate });

			if (macroService.IsRunning) {
				daily.skipDualGate.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}