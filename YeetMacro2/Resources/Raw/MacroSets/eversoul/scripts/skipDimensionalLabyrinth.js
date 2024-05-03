// @position=11
// Skip dimensional labyrinth
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.dimensionalLabyrinth];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.skipDimensionalLabyrinth.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('skipDimensionalLabyrinth: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('skipDimensionalLabyrinth: go to dimensional labyrinth');
			const dimensionalLabyrinthResult = macroService.PollPattern(patterns.adventure.dungeon, { DoClick: true, PredicatePattern: [patterns.adventure.dungeon.dimensionalLabyrinth, patterns.adventure.dungeon.dimensionalLabyrinth.disabled] });
			if (dimensionalLabyrinthResult.PredicatePath === 'adventure.dungeon.dimensionalLabyrinth.disabled') {
				if (macroService.IsRunning) {
					daily.skipDimensionalLabyrinth.done.IsChecked = true;
				}
				return;
			}
			macroService.PollPattern(patterns.adventure.dungeon.dimensionalLabyrinth, { DoClick: true, ClickOffset: { Y: -100 }, PredicatePattern: patterns.titles.dimensionalLabyrinth });
			break;
		case 'titles.dimensionalLabyrinth':
			logger.info('skipDimensionalLabyrinth: skip decoy operation');
			macroService.PollPattern(patterns.adventure.dungeon.dimensionalLabyrinth.sweepAvailable, { DoClick: true, PredicatePattern: patterns.adventure.dungeon.dimensionalLabyrinth.sweep });
			macroService.PollPattern(patterns.adventure.dungeon.dimensionalLabyrinth.sweep, { DoClick: true, PredicatePattern: patterns.adventure.dungeon.dimensionalLabyrinth.sweep.confirm });
			macroService.PollPattern(patterns.adventure.dungeon.dimensionalLabyrinth.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.titles.dimensionalLabyrinth });

			if (macroService.IsRunning) {
				daily.skipDimensionalLabyrinth.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}