// Sell inventory - normal and superior grade
const loopPatterns = [patterns.lobby.level, patterns.titles.inventory];
const weekly = weeklyManager.GetCurrentWeekly();
if (weekly.sellInventory.done.IsChecked && !settings.sellInventory.forceRun.Value) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sellInventory: click inventory tab');
			macroService.ClickPattern(patterns.tabs.inventory);
			sleep(500);
			break;
		case 'titles.inventory':
			logger.info('sellInventory: go to survey hub');
			macroService.PollPattern(patterns.inventory.sell, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
			macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
			macroService.PollPattern(patterns.inventory.sell.auto.grade.normal.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.normal.enabled });
			macroService.PollPattern(patterns.inventory.sell.auto.grade.superior.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.superior.enabled });
			const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory.sell.enabled, patterns.inventory.sell.disabled] });
			if (sellResult.PredicatePath === 'inventory.sell.disabled') {
				return;
			}
			macroService.PollPattern(patterns.inventory.sell.enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
			macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });

			if (macroService.IsRunning) {
				weekly.sellInventory.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}