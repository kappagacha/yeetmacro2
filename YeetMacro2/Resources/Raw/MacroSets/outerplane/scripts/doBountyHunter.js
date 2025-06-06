// @position=7
// Auto or sweep bounty hunter
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doBountyHunter.teamSlot.Value;
if (daily.doBountyHunter.done.IsChecked) {
	return "Script already completed.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.general.doNotShowFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doBountyHunter: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doBountyHunter: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			macroService.PollPattern(patterns.challenge.bountyHunter, { DoClick: true, PredicatePattern: patterns.challenge.selectTeam });
			macroService.PollPattern(patterns.challenge.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
			selectTeamAndBattle(teamSlot === 'RecommendedElement' ? 'light' : teamSlot);
			if (macroService.IsRunning) {
				daily.doBountyHunter.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}