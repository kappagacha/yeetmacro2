// @position=10
const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.bossRaid, patterns.stonehenge.select];
const daily = dailyManager.GetCurrentDaily();
if (daily.doStonehenge.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doStonehenge: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doStonehenge: click stonehenge');
			macroService.PollPattern(patterns.battle.dungeon, { DoClick: true, PredicatePattern: patterns.battle.selected });
			macroService.PollPattern(patterns.stonehenge, { DoClick: true, PredicatePattern: patterns.stonehenge.select });
			break;
		case 'stonehenge.select':
			macroService.PollPattern(patterns.stonehenge.select, { DoClick: true, PredicatePattern: patterns.stonehenge.start });
			macroService.PollPattern(patterns.stonehenge.start, { DoClick: true, PredicatePattern: patterns.stonehenge.summonBossAutoUse });
			macroService.PollPattern(patterns.stonehenge.summonBossAutoUse, { DoClick: true, InversePredicatePattern: patterns.stonehenge.summonBossAutoUse });
			sleep(5_000);
			macroService.PollPattern(patterns.stonehenge.summonBossAutoUse.checked);
			macroService.PollPattern(patterns.stonehenge.summonBossAutoUse.checked, { DoClick: true, InversePredicatePattern: patterns.stonehenge.summonBossAutoUse.checked });
			macroService.PollPattern(patterns.menu, { DoClick: true, PredicatePattern: patterns.menu.close });
			macroService.PollPattern(patterns.menu.return, { DoClick: true, PredicatePattern: patterns.lobby });

			if (macroService.IsRunning) daily.doStonehenge.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}