// @position=14
// Skip all stage13s in special requests
const loopPatterns = [patterns.lobby.level, patterns.challenge.sweepAll.title];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepAll.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepAll: go to sweepAll');
			macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.lobby.menu.sweepAll });
			macroService.PollPattern(patterns.lobby.menu.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
			break;
		case 'challenge.sweepAll.title':
			if (!daily.sweepAll.resourceDungeon.IsChecked) {
				logger.info('sweepAll: sweep resource dungeon');
				macroService.PollPattern(patterns.challenge.sweepAll.resourceDungeon, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.resourceDungeon.selected });
				// earth stone => Sunday, Thursday, Tuesday, Saturday
				// water stone => Sunday, Monday, Thursday, Saturday
				// fire stone => Sunday, Monday, Wednesday, Saturday
				// light stone => Sunday, Tuesday, Friday, Saturday
				// dark stone => Sunday, Wednesday, Friday, Saturday
				// 0 - Sunday, 1 - Monday, 2 - Teusday, 3 - Wednesday, 4 - Thursday, 5 - Friday, 6 - Saturday
				const dayOfWeek = weeklyManager.GetDayOfWeek();
				const dayOfWeekStones = {
					0: ["earth", "water", "fire", "light", "dark"],    // Sunday
					1: ["water", "fire"],                            // Monday
					2: ["earth", "light"],                            // Tuesday
					3: ["fire", "dark"],                              // Wednesday
					4: ["earth", "water"],                            // Thursday
					5: ["light", "dark"],                             // Friday
					6: ["earth", "water", "fire", "light", "dark"]    // Saturday
				};
				const availableStones = dayOfWeekStones[dayOfWeek];
				const elementTypeTarget1 = settings.sweepAll.elementTypeTarget1.Value;
				const elementTypeTarget2 = settings.sweepAll.elementTypeTarget2.Value;
				const elementTypeTarget3 = settings.sweepAll.elementTypeTarget3.Value;
				const elementTypeTarget4 = settings.sweepAll.elementTypeTarget4.Value;
				const elementPriorities = [elementTypeTarget1, elementTypeTarget2, elementTypeTarget3, elementTypeTarget4];
				const targetElement = elementPriorities.find(target => target && availableStones.includes(target));
				if (!targetElement) {
					throw Error(`Could not find priority elements [${elementPriorities}] from available stones: [${availableStones}]`)
				}
				macroService.PollPattern(patterns.challenge.sweepAll.resourceDungeon.upgradeStone[targetElement], { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.resourceDungeon.upgradeStone[targetElement].selected });
				macroService.PollPattern(patterns.challenge.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep.ok });
				macroService.PollPattern(patterns.challenge.sweepAll.sweep.ok, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep });

				if (macroService.IsRunning) daily.sweepAll.resourceDungeon.IsChecked = true;
			}
			
			if (!daily.sweepAll.storyHard.IsChecked) {
				logger.info('sweepAll: sweep story hard');
				macroService.PollPattern(patterns.challenge.sweepAll.storyHard, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.storyHard.selected });

				macroService.PollPattern(patterns.challenge.sweepAll.sweep, { DoClick: true, PredicatePattern: [patterns.challenge.sweepAll.sweep.ok, patterns.challenge.sweepAll.storyHard.dungeonPrompt] });
				macroService.PollPattern(patterns.challenge.sweepAll.sweep.ok, { DoClick: true, PredicatePattern: [patterns.challenge.sweepAll.sweep, patterns.challenge.sweepAll.storyHard.dungeonPrompt] });
				if (macroService.IsRunning) daily.sweepAll.storyHard.IsChecked = true;
			}

			if (macroService.IsRunning) daily.sweepAll.done.IsChecked = daily.sweepAll.storyHard.IsChecked && daily.sweepAll.resourceDungeon.IsChecked;
			return;
	}
	sleep(1_000);
}
