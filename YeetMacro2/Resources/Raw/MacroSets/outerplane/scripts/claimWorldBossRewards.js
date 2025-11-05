// @tags=favorites
// Claim world boss rewards
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.worldBoss];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimWorldBossRewards.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimWorldBossRewards: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('claimWorldBossRewards: click world boss');
			const worldBossResult = macroService.FindPattern([patterns.adventure.worldBoss, patterns.adventure.worldBoss.locked]);
			if (worldBossResult.Path === 'adventure.worldBoss.locked') return;

			const worldBossNotificationResult = macroService.PollPattern(patterns.adventure.worldBoss.notification, { TimeoutMs: 3_000 });
			if (worldBossNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.adventure.worldBoss);
			} else {	// already claimed
				return;
			}

			sleep(500);
			break;
		case 'titles.worldBoss':
			logger.info('claimWorldBossRewards: claim rewards');
			macroService.PollPattern(patterns.adventure.worldBoss.rewardNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.worldBoss });

			macroService.PollPattern(patterns.adventure.worldBoss.battleStart, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord });
			macroService.PollPattern(patterns.adventure.worldBoss.battleRecord, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord.restoreTeam });
			macroService.PollPattern(patterns.adventure.worldBoss.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord.restoreTeam.ok });
			macroService.PollPattern(patterns.adventure.worldBoss.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleRecord });
			macroService.PollPattern(patterns.adventure.worldBoss.enter, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.saveAndExit });
			macroService.PollPattern(patterns.adventure.worldBoss.saveAndExit, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.battleStart });

			macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification, { DoClick: true, PredicatePattern: patterns.adventure.worldBoss.challengeReward.notification2 });
			let notificationResult = { IsSuccess: true };
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification2, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.close });
				notificationResult = macroService.PollPattern(patterns.adventure.worldBoss.challengeReward.notification2, { TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) {
				daily.claimWorldBossRewards.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}