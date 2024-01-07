// @position=4
// Auto guild security area
const loopPatterns = [patterns.lobby.level, patterns.titles.guildBoard, patterns.titles.guild];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doGuildSecurityArea.teamSlot.Value;
const elementTypeTarget1 = settings.doGuildSecurityArea.elementTypeTarget1.Value;
const elementTypeTarget2 = settings.doGuildSecurityArea.elementTypeTarget2.Value;

if (daily.doGuildSecurityArea.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doGuildSecurityArea: click guild tab');
			const guildNotificationResult = macroService.PollPattern(patterns.tabs.guild.notification, { TimoutMs: 1_000 });
			if (guildNotificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.tabs.guild);
			} else {	// no notification
				return;
			}
			sleep(500);
			break;
		case 'titles.guild':
			logger.info('doGuildSecurityArea: click board');
			macroService.ClickPattern(patterns.guild.board);
			sleep(500);
			break;
		case 'titles.guildBoard':
			logger.info('doGuildSecurityArea: click security area move');
			const targetElementTypes = [elementTypeTarget1, elementTypeTarget2].map(ett => patterns.guild.securityArea[ett]);
			macroService.PollPattern(patterns.guild.securityArea.move, { DoClick: true, PredicatePattern: targetElementTypes });

			const elementTypeResult = macroService.FindPattern(targetElementTypes);
			const elementType = elementTypeResult.Path.split('.').pop();
			logger.info(`elementType: ${elementType}`);
			macroService.PollPattern(patterns.guild.securityArea[elementType], { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot, false);

			if (macroService.IsRunning) {
				daily.doGuildSecurityArea.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}