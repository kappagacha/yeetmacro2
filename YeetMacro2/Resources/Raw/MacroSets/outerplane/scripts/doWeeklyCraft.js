// Do weekly craft
const loopPatterns = [patterns.lobby.level, patterns.titles.base, patterns.titles.katesWorkshop];
//const weekly = weeklyManager.GetCurrentWeekly();
//if (weekly.doWeeklyCraft.done.IsChecked) {
//	return "Script already completed. Uncheck done to override weekly flag.";
//}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doWeeklyCraft: click base tab');
			macroService.ClickPattern(patterns.tabs.base);
			break;
		case 'titles.base':
			logger.info('doWeeklyCraft: click kate\'s workshop');
			macroService.ClickPattern(patterns.base.katesWorkshop);
			break;
		case 'titles.katesWorkshop':
			logger.info('doWeeklyCraft: click craft consumable');
			macroService.ClickPattern(patterns.base.katesWorkshop.craftConsumable, { });
			macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable, { DoClick: true, ClickOffset: { X: 100, Y: -200 }, PredicatePattern: patterns.base.katesWorkshop.craft });
			macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.selected });


			macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.armorGlunite, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.armorGlunite.selected });
			for (let i = 0; i < 2; i++) {
				macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable });
			}

			macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.specialGearEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.specialGearEnhancement.selected });
			macroService.PollPattern(patterns.base.katesWorkshop.talisman, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.talisman.selected });
			for (let i = 0; i < 4; i++) {
				macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable });
			}

			macroService.PollPattern(patterns.base.katesWorkshop.gear, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.gear.selected });
			for (let i = 0; i < 4; i++) {
				macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable });
			}

			macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement.selected });
			macroService.PollPattern(patterns.base.katesWorkshop.talisman, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.talisman.selected });
			macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable });

			macroService.PollPattern(patterns.base.katesWorkshop.gear, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.gear.selected });
			macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable });

			//if (macroService.IsRunning) {
			//	weekly.doWeeklyCraft.done.IsChecked = true;
			//}
			return;
	}
	sleep(1_000);
}