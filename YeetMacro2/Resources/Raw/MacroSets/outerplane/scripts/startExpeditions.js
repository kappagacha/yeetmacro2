// Start expeditions
const loopPatterns = [patterns.lobby.level, patterns.titles.base, patterns.titles.expeditionTeam];
const expeditions = ["goldenTemple", "forgeOfGlory", "abandonedFactoryOfRevolution", "blackMarket"];

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('startExpeditions: click base');
			macroService.ClickPattern(patterns.tabs.base);
			sleep(500);
			break;
		case 'titles.base':
			logger.info('startExpeditions: click expedition team');
			macroService.ClickPattern(patterns.base.expeditionTeam);
			sleep(500);
			break;
		case 'titles.expeditionTeam':
			logger.info('startExpeditions: start expedition');
			for (let expedition of expeditions) {
				macroService.PollPattern(patterns.base.expeditionTeam[expedition], { DoClick: true, PredicatePattern: patterns.base.expeditionTeam.autoFormation });
				macroService.PollPattern(patterns.base.expeditionTeam.autoFormation, { DoClick: true, PredicatePattern: patterns.base.expeditionTeam.search });
				macroService.PollPattern(patterns.base.expeditionTeam.search, { DoClick: true, PredicatePattern: patterns.titles.expeditionTeam });
			}
			return;
	}
	sleep(1_000);
}
