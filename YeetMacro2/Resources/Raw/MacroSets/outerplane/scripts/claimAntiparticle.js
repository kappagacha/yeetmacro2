// @position=1
// Claim anti particle generator rewards
const loopPatterns = [patterns.lobby.level, patterns.titles.base];
const daily = dailyManager.GetDaily();

const isLastRunWithinHour = (Date.now() - settings.claimAntiparticle.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimAntiparticle.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimAntiparticle: click base tab');
			const recruitNotificationResult = macroService.PollPattern(patterns.tabs.base.notification, { TimeoutMs: 2_000 });
			if (recruitNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.base);
			} else {	// already claimed
				return;
			}
			sleep(500);
			break;
		case 'titles.base':
			logger.info('claimAntiparticle: claim antiparticle');
			const antiparticleResult = macroService.PollPattern(patterns.base.antiparticle, { DoClick: true, PredicatePattern: [patterns.base.antiparticle.receiveReward, patterns.base.antiparticle.receiveReward.disabled] });
			if (antiparticleResult.PredicatePath === 'base.antiparticle.receiveReward') {
				macroService.PollPattern(patterns.base.antiparticle.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.general.back });
			}
			
			if (macroService.IsRunning) {
				daily.claimAntiparticle.count.Count++;
				settings.claimAntiparticle.lastRun.Value = new Date().toISOString();
			}
			return;
	}
	sleep(1_000);
}