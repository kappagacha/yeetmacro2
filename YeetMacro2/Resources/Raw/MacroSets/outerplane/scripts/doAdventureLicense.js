// @position=8
// Auto or sweep bandit chase
const loopPatterns = [patterns.lobby.level, patterns.adventure.adventureLicense.title, patterns.adventure.adventureLicense.weeklyConquest.selected, patterns.titles.adventure];


while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doAdventureLicense: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doAdventureLicense: click adventure license');
			macroService.ClickPattern(patterns.adventure.adventureLicense);
			sleep(500);
			break;
		case 'adventure.adventureLicense.title':
		case 'adventure.adventureLicense.weeklyConquest.selected':
			logger.info('doAdventureLicense: do weekly conquest');
			macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.selected });

			const wantedResults = macroService.FindPattern(patterns.adventure.adventureLicense.weeklyConquest.wanted, { Limit: 4 });
			let targetWanted;
			for (const p of wantedResults.Points) {
				const completedPattern = macroService.ClonePattern(patterns.adventure.adventureLicense.weeklyConquest.completed,
					{ CenterX: p.X - 10, CenterY: p.Y - 130, Height: 120, Width: 270, PathSuffix: `_${p.X}x` });
				if (!macroService.FindPattern(completedPattern).IsSuccess) {
					targetWanted = p;
					break;
				}
			}

			if (!targetWanted) {
				throw Error('Could not find wanted that is not complete');
			}
			macroService.PollPoint(targetWanted, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.heroDeployment });
			macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam2();
			macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.next });
			macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.next, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.exit });
			macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.exit, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.selected });
			return;
	}
	sleep(1_000);
}