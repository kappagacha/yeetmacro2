// Go to lobby
logger.info('goToPhone: start');

const loopPatterns = [patterns.phone.battery, patterns.notice.close];
const clickPatterns = [
	patterns.startScreen.touchTheScreen,
	patterns.phone,
	patterns.general.back,
	patterns.tribute.close,
	patterns.mall.back,
]

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
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