// @position=1
// Do skyward tower
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge, patterns.titles.skywardTower];
const teamSlot = settings.doSkywardTower.teamSlot.Value;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doSkywardTower: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doSkywardTower: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			logger.info('doSkywardTower: click skywardTower');
			macroService.ClickPattern(patterns.challenge.skywardTower);
			sleep(500);
			break;
		case 'titles.skywardTower':
			logger.info('doSkywardTower: do skyward tower');
			macroService.PollPattern(patterns.challenge.skywardTower.normal.disabled, { DoClick: true, PredicatePattern: patterns.challenge.skywardTower.normal.enabled });
			macroService.PollPattern(patterns.battle.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam(teamSlot, { applyPreset: true });
			macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.enter });
			macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.consecutiveBattles.ok });
			return;
	}
	sleep(1_000);
}