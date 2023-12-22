// Auto guild security area
const loopPatterns = [patterns.lobby.level, patterns.titles.guildHallOfHonor, patterns.titles.guild];
const daily = dailyManager.GetDaily();

if (daily.claimGuildBuff.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.checkIn.ok, patterns.guild.raid.startMessage] });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimGuildBuff: click guild tab');
			const guildNotificationResult = macroService.PollPattern(patterns.tabs.guild.notification, { TimoutMs: 2_000 });
			if (guildNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.guild);
			} else {	// no notification
				return;
			}
			sleep(500);
			break;
		case 'titles.guild':
			logger.info('claimGuildBuff: click hall of honor');
			macroService.ClickPattern(patterns.guild.hallOfHonor);
			sleep(500);
			break;
		case 'titles.guildHallOfHonor':
			logger.info('claimGuildBuff: click receive guild buff');
			macroService.PollPattern(patterns.guild.hallOfHonor.receiveGuildBuff, { DoClick: true, PredicatePattern: patterns.guild.hallOfHonor.alreadyReceived });
			if (macroService.IsRunning) {
				daily.claimGuildBuff.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}