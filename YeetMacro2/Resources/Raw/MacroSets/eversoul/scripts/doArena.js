// Use all arena tickets. Automatically fights minimum CP value
// Will rematch if cpThreshold is enabled and maximum CP is not met
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.arena.ticket];
const daily = dailyManager.GetDaily();
const isCpThresholdEnabled = settings.doArena.cpThreshold.IsEnabled;
const cpThreshold = Number(settings.doArena.cpThreshold.Value);
if (daily.doArena.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doArena: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doArena: click arena');
			macroService.PollPattern(patterns.adventure.tabs.arena, { DoClick: true, PredicatePattern: patterns.adventure.arena });
			sleep(500);
			macroService.PollPattern(patterns.adventure.arena, { DoClick: true, PredicatePattern: patterns.adventure.arena.info });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doArena: free challenges');
			macroService.PollPattern(patterns.adventure.arena.freeChallenge, { DoClick: true, PredicatePattern: patterns.adventure.arena.startMatch });
			break;
		case 'adventure.arena.startMatch':
			logger.info('doArena: start match');
			const match1CP = macroService.GetText(patterns.adventure.arena.match1.cp).replace(/[, ]/g, '');
			const match2CP = macroService.GetText(patterns.adventure.arena.match2.cp).replace(/[, ]/g, '');
			const match3CP = macroService.GetText(patterns.adventure.arena.match3.cp).replace(/[, ]/g, '');

			logger.debug('match1CP: ' + match1CP);
			logger.debug('match2CP: ' + match2CP);
			logger.debug('match3CP: ' + match3CP);

			const matches = [Number(match1CP), Number(match2CP), Number(match3CP)];
			const minIdx = matches.reduce((minIdx, val, idx, arr) => val < arr[minIdx] ? idx : minIdx, 0);
			const minCP = matches[minIdx];

			logger.debug('minIdx: ' + minIdx);
			logger.debug('minCP: ' + minCP);
			if (!isCpThresholdEnabled || minCP <= cpThreshold) {
				logger.debug('cpThreshold: ' + cpThreshold);
				macroService.PollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { DoClick: true, PredicatePattern: patterns.battle.start });
				sleep(500);
				macroService.PollPattern(patterns.battle.skip.disabled, { DoClick: true, PredicatePattern: patterns.battle.skip.enabled });
				macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.prompt.confirm2 });
				macroService.PollPattern(patterns.prompt.confirm2, { DoClick: true, PredicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.arena.ticket] });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
			} else {
				macroService.PollPattern(patterns.battle.rematch, { DoClick: true, InversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.arena.ticket':
			logger.info('doArena: done');
			if (macroService.IsRunning) {
				daily.doArena.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}