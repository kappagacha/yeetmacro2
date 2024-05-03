// @position=11
// Skip decoy operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.decoyOperation];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const targetDecoyOperation = settings.skipDecoyOperation.targetDecoyOperation.Value;

if (daily.skipDecoyOperation.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('skipDecoyOperation: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('skipDecoyOperation: click decoy operation');
			macroService.PollPattern(patterns.adventure.challenge.decoyOperation, { DoClick: true, ClickOffset: { Y: -100 }, PredicatePattern: patterns.titles.decoyOperation });
			break;
		case 'titles.decoyOperation':
			logger.info('skipDecoyOperation: skip decoy operation');
			macroService.PollPattern(patterns.adventure.challenge.decoyOperation[targetDecoyOperation], { DoClick: true, PredicatePattern: [patterns.adventure.challenge.decoyOperation.sweep.disabled, patterns.adventure.challenge.decoyOperation.sweep.enabled] });
			const sweepResult = macroService.PollPattern([patterns.adventure.challenge.decoyOperation.sweep.disabled, patterns.adventure.challenge.decoyOperation.sweep.enabled]);
			if (sweepResult.Path === 'adventure.challenge.decoyOperation.sweep.disabled') {
				const targetX = resolution.Width - 200;
				const swipeResult = macroService.SwipePollPattern(patterns.adventure.challenge.decoyOperation.sweepAvailable, { Start: { X: targetX, Y: 200 }, End: { X: targetX, Y: 650 } });
				if (!swipeResult.IsSuccess) {
					throw new Error('Unable to find sweep available');
				}
				macroService.PollPattern(patterns.adventure.challenge.decoyOperation.sweepAvailable, { DoClick: true, PredicatePattern: patterns.adventure.challenge.decoyOperation.sweep.enabled });
			}
			macroService.PollPattern(patterns.adventure.challenge.decoyOperation.sweep.enabled, { DoClick: true, PredicatePattern: patterns.adventure.challenge.decoyOperation.sweep.sweep });
			macroService.PollPattern(patterns.adventure.challenge.decoyOperation.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.decoyOperation });

			if (macroService.IsRunning) {
				daily.skipDecoyOperation.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}