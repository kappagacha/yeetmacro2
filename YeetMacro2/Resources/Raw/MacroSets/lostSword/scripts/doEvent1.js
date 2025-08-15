// @position=15
const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.battle.select];
const daily = dailyManager.GetCurrentDaily();
if (daily.doEvent1.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doEvent1: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doEvent1: click event 1');
			macroService.PollPattern(patterns.battle.event, { DoClick: true, PredicatePattern: patterns.battle.event.selected });
			macroService.PollPattern(patterns.battle.event.event1, { DoClick: true, PredicatePattern: patterns.battle.select });
			break;
		case 'battle.select':
			if (settings.doEvent1.startOnce.Value && !daily.doEvent1.started.IsChecked) {
				logger.info('doEvent1: startOnce');
				macroService.PollPattern(patterns.battle.select, { DoClick: true, PredicatePattern: patterns.battle.start });
				const startResult = macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: [patterns.battle.select, patterns.battle.event.confirm] });
				if (startResult.PredicatePath === 'battle.event.confirm') {
					macroService.PollPattern(patterns.battle.event.confirm, { DoClick: true, PredicatePattern: patterns.battle.start });
				}
				if (macroService.IsRunning) daily.doEvent1.started.IsChecked = true;
			}

			let sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });

			logger.info('doEvent1: do sweep');
			while (sweepResult.IsSuccess) {
				if (sweepResult.PredicatePath === 'battle.sweep.confirm') {
					macroService.PollPattern(patterns.battle.sweep.dontAskAgain, { DoClick: true, PredicatePattern: patterns.battle.sweep.dontAskAgain.checked });
					macroService.PollPattern(patterns.battle.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				}
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.battle.select });
				sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) daily.doEvent1.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}