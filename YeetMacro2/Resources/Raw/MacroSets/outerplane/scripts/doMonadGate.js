// @tags=weeklies
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.monadGate.gateEntryDevice, patterns.monadGate.selectEntryRoute];


while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doMonadGate: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doMonadGate: click monad gate');
			macroService.ClickPattern(patterns.adventure.monadGate);
			sleep(500);
			break;
		case 'monadGate.gateEntryDevice':
			logger.info('doMonadGate: click gate entry device');
			macroService.ClickPattern(patterns.adventure.monadGate.gateEntryDevice);
			break;
		case 'monadGate.selectEntryRoute':
			logger.info('doMonadGate: click select entry route select');
			macroService.ClickPattern(patterns.adventure.monadGate.gateEntryDevice);
			break;
		case 'monadGate.heroDeployment':
			macroService.PollPattern([patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.ok });
			macroService.PollPattern(patterns.monadGate.heroDeployment.filter.element.light, { DoClick: true, PredicatePattern: patterns.monadGate.heroDeployment.filter.element.light.selected });
			macroService.PollPattern(patterns.battle.characterFilter.ok, { DoClick: true, PredicatePattern: [patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied] });

			const allCharacterCloneOpts = { X: 70, Y: 880, Width: 1700, Height: 130, PathSuffix: '_all', OffsetCalcType: 'None', BoundsCalcType: 'FillWidth' };
			const allCharacterPattern = macroService.ClonePattern(patterns.battle.character[character.name], allCharacterCloneOpts);

			break;

	}
	sleep(1_000);
}


