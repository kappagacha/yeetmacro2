// @position=1000
// Handle popups
logger.info('handlePopups: start');

const popupPatterns = [
	patterns.notice.close,
	patterns.notice.attendance.close
];

if (macroService.FindPattern(popupPatterns).IsSuccess) {
	goToPhone();
}