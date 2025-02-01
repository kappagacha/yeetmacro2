// @isFavorite
// @position=-1
// Sell inventory - normal and superior grade
const loopPatterns = [patterns.lobby.level, patterns.titles.inventory];
const daily = dailyManager.GetCurrentDaily();
if (daily.sellInventory.done.IsChecked && !settings.sellInventory.forceRun.Value) {
	return "Script already completed. Uncheck done to override daily flag.";
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
			sellNormalAndSuperior();
			sellLowStarEpic();

			if (macroService.IsRunning) {
				daily.sellInventory.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function sellNormalAndSuperior() {
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
}

function sellLowStarEpic() {
	macroService.PollPattern(patterns.inventory.sell, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
	macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
	for (let i = 1; i < 6; i++) { // star rarity 1-5
		macroService.PollPattern(patterns.inventory.filter.starLevel[i].disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.starLevel[i].enabled });
	}
	macroService.PollPattern(patterns.inventory.filter.grade.epic.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.epic.enabled });

	const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory.sell.enabled, patterns.inventory.sell.disabled] });
	if (sellResult.PredicatePath === 'inventory.sell.disabled') {
		return;
	}
	macroService.PollPattern(patterns.inventory.sell.enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
	macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
	macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
}