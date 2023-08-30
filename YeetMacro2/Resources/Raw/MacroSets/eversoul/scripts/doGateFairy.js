let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.titles.gateBreakthrough, patterns.gateBreakthrough.challenge, patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry];
while (state.isRunning && !done) {
	const loopResult = macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'lobby.everstone':
			logger.info('doGateBreakthrough fairy: click adventure');
			macroService.clickPattern(patterns.lobby.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doGateBreakthrough fairy: click gate breakthrough');
			macroService.pollPattern(patterns.gateBreakthrough, { doClick: true, clickOffsetY: -60, predicatePattern: patterns.titles.gateBreakthrough });
			sleep(500);
			break;
		case 'titles.gateBreakthrough':
			logger.info('doGateBreakthrough fairy: click fairy gate');
			macroService.pollPattern(patterns.gateBreakthrough.gates.fairy, { doClick: true, predicatePattern: patterns.gateBreakthrough.challenge });
			sleep(500);
			break;
		case 'gateBreakthrough.challenge':
			logger.info('doGateBreakthrough fairy: click challenge');
			macroService.pollPattern(patterns.gateBreakthrough.challenge, { doClick: true, predicatePattern: patterns.battle.start });
			sleep(500);
			macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.prompt.close, predicatePattern: [patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry] });
			sleep(500);
			break;
		case 'gateBreakthrough.nextStage':
			logger.info('doGateBreakthrough fairy: click next stage');
			macroService.pollPattern(patterns.gateBreakthrough.nextStage, { doClick: true, predicatePattern: patterns.battle.start });
			sleep(500);
			macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.prompt.close, predicatePattern: patterns.gateBreakthrough.nextStage });
			sleep(500);
			break;
		case 'gateBreakthrough.retry':
			logger.info('doGateBreakthrough fairy: retry');
			done = true;
			result = 'Party defeated';
			break;
	}

	sleep(1_000);
}
logger.info('Done...');