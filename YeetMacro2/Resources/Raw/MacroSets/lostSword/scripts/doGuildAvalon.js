// @isFavorite
// @position=0
const loopPatterns = [patterns.lobby, patterns.battle.title, patterns.battle.guild.avalon.title];
const castles = ['isiel', 'elaia', 'sacredTitania', 'camelot', 'mercia', 'tintagel'];
const weekly = weeklyManager.GetCurrentWeekly();
if (weekly.doGuildAvalon.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doGuildAvalon: click battle');
			macroService.ClickPattern(patterns.battle);
			break;
		case 'battle.title':
			logger.info('doGuildAvalon: click labyrinth');
			macroService.PollPattern(patterns.battle.guild, { DoClick: true, PredicatePattern: patterns.battle.guild.selected });
			macroService.PollPattern(patterns.battle.guild.avalon, { DoClick: true, PredicatePattern: patterns.battle.guild.avalon.title });
			break;
		case 'battle.guild.avalon.title':
			for (let castle of castles) {
				logger.info(`doGuildAvalon: do ${castle} castle`);
				if (weekly.doGuildAvalon[castle].IsChecked) continue;

				macroService.PollPattern(patterns.battle.guild.avalon[castle], { DoClick: true, PredicatePattern: patterns.battle.guild.avalon.challenge });
				macroService.PollPattern(patterns.battle.guild.avalon.challenge, { DoClick: true, PredicatePattern: patterns.battle.guild.avalon.enter });
				macroService.PollPattern(patterns.battle.guild.avalon.enter, { DoClick: true, PredicatePattern: patterns.battle.guild.avalon.challenge.confirm });
				macroService.PollPattern(patterns.battle.guild.avalon.challenge.confirm, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
				macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.battle.guild.avalon.title });

				if (macroService.IsRunning) weekly.doGuildAvalon[castle].IsChecked = true;
			}

			if (macroService.IsRunning) weekly.doGuildAvalon.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}
