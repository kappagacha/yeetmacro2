let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.arena.ticket];
while (state.isRunning() && !done) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'lobby.everstone':
			logger.info('doArena: click adventure');
			macroService.clickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doArena: click arena');
			macroService.pollPattern(patterns.adventure.tabs.arena, { doClick: true, predicatePattern: patterns.adventure.arena });
			sleep(500);
			macroService.pollPattern(patterns.adventure.arena, { doClick: true, predicatePattern: patterns.adventure.arena.info });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doArena: free challenges');
			macroService.pollPattern(patterns.adventure.arena.freeChallenge, { doClick: true, predicatePattern: patterns.adventure.arena.startMatch });
		case 'adventure.arena.startMatch':
			logger.info('doArena: start match');
			const match1CP = (screenService.getText(patterns.adventure.arena.match1.cp)).replace(/[, ]/g, '');
			const match2CP = (screenService.getText(patterns.adventure.arena.match2.cp)).replace(/[, ]/g, '');
			const match3CP = (screenService.getText(patterns.adventure.arena.match3.cp)).replace(/[, ]/g, '');

			logger.debug('match1CP: ' + match1CP);
			logger.debug('match2CP: ' + match2CP);
			logger.debug('match3CP: ' + match3CP);

			const matches = [Number(match1CP), Number(match2CP), Number(match3CP)];
			const minIdx = matches.reduce((minIdx, val, idx, arr) => val < arr[minIdx] ? idx : minIdx, 0);
			const minCP = matches[minIdx];
			const cpThreshold = Number(settings.arena.cpThreshold.props.value);

			logger.debug('minIdx: ' + minIdx);
			logger.debug('minCP: ' + minCP);
			logger.debug('cpThreshold: ' + cpThreshold);
			if (minCP <= cpThreshold) {
				macroService.pollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { doClick: true, predicatePattern: patterns.battle.start });
				sleep(500);
				macroService.pollPattern(patterns.battle.skip.disabled, { doClick: true, predicatePattern: patterns.battle.skip.enabled });
				macroService.pollPattern(patterns.battle.start, { doClick: true, predicatePattern: patterns.prompt.confirm2 });
				macroService.pollPattern(patterns.prompt.confirm2, { doClick: true, predicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.arena.ticket] });
				//macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.battle.skip, predicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.arena.ticket] });
			}
			else {
				macroService.pollPattern(patterns.battle.rematch, { doClick: true, inversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.arena.ticket':
			logger.info('doArena: done');
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');