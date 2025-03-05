// @position=1
// Claim anti particle generator rewards
const popupPatterns = [patterns.lobby.expedition, patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.general.startMessageClose];
const loopPatterns = [patterns.lobby.level, patterns.titles.base, ...popupPatterns];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.claimAntiparticle.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimAntiparticle.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.expedition':
		case 'general.tapEmptySpace':
		case 'settings.goToLobby.userClickPattern':
		case 'general.exitCheckIn':
		case 'general.startMessageClose':
			goToLobby();
			break;
		case 'lobby.level':
			logger.info('claimAntiparticle: click base tab');

			const obtainAntiParticleResult = macroService.PollPattern(patterns.lobby.obtainAntiParticle, { DoClick: true, PredicatePattern: patterns.lobby.obtainAntiParticle.receiveReward, TimeoutMs: 3_000 });
			if (obtainAntiParticleResult.IsSuccess) {
				macroService.PollPattern(patterns.lobby.obtainAntiParticle.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.lobby.level });

				if (macroService.IsRunning) {
					daily.claimAntiparticle.count.Count++;
					settings.claimAntiparticle.lastRun.Value = new Date().toISOString();
				}

				return;
			}

			//const baseNotificationResult = macroService.PollPattern(patterns.tabs.base.notification, { TimeoutMs: 2_000 });
			//if (baseNotificationResult.IsSuccess) {
			//	macroService.ClickPattern(patterns.tabs.base);
			//} else {	// already claimed
			//	return;
			//}
			macroService.ClickPattern(patterns.tabs.base);
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