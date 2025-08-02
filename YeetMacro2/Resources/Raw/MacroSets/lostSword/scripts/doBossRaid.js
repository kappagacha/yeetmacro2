const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.bossRaid];
const daily = dailyManager.GetCurrentDaily();
if (daily.doBossRaid.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doBossRaid: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doColosseum: click boss raid');
			macroService.ClickPattern(patterns.battle.bossRaid);
			break;
		case 'bossRaid':
			if (settings.doBossRaid.enterOnce.Value) {
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
				macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.bossRaid });
				if (macroService.IsRunning) daily.doBossRaid.entered.IsChecked = true;
			}

			let sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });


			while (sweepResult.IsSuccess) {
				if (sweepResult.PredicatePath === 'battle.sweep.confirm') {
					macroService.PollPattern(patterns.battle.sweep.dontAskAgain, { DoClick: true, PredicatePattern: patterns.battle.sweep.dontAskAgain.checked });
					macroService.PollPattern(patterns.battle.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
					macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.bossRaid });
				}
				macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) daily.doBossRaid.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}