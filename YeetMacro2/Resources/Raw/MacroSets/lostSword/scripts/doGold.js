const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.bossRaid];
const daily = dailyManager.GetCurrentDaily();
if (daily.doGold.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doGold: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doGold: click gold');
			macroService.PollPattern(patterns.battle.dungeon, { DoClick: true, PredicatePattern: patterns.battle.dungeon.selected });
			macroService.PollPattern(patterns.gold, { DoClick: true, PredicatePattern: patterns.gold.select });
			break;
		case 'bossRaid':
			if (settings.doGold.startOnce.Value && !daily.doGold.started.IsChecked) {
				macroService.PollPattern(patterns.gold.select, { DoClick: true, PredicatePattern: patterns.gold.start });
				macroService.PollPattern(patterns.gold.start, { DoClick: true, PredicatePattern: patterns.gold.select });
				if (macroService.IsRunning) daily.doGold.started.IsChecked = true;
			}

			let sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });

			while (sweepResult.IsSuccess) {
				if (sweepResult.PredicatePath === 'battle.sweep.confirm') {
					macroService.PollPattern(patterns.battle.sweep.dontAskAgain, { DoClick: true, PredicatePattern: patterns.battle.sweep.dontAskAgain.checked });
					macroService.PollPattern(patterns.battle.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
					macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.bossRaid });
				}
				sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) daily.doGold.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}