// @raw-script
function doGate(type) {
	const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.titles.gateBreakthrough, patterns.gateBreakthrough.challenge, patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry];
	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.everstone':
				logger.info(`doGateBreakthrough ${type}: click adventure`);
				macroService.ClickPattern(patterns.lobby.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info(`doGateBreakthrough ${type}: click gate breakthrough`);
				macroService.PollPattern(patterns.gateBreakthrough, { DoClick: true, ClickOffset: { Y: -60 }, PredicatePattern: patterns.titles.gateBreakthrough });
				sleep(500);
				break;
			case 'titles.gateBreakthrough':
				logger.info(`doGateBreakthrough ${type}: click ${type} gate`);
				macroService.PollPattern(patterns.gateBreakthrough.gates[type], { DoClick: true, PredicatePattern: patterns.gateBreakthrough.challenge });
				sleep(500);
				break;
			case 'gateBreakthrough.challenge':
				logger.info(`doGateBreakthrough ${type}: click challenge`);
				macroService.PollPattern(patterns.gateBreakthrough.challenge, { DoClick: true, PredicatePattern: patterns.battle.start });
				sleep(500);
				macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.prompt.close, PredicatePattern: [patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry] });
				sleep(500);
				break;
			case 'gateBreakthrough.nextStage':
				logger.info(`doGateBreakthrough ${type}: click next stage`);
				macroService.PollPattern(patterns.gateBreakthrough.nextStage, { DoClick: true, PredicatePattern: patterns.battle.start });
				sleep(500);
				macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.prompt.close, PredicatePattern: patterns.gateBreakthrough.nextStage });
				sleep(500);
				break;
			case 'gateBreakthrough.retry':
				logger.info(`doGateBreakthrough ${type}: retry`);
				return 'Party defeated';
		}

		sleep(1_000);
	}
}

