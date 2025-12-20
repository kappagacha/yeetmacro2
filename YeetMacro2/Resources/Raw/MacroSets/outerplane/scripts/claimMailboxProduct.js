// Claim non stamina mailbox product items
// @tags=mailbox
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
let allStamina = true;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimMailboxProduct: click mailbox');
			macroService.ClickPattern(patterns.lobby.mailbox);
			sleep(500);
			break;
		case 'titles.mailbox':
			logger.info('claimMailboxProduct: claim non stamina product items');
			macroService.PollPattern(patterns.mailbox.product, { DoClick: true, PredicatePattern: patterns.mailbox.product.selected });
			sleep(1000);
			while (macroService.IsRunning) {
				const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
				allStamina = true;
				for (const p of receiveResult.Points) {
					const staminaPattern = macroService.ClonePattern(patterns.mailbox.stamina, { CenterY: p.Y, Height: 50 });
					const staminaPatternResult = macroService.FindPattern(staminaPattern);
					if (staminaPatternResult.IsSuccess) {
						continue;
					}
					macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
					sleep(500);
					allStamina = false;
					break;
				}
				if (allStamina) {
					macroService.SwipePattern(patterns.mailbox.swipeDown);
					sleep(1_500);
				}
			}
			return;
	}
	sleep(1_000);
}