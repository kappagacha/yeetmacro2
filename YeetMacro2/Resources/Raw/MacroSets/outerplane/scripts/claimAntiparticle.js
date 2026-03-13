// @position=1
// Claim anti particle generator rewards
const popupPatterns = [patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.lobby.popup.doNotShowAgainToday];
const loopPatterns = [patterns.lobby.level, patterns.titles.base, ...popupPatterns];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.claimAntiparticle.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.claimAntiparticle.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'general.tapEmptySpace':
		case 'settings.goToLobby.userClickPattern':
		case 'general.exitCheckIn':
		case 'lobby.popup.doNotShowAgainToday':
			goToLobby();
			break;
		case 'lobby.level':
			logger.info('claimAntiparticle: click base tab');
			const claimRewardsResult = macroService.FindPattern(patterns.tabs.base.claimRewards);
			if (claimRewardsResult.IsSuccess) {
				macroService.PollPattern(patterns.tabs.base.claimRewards, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.receiveReward });

				const specialRewardNotificationResult = macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward.notification, { TimeoutMs: 2_000 });
				if (specialRewardNotificationResult.IsSuccess) {
					macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.specialReward.free });
					macroService.PollPattern(patterns.tabs.base.claimRewards.specialReward.free, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.tabs.base.claimRewards.receiveReward });
				}

				macroService.PollPattern(patterns.tabs.base.claimRewards.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
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
			const antiParticleResult = macroService.PollPattern(patterns.base.antiparticle, { DoClick: true, PredicatePattern: [patterns.base.antiparticle.receiveReward, patterns.base.antiparticle.receiveReward.disabled] });

			const baseSpecialRewardNotificationResult = macroService.PollPattern(patterns.base.antiparticle.specialReward.notification, { TimeoutMs: 2_000 });
			if (baseSpecialRewardNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.base.antiparticle.specialReward, { DoClick: true, PredicatePattern: patterns.base.antiparticle.specialReward.free });
				macroService.PollPattern(patterns.base.antiparticle.specialReward.free, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.antiparticle.receiveReward, patterns.base.antiparticle.receiveReward.disabled] });
			}

			if (antiParticleResult.PredicatePath === 'base.antiparticle.receiveReward.disabled') {
				return;
			}

			macroService.PollPattern(patterns.base.antiparticle.receiveReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.base.antiparticle.receiveReward.disabled });

			if (macroService.IsRunning) {
				daily.claimAntiparticle.count.Count++;
				settings.claimAntiparticle.lastRun.Value = new Date().toISOString();
			}
			return;
	}
	sleep(1_000);
}