// @position=0
// Claim arena rewards
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
//const resolution = macroService.GetCurrentResolution();

if (daily.claimArenaRewards.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimArenaRewards: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimArenaRewards: claim rewards');
			macroService.PollPattern(patterns.event.arenaReward, { SwipePattern: patterns.event.swipeDown });
			macroService.PollPattern(patterns.event.arenaReward, { DoClick: true, PredicatePattern: patterns.event.arenaReward.utc });

			let notificationResult = macroService.PollPattern(patterns.event.arenaReward.notification, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.arenaReward.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.arenaReward.utc });
				notificationResult = macroService.PollPattern(patterns.event.arenaReward.notification, { TimeoutMs: 3_000 });
			}

			const claimedAllResult = macroService.FindPattern(patterns.event.arenaReward.claimedAll);

			if (claimedAllResult.IsSuccess && macroService.IsRunning) {
				daily.claimArenaRewards.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}