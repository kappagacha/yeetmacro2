// Go to lobby
logger.info('goToPhone: start');

const loopPatterns = [patterns.startScreen.settings, patterns.phone.battery, patterns.notice.close, patterns.notice.attendance.close];
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
		case 'notice.attendance..close':
			logger.info('goToPhone: close attendance notice');
			macroService.PollPattern(patterns.notice.attendance.claimAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.notice.attendance.close });
			macroService.PollPattern(patterns.notice.attendance.close, { DoClick: true, PredicatePattern: [patterns.phone, patterns.phone.battery] });
			return;
		case 'phone.battery':
			logger.info('goToPhone: done');
			return;
	}
	sleep(1_000);
}