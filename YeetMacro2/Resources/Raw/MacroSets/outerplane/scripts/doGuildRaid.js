// @position=11
// @tags=favorites
// Auto guild raid
const loopPatterns = [patterns.lobby.level, patterns.titles.guildBoard, patterns.titles.guildRaid, patterns.titles.guild];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doGuildRaid.teamSlot.Value;

if (daily.doGuildRaid.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok] });
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
			const selectTeamResult = macroService.PollPattern([patterns.guild.raid.selectTeam, patterns.guild.raid.selectTeam1, patterns.guild.raid.selectTeam2]);
			const isPhase1 = selectTeamResult.Path === 'guild.raid.selectTeam1' || selectTeamResult.Path === 'guild.raid.selectTeam2';

			for (const battleNum of [1, 2]) {
				const battleKey = `battle${battleNum}`;
				const recordKey = isPhase1 ? 'record1' : `record${battleNum}`;
				const selectKey = isPhase1 ? `selectTeam${battleNum}` : 'selectTeam';
				if (daily.doGuildRaid[battleKey].IsChecked) continue;

				logger.info(`doGuildRaid: battle ${battleNum}`);
				macroService.PollPattern(patterns.guild.raid[selectKey], { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.battle.enter });
				selectTeam(teamSlot);
				macroService.PollPattern(patterns.guild.raid.battleRecord, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam });
				macroService.PollPattern(patterns.guild.raid.battleRecord[recordKey], { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord[recordKey].selected });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord });
				macroService.PollPattern(patterns.battle.teamFormation, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.selected });
				setChainOrder();
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, ClickPattern: patterns.guild.raid.enterBattle, PredicatePattern: patterns.guild.raid.exitBattle });
				macroService.PollPattern(patterns.guild.raid.exitBattle, { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.titles.guildRaid });
				//macroService.PollPattern(patterns.guild.raid[selectKey], { DoClick: true, PredicatePattern: patterns.battle.enter });

				if (macroService.IsRunning) daily.doGuildRaid[battleKey].IsChecked = true;
			}

			if (macroService.IsRunning) daily.doGuildRaid.done.IsChecked = true;
			return;
	}
	sleep(1_000);
}