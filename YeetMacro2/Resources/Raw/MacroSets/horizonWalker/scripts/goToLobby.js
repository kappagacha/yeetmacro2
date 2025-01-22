// Go to lobby
logger.info('goToLobby: start');

macroService.PollPattern(patterns.lobby.stage, {
	ClickPattern: [
		patterns.general.back,
		patterns.tribute.close,
		patterns.mall.back,
		patterns.phone.back,
		patterns.schedule.close
	]
});

macroService.IsRunning && (logger.info('goToLobby: done'));
