const loopPatterns = [patterns.lobby.everstone, patterns.town.evertalk, patterns.town.outings.outingsCompleted, patterns.town.outings, patterns.titles.outingGo, patterns.town.outings.selectAKeyword];
const targetSoul = macroService.ClonePattern(settings.outings.target.Value);
targetSoul.Path = 'settings.outings.target';
for (const pattern of targetSoul.Patterns) {
	pattern.Rect = {
		X: 275.85736083984375,
		Y: 82.9250717163086,
		//Width: 1005.4752807617188,
		Width: 1500.4752807617188,		// should calculate using resolution comparisons
		Height: 857.20263671875
	};
}
const maxSwipes = 5;
let swipeCount = 0;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.everstone':
			logger.info('doOutings: click town');
			macroService.PollPattern(patterns.lobby.town, { DoClick: true, PredicatePattern: patterns.town.enter });
			sleep(500);
			macroService.PollPattern(patterns.town.enter, { DoClick: true, PredicatePattern: patterns.town.evertalk });
			break;
		case 'town.evertalk':
			logger.info('doOutings: click info with Offset');
			macroService.PollPattern(patterns.town.info, { DoClick: true, Offset: { X: -60 }, PredicatePattern: patterns.town.outings });
			break;
		case 'town.outings':
			logger.info('doOutings: click outing target');
			sleep(500);
			swipeCount = 0;
			let result = macroService.PollPattern(targetSoul, { TimoutMs: 2000 });
			while (macroService.IsRunning && !result.IsSuccess && swipeCount < maxSwipes) {
				macroService.DoSwipe({ X: 1080, Y: 800 }, { X: 1080, Y: 250 });
				sleep(500);
				result = macroService.PollPattern(targetSoul, { TimoutMs: 2000 });
				swipeCount++;
			}

			if (!result.IsSuccess) {
				return 'Unable to find target soul';
			} 
			macroService.ClickPattern(targetSoul);
			macroService.PollPattern(targetSoul, { DoClick: true, PredicatePattern: patterns.town.outings.call });
			sleep(500);
			macroService.PollPattern(patterns.town.outings.call, { DoClick: true, PredicatePattern: patterns.prompt.confirm });
			sleep(500);
			macroService.PollPattern(patterns.prompt.confirm, { DoClick: true, PredicatePattern: patterns.town.outings.goOnOuting });
			sleep(500);
			macroService.PollPattern(patterns.town.outings.goOnOuting, { DoClick: true, ClickPattern: patterns.prompt.next, PredicatePattern: patterns.titles.outingGo });
			break;
		case 'titles.outingGo':
			logger.info('doOutings: select outing');
			const outingNumber = 1 + Math.floor(Math.random() * 4);
			logger.debug('outingNumber: ' + outingNumber);
			macroService.PollPattern(patterns.town.outings['outing' + outingNumber], { DoClick: true, PredicatePattern: patterns.prompt.confirm });
			sleep(500);
			macroService.PollPattern(patterns.prompt.confirm, { DoClick: true, ClickPattern: patterns.prompt.next, PredicatePattern: patterns.town.outings.keywordSelectionOpportunity });
			sleep(500);
			macroService.PollPattern(patterns.prompt.next, { DoClick: true, ClickPattern: patterns.prompt.middleTap, PredicatePattern: patterns.town.outings.selectAKeyword });
			break;
		case 'town.outings.selectAKeyword':
			logger.info('doOutings: select keyword');
			const keywordPoints1 = (macroService.GetText(patterns.town.outings.keywordPoints1)).replace(/[\+ ]/g, '');
			const keywordPoints2 = (macroService.GetText(patterns.town.outings.keywordPoints2)).replace(/[\+ ]/g, '');
			const keywordPoints3 = (macroService.GetText(patterns.town.outings.keywordPoints3)).replace(/[\+ ]/g, '');
			logger.info('keywordPoints1: ' + keywordPoints1);
			logger.info('keywordPoints2: ' + keywordPoints2);
			logger.info('keywordPoints3: ' + keywordPoints3);
			const keywordPoints = [Number(keywordPoints1), Number(keywordPoints2), Number(keywordPoints3)];
			const maxIdx = keywordPoints.reduce((maxIdx, val, idx, arr) => val > arr[maxIdx] ? idx : maxIdx, 0);
			logger.debug('keywordTarget: ' + (maxIdx + 1));
			macroService.PollPattern(patterns.town.outings['keywordPoints' + (maxIdx + 1)], { DoClick: true, PredicatePattern: patterns.prompt.next });
			sleep(500);
			macroService.PollPattern(patterns.prompt.next, { DoClick: true, ClickPattern: [patterns.prompt.next, patterns.prompt.tapTheScreen, patterns.prompt.middleTap], PredicatePattern: [patterns.town.outings.selectAKeyword, patterns.town.outings] });
			break;
		case 'town.outings.outingsCompleted':
			return;
	}

	sleep(1_000);
}