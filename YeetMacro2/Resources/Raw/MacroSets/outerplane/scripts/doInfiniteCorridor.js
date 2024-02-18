// Skip infinite corridor
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge, patterns.titles.archdemonRuins];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doInfiniteCorridor.teamSlot.Value;
const targetReward = settings.doInfiniteCorridor.targetReward.Value;
if (daily.doInfiniteCorridor.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doInfiniteCorridor: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doInfiniteCorridor: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			logger.info('doInfiniteCorridor: click archdemonRuins');
			macroService.ClickPattern(patterns.challenge.archdemonRuins);
			sleep(500);
			break;
		case 'titles.archdemonRuins':
			logger.info('doInfiniteCorridor: skip infiniteCorridor');
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.selected });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.stage3, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.selectTeam });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.selectTeam, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.enterRuins });
			selectTeam(teamSlot);
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.enterRuins, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.ok });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.ok, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.skip });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.skip, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.artifactOk });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.artifactOk, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.boss });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.boss, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.bossOk });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.bossOk, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.chest });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.chest, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.chestOk });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.chestOk, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.getReward });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.getReward[targetReward], { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.getReward[targetReward].selected });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.getReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.challenge.archdemonRuins.infiniteCorridor.endSearch });
			macroService.PollPattern(patterns.challenge.archdemonRuins.infiniteCorridor.endSearch, { DoClick: true, PredicatePattern: patterns.titles.archdemonRuins });

			if (macroService.IsRunning) {
				daily.doInfiniteCorridor.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}