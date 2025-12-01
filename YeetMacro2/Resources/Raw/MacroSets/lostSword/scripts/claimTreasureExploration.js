// @position=1
const loopPatterns = [patterns.lobby, patterns.treasureExploration.title];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.claimTreasureExploration.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimTreasureExploration.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			macroService.ClickPattern(patterns.treasureExploration);
			break;
		case 'treasureExploration.title':
			logger.info('claimTreasureExploration: do quick expedition');

			const quickExpeditionNotificationResult = macroService.FindPattern(patterns.treasureExploration.quickExpedition.notification);
			if (quickExpeditionNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.treasureExploration.quickExpedition, { DoClick: true, PredicatePattern: [patterns.treasureExploration.quickExpedition.title, patterns.lobby], Callback: handlePopups });
				macroService.PollPattern(patterns.treasureExploration.quickExpedition.goOnExpedition, { DoClick: true, ClickPattern: patterns.general.itemsAcquired, PredicatePattern: [patterns.treasureExploration.quickExpedition, patterns.lobby] });
			}

			logger.info('claimTreasureExploration: acquireTreasure');
			macroService.PollPattern(patterns.treasureExploration.acquireTreasure, { DoClick: true, ClickPattern: patterns.general.itemsAcquired, PredicatePattern: patterns.lobby });

			if (macroService.IsRunning) {
				daily.claimTreasureExploration.count.Count++;
				settings.claimTreasureExploration.lastRun.Value = new Date().toISOString();
			}
			return;

	}
	sleep(1_000);
}