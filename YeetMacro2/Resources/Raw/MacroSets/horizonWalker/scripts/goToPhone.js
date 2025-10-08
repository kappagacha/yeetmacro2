// Go to lobby
logger.info('goToPhone: start');

const loopPatterns = [patterns.startScreen.settings, patterns.phone.battery, patterns.notice.close];
const clickPatterns = [
	patterns.phone,
	patterns.general.back,
	patterns.tribute.close,
	patterns.mall.back,
]

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
		case 'startScreen.settings':
			macroService.PollPattern(patterns.startScreen.settings, { DoClick: true, ClickOffset: { X: -200 }, InversePredicatePattern: patterns.startScreen.settings });
			break;
		case 'notice.close':
			logger.info('goToPhone: close notice');
			macroService.PollPattern(patterns.notice.dontRemindMeToday, { DoClick: true, PredicatePattern: patterns.notice.dontRemindMeToday.checked });
			macroService.PollPattern(patterns.notice.close, { DoClick: true, PredicatePattern: [patterns.phone, patterns.phone.battery] });
			return;
		case 'phone.battery':
			logger.info('goToPhone: done');
			return;
	}
	sleep(1_000);
}