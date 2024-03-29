// @position=1
// Claim loot
const loopPatterns = [patterns.lobby.level, patterns.titles.loot];
const daily = dailyManager.GetDaily();

const isLastRunWithinHour = (Date.now() - settings.claimLoot.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimLoot.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimLoot: click loot');
			macroService.PollPattern(patterns.lobby.loot, { DoClick: true, ClickOffset: { Y: 60 }, PredicatePattern: patterns.general.back });
			macroService.PollPattern(patterns.battleFront.loot, { DoClick: true, PredicatePattern: patterns.titles.loot });
			break;
		case 'titles.loot':
			logger.info('claimLoot: receiveAll and quickHunt');
			macroService.PollPattern(patterns.loot.receiveAll, { DoClick: true, ClickPattern: [patterns.general.tapTheScreen, patterns.loot.tapTheScreen], PredicatePattern: patterns.loot.receiveAll.disabled });

			const notificationResult = macroService.PollPattern(patterns.loot.quickHunt.notification, { TimeoutMs: 1_500 });
			if (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.loot.quickHunt.notification, { DoClick: true, PredicatePattern: patterns.loot.quickHunt.free });
				macroService.PollPattern(patterns.loot.quickHunt.free, { DoClick: true, PredicatePattern: patterns.loot.tapTheScreen });
				macroService.PollPattern(patterns.loot.tapTheScreen, { DoClick: true, ClickPattern: patterns.loot.tapTheScreen, PredicatePattern: patterns.loot.quickHunt.cancel });
				macroService.PollPattern(patterns.loot.quickHunt.cancel, { DoClick: true, PredicatePattern: patterns.titles.loot });
			}

			logger.info('claimLoot: done');
			if (macroService.IsRunning) {
				daily.claimLoot.count.Count++;
				settings.claimLoot.lastRun.Value = new Date().toISOString();
			}

			return;
	}

	sleep(1_000);
}