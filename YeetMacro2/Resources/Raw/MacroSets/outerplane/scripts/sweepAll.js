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
			if (!daily.sweepAll.arkRaid.IsChecked) {
				logger.info('sweepAll: sweep resource dungeon');
				macroService.PollPattern(patterns.challenge.sweepAll.resourceDungeon, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.resourceDungeon.selected });
				const arkRaidTarget = settings.sweepAll.arkRaidTarget.Value;
				macroService.PollPattern(patterns.challenge.sweepAll.resourceDungeon.arkRaid[arkRaidTarget], { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.resourceDungeon.arkRaid[arkRaidTarget].selected });
				macroService.PollPattern(patterns.challenge.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep.ok });
				macroService.PollPattern(patterns.challenge.sweepAll.sweep.ok, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep });

				if (macroService.IsRunning) daily.sweepAll.arkRaid.IsChecked = true;
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
