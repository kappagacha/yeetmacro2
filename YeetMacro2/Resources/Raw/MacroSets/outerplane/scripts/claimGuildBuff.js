// @position=5
// Claims guild buff
const loopPatterns = [patterns.lobby.level, patterns.titles.guildHallOfHonor, patterns.titles.guild];

const daily = dailyManager.GetCurrentDaily();

if (daily.claimGuildBuff.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.general.tapEmptySpace, patterns.guild.checkIn.ok, patterns.guild.raid.startMessage, patterns.guild.raid.endMessage.ok] });
	switch (loopResult.Path) {
		case 'lobby.level':
			//const receiveGuildBuffResult = macroService.PollPattern(patterns.lobby.receiveGuildBuff, { DoClick: true, PredicatePattern: patterns.lobby.receiveGuildBuff.message, TimeoutMs: 3_000 });
			//if (receiveGuildBuffResult.IsSuccess) {
			//	macroService.PollPattern(patterns.lobby.receiveGuildBuff.message, { DoClick: true, PredicatePattern: patterns.lobby.level });

			//	if (macroService.IsRunning) {
			//		daily.claimGuildBuff.done.IsChecked = true;
			//	}

			//	return;
			//}

			logger.info('claimGuildBuff: click guild tab');
			macroService.ClickPattern(patterns.tabs.guild);
			break;
		case 'titles.guild':
			logger.info('claimGuildBuff: click hall of honor');
			macroService.ClickPattern(patterns.guild.hallOfHonor);
			sleep(500);
			break;
		case 'titles.guildHallOfHonor':
			logger.info('claimGuildBuff: click receive guild buff');
			macroService.PollPattern(patterns.guild.hallOfHonor.notification);
			macroService.PollPattern(patterns.guild.hallOfHonor.notification, { DoClick: true, PredicatePattern: patterns.guild.hallOfHonor.recieveMessage });
			macroService.PollPattern(patterns.guild.hallOfHonor.recieveMessage, { DoClick: true, PredicatePattern: patterns.titles.guildHallOfHonor });
			if (macroService.IsRunning) {
				daily.claimGuildBuff.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}