// Go to lobby
logger.info('goToPhone: start');

macroService.PollPattern(patterns.phone.battery, {
	ClickPattern: [
		patterns.startScreen.touchTheScreen,
		patterns.notice.close,
		patterns.phone,
		patterns.general.back,
		patterns.tribute.close,
		patterns.mall.back,
		//patterns.phone.back,
		//patterns.schedule.close
	]
});

macroService.IsRunning && (logger.info('goToPhone: done'));
