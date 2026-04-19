// @isFavorite
// @tags=inventory
// @position=-1
// Sell inventory - normal and superior grade
const loopPatterns = [patterns.lobby.level, patterns.titles.inventory];
const daily = dailyManager.GetCurrentDaily();
const weekly = weeklyManager.GetCurrentWeekly();
const doDismantle = !weekly.sellInventory.doDismantleOnce.IsChecked;

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
			sellNormalAndSuperior(doDismantle);
			sellLowStarEpic(doDismantle);

			if (macroService.IsRunning) {
				daily.sellInventory.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function sellNormalAndSuperior(doDismantle) {
	const action = doDismantle ? 'dismantle' : 'sell';

	macroService.PollPattern(patterns.inventory[action], { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
	macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
	macroService.PollPattern(patterns.inventory.sell.auto.grade.normal.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.normal.enabled });
	macroService.PollPattern(patterns.inventory.sell.auto.grade.superior.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.superior.enabled });

	const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory[action].enabled, patterns.inventory[action].disabled] });
	if (sellResult.PredicatePath === `inventory.${action}.disabled`) {
		return;
	}
	macroService.PollPattern(patterns.inventory[action].enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
	macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
	macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
}

function sellLowStarEpic(doDismantle) {
	const action = doDismantle ? 'dismantle' : 'sell';
	macroService.PollPattern(patterns.inventory[action], { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
	macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
	for (let i = 1; i < 6; i++) { // star rarity 1-5
		macroService.PollPattern(patterns.inventory.filter.starLevel[i].disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.starLevel[i].enabled });
	}
	macroService.PollPattern(patterns.inventory.filter.grade.epic.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.epic.enabled });

	const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory[action].enabled, patterns.inventory[action].disabled] });
	if (sellResult.PredicatePath === `inventory.${action}.disabled`) {
		return;
	}
	macroService.PollPattern(patterns.inventory[action].enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
	macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
	macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
}