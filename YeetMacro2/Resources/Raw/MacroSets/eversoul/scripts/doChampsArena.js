let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.champsArena.buyTicket];
while (state.isRunning && !done) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'lobby.everstone':
			logger.info('doChampsArena: click adventure');
			macroService.clickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doChampsArena: click champs arena');
			macroService.pollPattern(patterns.adventure.tabs.arena, { doClick: true, predicatePattern: patterns.adventure.champsArena });
			sleep(500);
			macroService.pollPattern(patterns.adventure.champsArena, { doClick: true, predicatePattern: patterns.titles.champsArena });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doChampsArena: free challenges');
			macroService.pollPattern(patterns.adventure.arena.freeChallenge, { doClick: true, predicatePattern: patterns.adventure.arena.startMatch, intervalDelayMs: 1_000 });
			break;
		case 'adventure.arena.startMatch':
			logger.info('doChampsArena: start match');
			const match1CP = (screenService.getText(patterns.adventure.arena.match1.cp)).replace(/[, ]/g, '');
			const match2CP = (screenService.getText(patterns.adventure.arena.match2.cp)).replace(/[, ]/g, '');
			const match3CP = (screenService.getText(patterns.adventure.arena.match3.cp)).replace(/[, ]/g, '');
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
				macroService.pollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { doClick: true, clickPattern: patterns.adventure.champsArena.nextTeam, predicatePattern: patterns.battle.start, intervalDelayMs: 1_000 });
				sleep(500);
				macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.battle.skip, predicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.champsArena.buyTicket] });
			}
			else {
				macroService.pollPattern(patterns.battle.rematch, { doClick: true, inversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.champsArena.buyTicket':
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');