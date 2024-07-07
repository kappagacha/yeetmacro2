// Claim world boss rewards
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.worldBoss];
const daily = dailyManager.GetCurrentDaily();

if (daily.claimWorldBossRewards.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimWorldBossRewards: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('claimWorldBossRewards: click world boss');
			macroService.ClickPattern(patterns.adventure.worldBoss);
			sleep(500);
			break;
		case 'titles.worldBoss':
			logger.info('claimWorldBossRewards: claim rewards');
			macroService.PollPattern(patterns.adventure.worldBoss.rewardNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.worldBoss });

			if (macroService.IsRunning) {
				daily.claimWorldBossRewards.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}