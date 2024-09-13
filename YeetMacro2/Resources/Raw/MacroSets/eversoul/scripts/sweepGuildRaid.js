// @position=11
// Skip decoy operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepGuildRaid.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepGuildRaid: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('sweepGuildRaid: click raid');
			const guildRaidResult = macroService.PollPattern(patterns.adventure.raid, { DoClick: true, PredicatePattern: [patterns.adventure.raid.guildRaid, patterns.adventure.raid.guildRaid.disabled] });
			if (guildRaidResult.PredicatePath === 'adventure.raid.guildRaid.disabled' && macroService.IsRunning) {
				daily.sweepGuildRaid.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.adventure.raid.guildRaid, { DoClick: true, PredicatePattern: patterns.guildRaid.sweep });
			macroService.PollPattern(patterns.guildRaid.sweep, { DoClick: true, PredicatePattern: [patterns.guildRaid.sweep.confirm, patterns.guildRaid.sweep.disabled] });
			macroService.PollPattern(patterns.guildRaid.sweep.confirm, { DoClick: true, PredicatePattern: patterns.guildRaid.sweep.disabled });

			if (macroService.IsRunning) {
				daily.sweepGuildRaid.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}