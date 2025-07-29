// @position=1000
// Go to lobby
logger.info('goToLobby: start');

const loopPatterns = [patterns.lobby];
const clickPatterns = [
	patterns.summon.exit,
	patterns.general.itemsAcquired,
]

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('goToLobby: done');
			return;
	}
	sleep(1_000);
}
