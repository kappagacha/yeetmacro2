// Do unilimted gate once
const targetGate = 'unlimited';
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.gateBreakthrough, patterns.gateBreakthrough.challenge, patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry];

const daily = dailyManager.GetCurrentDaily();
if (daily.doUnlimitedGateOnce.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.prompt.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info(`doUnlimitedGateOnce ${targetGate}: click adventure`);
			macroService.ClickPattern(patterns.lobby.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info(`doUnlimitedGateOnce ${targetGate}: click gate breakthrough`);
			macroService.PollPattern(patterns.gateBreakthrough, { DoClick: true, ClickOffset: { Y: -60 }, PredicatePattern: patterns.titles.gateBreakthrough });
			sleep(500);
			break;
		case 'titles.gateBreakthrough':
			logger.info(`doUnlimitedGateOnce ${targetGate}: click ${targetGate} gate`);
			macroService.PollPattern(patterns.gateBreakthrough.gates[targetGate], { DoClick: true, PredicatePattern: patterns.gateBreakthrough.challenge });
			sleep(500);
			break;
		case 'gateBreakthrough.challenge':
			logger.info(`doUnlimitedGateOnce ${targetGate}: click challenge`);
			macroService.PollPattern(patterns.gateBreakthrough.challenge, { DoClick: true, ClickPattern: patterns.gateBreakthrough.nextTeam, PredicatePattern: patterns.battle.start });
			sleep(500);
			macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.prompt.close, PredicatePattern: [patterns.gateBreakthrough.nextStage, patterns.gateBreakthrough.retry] });
			sleep(500);
			break;
		case 'gateBreakthrough.nextStage':
			logger.info(`doUnlimitedGateOnce ${targetGate}: success`);
			if (macroService.IsRunning) {
				daily.doUnlimitedGateOnce.done.IsChecked = true;
			}
			return;
		case 'gateBreakthrough.retry':
			logger.info(`doUnlimitedGateOnce ${targetGate}: fail`);
			if (macroService.IsRunning) {
				daily.doUnlimitedGateOnce.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}