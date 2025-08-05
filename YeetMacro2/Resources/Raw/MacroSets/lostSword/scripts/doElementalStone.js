// @position=13
const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.battle.select];
const daily = dailyManager.GetCurrentDaily();
if (daily.doElementalStone.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doElementalStone: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doElementalStone: elemental stone');
			macroService.PollPattern(patterns.battle.dungeon, { DoClick: true, PredicatePattern: patterns.battle.selected });
			macroService.PollPattern(patterns.battle.dungeon.elementalStone, { DoClick: true, PredicatePattern: patterns.battle.select });
			break;
		case 'battle.select':
			if (settings.doElementalStone.startOnce.Value && !daily.doElementalStone.started.IsChecked) {
				macroService.PollPattern(patterns.battle.select, { DoClick: true, PredicatePattern: patterns.battle.start });
				macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.select });
				if (macroService.IsRunning) daily.doElementalStone.started.IsChecked = true;
			}

			let sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });

			while (sweepResult.IsSuccess) {
				if (sweepResult.PredicatePath === 'battle.sweep.confirm') {
					macroService.PollPattern(patterns.battle.sweep.dontAskAgain, { DoClick: true, PredicatePattern: patterns.battle.sweep.dontAskAgain.checked });
					macroService.PollPattern(patterns.battle.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				}
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.battle.select });
				sweepResult = macroService.PollPattern(patterns.battle.sweep, { DoClick: true, PredicatePattern: [patterns.battle.sweep.confirm, patterns.general.itemsAcquired], TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) daily.doElementalStone.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}