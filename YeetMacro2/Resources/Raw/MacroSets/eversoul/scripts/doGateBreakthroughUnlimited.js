let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.titles.gateBreakthrough, patterns.gateBreakthrough.challenge, patterns.gateBreakthrough.nextStage];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'lobby.everstone':
			logger.info('doGateBreakthrough unlimited: click adventure');
			await macroService.clickPattern(patterns.lobby.adventure);
			await sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doGateBreakthrough unlimited: click gate breakthrough');
			await macroService.pollPattern(patterns.gateBreakthrough, { doClick: true, clickOffsetY: -60, predicatePattern: patterns.titles.gateBreakthrough });
			await sleep(500);
			break;
		case 'titles.gateBreakthrough':
			logger.info('doGateBreakthrough unlimited: click unlimited gate');
			await macroService.pollPattern(patterns.gateBreakthrough.gates.unlimited, { doClick: true, predicatePattern: patterns.gateBreakthrough.challenge });
			await sleep(500);
			break;
		case 'gateBreakthrough.challenge':
			logger.info('doGateBreakthrough unlimited: click challenge');
			await macroService.pollPattern(patterns.gateBreakthrough.challenge, { doClick: true, predicatePattern: patterns.battle.start });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.prompt.close, predicatePattern: patterns.gateBreakthrough.nextStage });
			await sleep(500);
			break;
		case 'gateBreakthrough.nextStage':
			logger.info('doGateBreakthrough unlimited: click next stage');
			await macroService.pollPattern(patterns.gateBreakthrough.nextStage, { doClick: true, predicatePattern: patterns.battle.start });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.prompt.close, predicatePattern: patterns.gateBreakthrough.nextStage });
			await sleep(500);
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');