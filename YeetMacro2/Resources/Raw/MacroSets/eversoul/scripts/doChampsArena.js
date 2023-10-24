const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.champsArena.buyTicket];
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.everstone':
			logger.info('doChampsArena: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doChampsArena: click champs arena');
			macroService.PollPattern(patterns.adventure.tabs.arena, { DoClick: true, PredicatePattern: patterns.adventure.champsArena });
			sleep(500);
			macroService.PollPattern(patterns.adventure.champsArena, { DoClick: true, PredicatePattern: patterns.titles.champsArena });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doChampsArena: free challenges');
			macroService.PollPattern(patterns.adventure.arena.freeChallenge, { DoClick: true, PredicatePattern: patterns.adventure.arena.startMatch, IntervalDelayMs: 1_000 });
			break;
		case 'adventure.arena.startMatch':
			logger.info('doChampsArena: start match');
			const match1CP = (macroService.GetText(patterns.adventure.arena.match1.cp)).replace(/[, ]/g, '');
			const match2CP = (macroService.GetText(patterns.adventure.arena.match2.cp)).replace(/[, ]/g, '');
			const match3CP = (macroService.GetText(patterns.adventure.arena.match3.cp)).replace(/[, ]/g, '');
			logger.info('match1CP: ' + match1CP);
			logger.info('match2CP: ' + match2CP);
			logger.info('match3CP: ' + match3CP);
			const matches = [Number(match1CP), Number(match2CP), Number(match3CP)];
			const minIdx = matches.reduce((minIdx, val, idx, arr) => val < arr[minIdx] ? idx : minIdx, 0);
			const minCP = matches[minIdx];
			const cpThreshold = Number(settings.champsArena.cpThreshold.props.value);

			logger.info('minIdx: ' + minIdx);
			logger.info('minCP: ' + minCP);
			logger.info('cpThreshold: ' + cpThreshold);
			if (minCP <= cpThreshold) {
				macroService.PollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { DoClick: true, ClickPattern: patterns.adventure.champsArena.nextTeam, PredicatePattern: patterns.battle.start, IntervalDelayMs: 1_000 });
				sleep(500);
				macroService.PollPattern(patterns.battle.start, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.champsArena.buyTicket] });
			}
			else {
				macroService.PollPattern(patterns.battle.rematch, { DoClick: true, InversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.champsArena.buyTicket':
			return;
	}

	sleep(1_000);
}
logger.info('Done...');