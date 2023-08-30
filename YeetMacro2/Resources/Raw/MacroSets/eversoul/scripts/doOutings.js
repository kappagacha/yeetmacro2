let done = false;
const loopPatterns = [patterns.lobby.everstone, patterns.town.evertalk, patterns.town.outings.outingsCompleted, patterns.town.outings, patterns.titles.outingGo, patterns.town.outings.selectAKeyword];
while (state.isRunning && !done) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'lobby.everstone':
			logger.info('doOutings: click town');
			macroService.pollPattern(patterns.lobby.town, { doClick: true, predicatePattern: patterns.town.enter });
			sleep(500);
			macroService.pollPattern(patterns.town.enter, { doClick: true, predicatePattern: patterns.town.evertalk });
			break;
		case 'town.evertalk':
			logger.info('doOutings: click info with offset');
			macroService.pollPattern(patterns.town.info, { doClick: true, clickOffsetX: -60, predicatePattern: patterns.town.outings });
			break;
		case 'town.outings':
			logger.info('doOutings: click outing target');
			logger.debug(JSON.stringify(settings.outings.target, null, 2));
			const targetSoul = {
				props: {
					...settings.outings.target.props.value,
					path: 'settings.outings.target',
					patterns: settings.outings.target.props.value.patterns.map(p => ({
						...p,
						rect: {
							x: 275.85736083984375,
							y: 82.9250717163086,
							//width: 1005.4752807617188,
							width: 1500.4752807617188,		// should calculate using resolution comparisons
							height: 857.20263671875
						},
					})),
				}
			};

			sleep(500);
			macroService.clickPattern(targetSoul);
			macroService.pollPattern(targetSoul, { doClick: true, predicatePattern: patterns.town.outings.call });
			sleep(500);
			macroService.pollPattern(patterns.town.outings.call, { doClick: true, predicatePattern: patterns.prompt.confirm });
			sleep(500);
			macroService.pollPattern(patterns.prompt.confirm, { doClick: true, predicatePattern: patterns.town.outings.goOnOuting });
			sleep(500);
			macroService.pollPattern(patterns.town.outings.goOnOuting, { doClick: true, clickPattern: patterns.prompt.next, predicatePattern: patterns.titles.outingGo });
			break;
		case 'titles.outingGo':
			logger.info('doOutings: select outing');
			const outingNumber = 1 + Math.floor(Math.random() * 4);
			logger.debug('outingNumber: ' + outingNumber);

			macroService.pollPattern(patterns.town.outings['outing' + outingNumber], { doClick: true, predicatePattern: patterns.prompt.confirm });
			sleep(500);
			macroService.pollPattern(patterns.prompt.confirm, { doClick: true, clickPattern: patterns.prompt.next, predicatePattern: patterns.town.outings.keywordSelectionOpportunity });
			sleep(500);
			macroService.pollPattern(patterns.prompt.next, { doClick: true, clickPattern: patterns.prompt.middleTap, predicatePattern: patterns.town.outings.selectAKeyword });
			break;
		case 'town.outings.selectAKeyword':
			logger.info('doOutings: select keyword');
			const keywordPoints1 = (screenService.getText(patterns.town.outings.keywordPoints1)).replace(/[\+ ]/g, '');
			const keywordPoints2 = (screenService.getText(patterns.town.outings.keywordPoints2)).replace(/[\+ ]/g, '');
			const keywordPoints3 = (screenService.getText(patterns.town.outings.keywordPoints3)).replace(/[\+ ]/g, '');
			logger.info('keywordPoints1: ' + keywordPoints1);
			logger.info('keywordPoints2: ' + keywordPoints2);
			logger.info('keywordPoints3: ' + keywordPoints3);
			const keywordPoints = [Number(keywordPoints1), Number(keywordPoints2), Number(keywordPoints3)];
			const maxIdx = keywordPoints.reduce((maxIdx, val, idx, arr) => val > arr[maxIdx] ? idx : maxIdx, 0);
			logger.debug('keywordTarget: ' + (maxIdx + 1));
			macroService.pollPattern(patterns.town.outings['keywordPoints' + (maxIdx + 1)], { doClick: true, predicatePattern: patterns.prompt.next });
			sleep(500);
			macroService.pollPattern(patterns.prompt.next, { doClick: true, clickPattern: [patterns.prompt.next, patterns.prompt.tapTheScreen, patterns.prompt.middleTap], predicatePattern: [patterns.town.outings.selectAKeyword, patterns.town.outings] });
			break;
		case 'town.outings.outingsCompleted':
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');