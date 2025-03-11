// @position=11
// Skip decoy operation
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepEvilSoulSubjugation.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepEvilSoulSubjugation: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('sweepEvilSoulSubjugation: click raid');
			const evilSoulSubjugationResult = macroService.PollPattern(patterns.adventure.raid, { DoClick: true, PredicatePattern: [patterns.adventure.raid.evilSoulSubjugation, patterns.adventure.raid.evilSoulSubjugation.disabled] });
			if (evilSoulSubjugationResult.PredicatePath === 'adventure.raid.evilSoulSubjugation.disabled' && macroService.IsRunning) {
				daily.sweepEvilSoulSubjugation.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.adventure.raid.evilSoulSubjugation, { DoClick: true, PredicatePattern: patterns.evilSoulSubjugation.sweep });
			//macroService.PollPattern(patterns.evilSoulSubjugation.sweep, { DoClick: true, PredicatePattern: [patterns.evilSoulSubjugation.sweep.confirm, patterns.evilSoulSubjugation.sweep.disabled] });
			macroService.PollPattern(patterns.evilSoulSubjugation.sweep, { DoClick: true, PredicatePattern: [patterns.general.tapTheScreen, patterns.evilSoulSubjugation.sweep.disabled] });
			macroService.PollPattern(patterns.evilSoulSubjugation.sweep.confirm, { DoClick: true, PredicatePattern: patterns.evilSoulSubjugation.sweep.disabled });

			if (macroService.IsRunning) {
				daily.sweepEvilSoulSubjugation.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}