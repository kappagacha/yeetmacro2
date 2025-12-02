// @position=1000
// Handle popups
logger.info('handlePopups: start');

const popupPatterns = [
	patterns.startScreen.tapTheScreen,
	patterns.summon.exit,
	patterns.general.itemsAcquired,
	patterns.general.close
];

if (macroService.FindPattern(popupPatterns).IsSuccess) {
	goToLobby();
}