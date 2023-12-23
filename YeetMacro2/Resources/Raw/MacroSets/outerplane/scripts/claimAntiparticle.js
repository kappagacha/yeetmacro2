// Claim anti particle generator rewards
const loopPatterns = [patterns.lobby.level, patterns.titles.base];
const daily = dailyManager.GetDaily();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimAntiparticle: click base tab');
			const recruitNotificationResult = macroService.PollPattern(patterns.tabs.base.notification, { TimoutMs: 2_000 });
			if (recruitNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.base);
			} else {	// already claimed
				return;
			}
			sleep(500);
			break;
		case 'titles.base':
			logger.info('claimAntiparticle: claim antiparticle');

			macroService.PollPattern(patterns.base.antiparticle, { PredicatePattern: patterns.base.antiparticle.receiveReward });
			macroService.PollPattern(patterns.base.antiparticle.receiveReward, { PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { PredicatePattern: patterns.general.back });

			if (macroService.IsRunning) {
				daily.claimAntiparticle.count.Count++;
			}
			return;
	}
	sleep(1_000);
}