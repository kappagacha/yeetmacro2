// Use all champs arena tickets. Automatically fights minimum CP value
// Will rematch if cpThreshold is enabled and maximum CP is not met
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.champsArena.buyTicket];
const daily = dailyManager.GetDaily();
const isCpThresholdEnabled = settings.doChampsArena.cpThreshold.IsEnabled;
const cpThreshold = Number(settings.doChampsArena.cpThreshold.Value);
if (daily.doChampsArena.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doChampsArena: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doChampsArena: click champs arena');
			macroService.PollPattern(patterns.adventure.tabs.arena, { DoClick: true, PredicatePattern: patterns.adventure.champsArena });
			sleep(500);
			macroService.PollPattern(patterns.adventure.champsArena, { DoClick: true, PredicatePattern: patterns.adventure.arena.freeChallenge });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doChampsArena: free challenges');
			macroService.PollPattern(patterns.adventure.arena.freeChallenge, { DoClick: true, PredicatePattern: patterns.adventure.arena.startMatch, IntervalDelayMs: 1_500 });
			break;
		case 'adventure.arena.startMatch':
			logger.info('doChampsArena: start match');
			const match1CP = macroService.GetText(patterns.adventure.arena.match1.cp).replace(/[, ]/g, '');
			const match2CP = macroService.GetText(patterns.adventure.arena.match2.cp).replace(/[, ]/g, '');
			const match3CP = macroService.GetText(patterns.adventure.arena.match3.cp).replace(/[, ]/g, '');
			logger.info('match1CP: ' + match1CP);
			logger.info('match2CP: ' + match2CP);
			logger.info('match3CP: ' + match3CP);
			const matches = [Number(match1CP), Number(match2CP), Number(match3CP)];
			const minIdx = matches.reduce((minIdx, val, idx, arr) => val < arr[minIdx] ? idx : minIdx, 0);
			const minCP = matches[minIdx];

			logger.info('minIdx: ' + minIdx);
			logger.info('minCP: ' + minCP);
			logger.info('cpThreshold: ' + cpThreshold);
			if(!isCpThresholdEnabled || minCP <= cpThreshold) {
				macroService.PollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { DoClick: true, ClickPattern: patterns.adventure.champsArena.nextTeam, PredicatePattern: patterns.battle.start, IntervalDelayMs: 1_000 });
				sleep(500);
				macroService.PollPattern(patterns.battle.skip.disabled, { DoClick: true, PredicatePattern: patterns.battle.skip.enabled });
				macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.prompt.confirm2 });
				macroService.PollPattern(patterns.prompt.confirm2, { DoClick: true, PredicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.champsArena.buyTicket] });
				if (macroService.IsRunning) {
					daily.doChampsArena.count.Count++;
				}
			}
			else {
				macroService.PollPattern(patterns.battle.rematch, { DoClick: true, InversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.champsArena.buyTicket':
			logger.info('doChampsArena: done');
			if (macroService.IsRunning) {
				daily.doChampsArena.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}
logger.info('Done...');