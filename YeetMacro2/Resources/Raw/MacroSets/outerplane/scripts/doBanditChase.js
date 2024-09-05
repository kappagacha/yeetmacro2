// @position=8
// Auto or sweep bandit chase
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();
const sweepBattle = settings.doBanditChase.sweepBattle.Value;
const teamSlot = settings.doBanditChase.teamSlot.Value;
if (daily.doBanditChase.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doBanditChase: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doBanditChase: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			macroService.PollPattern(patterns.challenge.banditChase, { DoClick: true, PredicatePattern: patterns.challenge.selectTeam });
			macroService.PollPattern(patterns.challenge.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
			selectTeamAndBattle(teamSlot === 'RecommendedElement' ? 'dark' : teamSlot, sweepBattle);
			if (macroService.IsRunning) {
				daily.doBanditChase.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}