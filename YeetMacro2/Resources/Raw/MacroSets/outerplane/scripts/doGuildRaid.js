// @position=11
// Auto guild raid
const loopPatterns = [patterns.lobby.level, patterns.titles.guildBoard, patterns.titles.guildRaid, patterns.titles.guild];
const daily = dailyManager.GetCurrentDaily();
const teamSlot1 = settings.doGuildRaid.teamSlot1.Value;
const targetStage1 = settings.doGuildRaid.targetStage1.Value;
const teamSlot2 = settings.doGuildRaid.teamSlot2.Value;
const targetStage2 = settings.doGuildRaid.targetStage2.Value;

if (daily.doGuildRaid.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.general.tapEmptySpace });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doGuildRaid: click guild tab');
			const guildNotificationResult = macroService.PollPattern(patterns.tabs.guild.notification, { TimeoutMs: 1_500 });
			if (guildNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.guild);
			} else {	// no notification
				return;
			}
			sleep(500);
			break;
		case 'titles.guild':
			logger.info('doGuildRaid: click board');
			macroService.ClickPattern(patterns.guild.board);
			sleep(500);
			break;
		case 'titles.guildBoard':
			logger.info('doGuildRaid: click raid move');
			macroService.ClickPattern(patterns.guild.raid.move);
			break;
		case 'titles.guildRaid':
			macroService.PollPattern(patterns.guild.raid.stageRight);
			//const stageRightResult = macroService.PollPattern(patterns.guild.raid.stageRight, { TimeoutMs: 5_000 });
			//if (!stageRightResult.IsSuccess) {	// guild raid is not live
			//	if (macroService.IsRunning) {
			//		daily.doGuildRaid.done.IsChecked = true;
			//	}
			//	return;
			//}

			logger.info(`doGuildRaid: target stage first team`);
			macroService.PollPattern(patterns.guild.raid[`stage${targetStage1}`], { ClickPattern: patterns.guild.raid.stageRight });
			macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam(teamSlot1);
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, ClickPattern: [patterns.guild.raid.enterBattle, patterns.battle.setup.auto], PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, ClickPattern: [patterns.guild.raid.battleRecordExit, patterns.general.tapEmptySpace], PredicatePattern: patterns.titles.guildRaid });
			sleep(1000);

			logger.info(`doGuildRaid: target stage second team`);
			macroService.PollPattern(patterns.guild.raid[`stage${targetStage2}`], { ClickPattern: patterns.guild.raid.stageRight });
			macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam(teamSlot2);
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, ClickPattern: [patterns.guild.raid.enterBattle, patterns.battle.setup.auto], PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, ClickPattern: [patterns.guild.raid.battleRecordExit, patterns.general.tapEmptySpace], PredicatePattern: patterns.titles.guildRaid });

			const raidRewardNotificationResult = macroService.PollPattern(patterns.guild.raid.reward.notification, { TimeoutMs: 1_500 });
			if (raidRewardNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.guild.raid.reward.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.guildRaid });
			}

			if (macroService.IsRunning) {
				daily.doGuildRaid.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}