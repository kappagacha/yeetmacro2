// @position=1
// Skip dungeon
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.dungeon];
const daily = dailyManager.GetCurrentDaily();
const targetDungeon = settings.skipDungeon.targetDungeon.Value;
const targetDungeonPattern = patterns.dungeon[targetDungeon];

if (daily.skipDungeon.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('skipDungeon: click quest tab');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('skipDungeon: click dungeon');
			macroService.ClickPattern(patterns.quest.dungeon);
			break;
		case 'titles.dungeon':
			logger.info('skipDungeon: skip dungeon');
			macroService.SwipePollPattern(targetDungeonPattern, { Start: { X: 1000, Y: 800 }, End: { X: 1000, Y: 400 } });
			macroService.PollPattern(targetDungeonPattern, { DoClick: true, PredicatePattern: patterns.dungeon.advanced });
			macroService.PollPattern(patterns.dungeon.advanced, { DoClick: true, PredicatePattern: patterns.dungeon.skip });
			macroService.PollPattern(patterns.dungeon.skip, { DoClick: true, PredicatePattern: patterns.dungeon.skip.skip });
			macroService.PollPattern(patterns.dungeon.skip.skip, { DoClick: true, PredicatePattern: patterns.dungeon.skip.ok });
			macroService.PollPattern(patterns.dungeon.skip.ok, { DoClick: true, PredicatePattern: patterns.dungeon.skip });
			
			if (macroService.IsRunning) {
				daily.skipDungeon.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}