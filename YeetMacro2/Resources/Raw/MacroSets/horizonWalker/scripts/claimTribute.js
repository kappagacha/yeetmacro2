// Claim tribute
const loopPatterns = [patterns.lobby.stage, patterns.tribute.receive];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.claimTribute.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimTribute.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('claimTribute: click tribute');
			macroService.ClickPattern(patterns.lobby.tribute);
			break;
		case 'tribute.receive':
			logger.info('claimTribute: claim tribute');
			//const receiveResult = macroService.PollPattern(patterns.tribute.receive, { DoClick: true, PredicatePattern: [patterns.general.touchTheScreen, patterns.general.confirm] });
			//if (receiveResult.PredicatePath === 'general.confirm') {
			//	macroService.PollPattern(patterns.general.confirm, { DoClick: true, PredicatePattern: patterns.tribute.receive });
			//	if (macroService.IsRunning) {
			//		daily.claimTribute.count.Count++;
			//		settings.claimTribute.lastRun.Value = new Date().toISOString();
			//	}
			//	return;
			//}

			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, ClickPattern: [patterns.general.levelUp, patterns.general.confirm], PredicatePattern: patterns.tribute.receive });

			if (macroService.IsRunning) {
				daily.claimTribute.count.Count++;
				settings.claimTribute.lastRun.Value = new Date().toISOString();
			}
			return;
	}
	sleep(1_000);
}