// Auto or sweep bounty chase
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetDaily();
const sweepBattle = settings.doUpgradeStoneRetrieval.sweepBattle.Value;
const teamSlot = settings.doUpgradeStoneRetrieval.teamSlot.Value;
const elementTypeTarget1 = settings.doUpgradeStoneRetrieval.elementTypeTarget1.Value;
const elementTypeTarget2 = settings.doUpgradeStoneRetrieval.elementTypeTarget2.Value;
const elementTypeTarget3 = settings.doUpgradeStoneRetrieval.elementTypeTarget3.Value;
if (daily.doUpgradeStoneRetrieval.done.IsChecked) {
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
			logger.info('doSpecialRequests: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			macroService.PollPattern(patterns.challenge.upgradeStoneRetrieval, { DoClick: true, PredicatePattern: patterns.challenge.selectTeam });
			const targetElementTypes = [elementTypeTarget1, elementTypeTarget2, elementTypeTarget3].map(ett => patterns.challenge.upgradeStoneRetrieval[ett]);
			const elementTypeResult = macroService.FindPattern(targetElementTypes);
			if (!elementTypeResult.IsSuccess) {
				throw Error(`Unable to find target element type: ${elementTypeTarget1}, ${elementTypeTarget2}, ${elementTypeTarget3}`)
			}

			const elementType = elementTypeResult.Path.split('.').pop();
			logger.info(`elementType: ${elementType}`);
			macroService.PollPattern(patterns.challenge.upgradeStoneRetrieval[elementType], { DoClick: true, PredicatePattern: patterns.challenge.upgradeStoneRetrieval[elementType].selected });
			macroService.PollPattern(patterns.challenge.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });

			selectTeamAndBattle(teamSlot, sweepBattle);
			if (macroService.IsRunning) {
				daily.doUpgradeStoneRetrieval.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}