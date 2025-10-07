// @position=14
const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.battle.party.partyDungeon.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.doPartyDungeon.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doPartyDungeon: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doPartyDungeon: party dungeon');
			macroService.PollPattern(patterns.battle.party, { DoClick: true, PredicatePattern: patterns.battle.party.selected });
			macroService.PollPattern(patterns.battle.party.partyDungeon, { DoClick: true, PredicatePattern: patterns.battle.party.partyDungeon.title });
			break;
		case 'battle.party.partyDungeon.title':
			macroService.PollPattern(patterns.battle.party.partyDungeon.startMatchmaking, { DoClick: true, PredicatePattern: patterns.battle.party.partyDungeon.confirm });
			macroService.PollPattern(patterns.battle.party.partyDungeon.confirm, { DoClick: true, PredicatePattern: patterns.battle.party.partyDungeon.return });
			macroService.PollPattern(patterns.battle.party.partyDungeon.return, { DoClick: true, PredicatePattern: patterns.battle.party.partyDungeon.title });
			
			if (macroService.IsRunning) daily.doPartyDungeon.done.IsChecked = true;
			return;
	}
	sleep(1_000);
}