let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.titles.gateBreakthrough, patterns.gateBreakthrough.challenge, patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry];
while (macroService.IsRunning && !done) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.everstone':
			logger.info('doGateBreakthrough beast: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doGateBreakthrough beast: click gate breakthrough');
			macroService.PollPattern(patterns.gateBreakthrough, { DoClick: true, Offset: { Y: -60 }, PredicatePattern: patterns.titles.gateBreakthrough });
			sleep(500);
			break;
		case 'titles.gateBreakthrough':
			logger.info('doGateBreakthrough beast: click beast gate');
			macroService.PollPattern(patterns.gateBreakthrough.gates.beast, { DoClick: true, PredicatePattern: patterns.gateBreakthrough.challenge });
			sleep(500);
			break;
		case 'gateBreakthrough.challenge':
			logger.info('doGateBreakthrough beast: click challenge');
			macroService.PollPattern(patterns.gateBreakthrough.challenge, { DoClick: true, PredicatePattern: patterns.battle.start });
			sleep(500);
			macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.prompt.close, PredicatePattern: [patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry] });
			sleep(500);
			break;
		case 'gateBreakthrough.nextStage':
			logger.info('doGateBreakthrough beast: click next stage');
			macroService.PollPattern(patterns.gateBreakthrough.nextStage, { DoClick: true, PredicatePattern: patterns.battle.start });
			sleep(500);
			macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.prompt.close, PredicatePattern: patterns.gateBreakthrough.nextStage });
			sleep(500);
			break;
		case 'gateBreakthrough.retry':
			logger.info('doGateBreakthrough beast: retry');
			done = true;
			result = 'Party defeated';
			break;
	}

	sleep(1_000);
}
logger.info('Done...');