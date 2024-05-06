// @position=20
// Start terminus isle exploration
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage]
const daily = dailyManager.GetCurrentDaily();
if (daily.startTerminusIsleExploration.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('startTerminusIsleExploration: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doArena: click terminus isle');
			macroService.ClickPattern(patterns.adventure.terminusIsle);
			sleep(500);
			break;
		case 'terminusIsle.stage':
			logger.info('startTerminusIsleExploration: star exploration');
			const formExplorationTeamResult = macroService.PollPattern(patterns.terminusIsle.formExplorationTeam, { DoClick: true, PredicatePattern: [patterns.terminusIsle.formExplorationTeam.autoFormation, patterns.terminusIsle.zeroExplorationChances] });
			if (formExplorationTeamResult.PredicatePath === 'terminusIsle.zeroExplorationChances') {
				return;
			}
			macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.autoFormation, { DoClick: true, PredicatePattern: patterns.terminusIsle.formExplorationTeam.startExploration });
			macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.startExploration, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });

			if (macroService.IsRunning) {
				daily.startTerminusIsleExploration.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}
