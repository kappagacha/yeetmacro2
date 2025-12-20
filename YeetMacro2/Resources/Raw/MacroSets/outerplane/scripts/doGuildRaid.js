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
			if (!daily.doGuildRaid.battle1.IsChecked) {
				macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.battle.enter });
				selectTeam(teamSlot);
				macroService.PollPattern(patterns.guild.raid.battleRecord, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam });
				macroService.PollPattern(patterns.guild.raid.battleRecord.record1, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.record1.selected });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord });
				macroService.PollPattern(patterns.battle.teamFormation, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.selected });
				setChainOrder();
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.guild.raid.enterBattle });
				macroService.PollPattern(patterns.guild.raid.enterBattle, { DoClick: true, PredicatePattern: patterns.guild.raid.exitBattle });
				macroService.PollPattern(patterns.guild.raid.exitBattle, { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.titles.guildRaid });
				macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
				
				if (macroService.IsRunning) daily.doGuildRaid.battle1.IsChecked = true;
			}

			if (!daily.doGuildRaid.battle2.IsChecked) {
				macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.battle.enter });
				selectTeam(teamSlot);
				macroService.PollPattern(patterns.guild.raid.battleRecord, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam });
				macroService.PollPattern(patterns.guild.raid.battleRecord.record2, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.record2.selected });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.guild.raid.battleRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.guild.raid.battleRecord });
				macroService.PollPattern(patterns.battle.teamFormation, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.selected });
				setChainOrder();
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.guild.raid.enterBattle });
				macroService.PollPattern(patterns.guild.raid.enterBattle, { DoClick: true, PredicatePattern: patterns.guild.raid.exitBattle });
				macroService.PollPattern(patterns.guild.raid.exitBattle, { DoClick: true, ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.raid.ok], PredicatePattern: patterns.titles.guildRaid });
				macroService.PollPattern(patterns.guild.raid.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });

				if (macroService.IsRunning) daily.doGuildRaid.battle2.IsChecked = true;
			}

			if (macroService.IsRunning) daily.doGuildRaid.done.IsChecked = true;
			return;
	}
	sleep(1_000);
}