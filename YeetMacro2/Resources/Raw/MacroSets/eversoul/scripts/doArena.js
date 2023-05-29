let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.titles.adventure, patterns.adventure.arena.freeChallenge, patterns.adventure.arena.startMatch, patterns.adventure.arena.ticket];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'lobby.everstone':
			logger.info('doArena: click adventure');
			await macroService.clickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doArena: click arena');
			await macroService.pollPattern(patterns.adventure.tabs.arena, { doClick: true, predicatePattern: patterns.adventure.arena });
			await macroService.pollPattern(patterns.adventure.arena, { doClick: true, predicatePattern: patterns.titles.arena });
			break;
		case 'adventure.arena.freeChallenge':
			logger.info('doArena: free challenges');
			await macroService.pollPattern(patterns.adventure.arena.freeChallenge, { doClick: true, predicatePattern: patterns.adventure.arena.startMatch });
		case 'adventure.arena.startMatch':
			logger.info('doArena: start match');
			const match1CP = (await screenService.getText(patterns.adventure.arena.match1.cp)).replace(/[, ]/g, '');
			const match2CP = (await screenService.getText(patterns.adventure.arena.match2.cp)).replace(/[, ]/g, '');
			const match3CP = (await screenService.getText(patterns.adventure.arena.match3.cp)).replace(/[, ]/g, '');

			logger.info('match1CP: ' + match1CP);
			logger.info('match2CP: ' + match2CP);
			logger.info('match3CP: ' + match3CP);

			const matches = [Number(match1CP), Number(match2CP), Number(match3CP)];
			const minIdx = matches.reduce((minIdx, val, idx, arr) => val < arr[minIdx] ? idx : minIdx, 0);
			const minCP = matches[minIdx];
			const cpThreshold = Number(settings.arena.cpThreshold.props.value);

			logger.info('minIdx: ' + minIdx);
			logger.info('minCP: ' + minCP);
			logger.info('cpThreshold: ' + cpThreshold);
			if (minCP <= cpThreshold) {
				await macroService.pollPattern(patterns.adventure.arena['match' + (minIdx + 1)].challenge, { doClick: true, predicatePattern: patterns.battle.start });
				await macroService.pollPattern(patterns.battle.start, { doClick: true, clickPattern: patterns.battle.skip, predicatePattern: [patterns.adventure.arena.freeChallenge, patterns.adventure.arena.ticket] });
			}
			else {
				await macroService.pollPattern(patterns.battle.rematch, { doClick: true, inversePredicatePattern: patterns.battle.rematch });
			}
			break;
		case 'adventure.arena.ticket':
			logger.info('doArena: done');
			done = true;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');