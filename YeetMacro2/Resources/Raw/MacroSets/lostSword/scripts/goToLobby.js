// @position=1000
// Go to lobby
logger.info('goToLobby: start');

const loopPatterns = [patterns.lobby];
const clickPatterns = [
	patterns.startScreen.tapTheScreen,
	patterns.summon.exit,
	patterns.general.itemsAcquired,
	patterns.general.close
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

function handlePopups() {
	const popupPatterns = [...clickPatterns];
	if (macroService.FindPattern(popupPatterns).IsSuccess) {
		goToLobby();
	}
}