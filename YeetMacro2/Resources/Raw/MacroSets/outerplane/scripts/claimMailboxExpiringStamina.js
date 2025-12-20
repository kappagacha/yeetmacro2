// Claim mailbox items close to expiration (only stamina)
// @tags=mailbox
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
let done = false;
const daily = dailyManager.GetCurrentDaily();

//if (daily.claimMailboxExpiringStamina.done.IsChecked) {
//if (daily.claimMailboxExpiringStamina.count.Count > 2) {
//	return "Script already completed.";
//}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimMailboxExpiringStamina: click mailbox');
			macroService.ClickPattern(patterns.lobby.mailbox);
			sleep(500);
			break;
		case 'titles.mailbox':
			logger.info('claimMailboxExpiringStamina: claim mailbox items that are almost expiring');
			
			while (!done) {
				macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
				//macroService.PollPattern(patterns.mailbox.receive);
				sleep(3_000);
				const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
				if (!receiveResult.IsSuccess) break;

				done = true;
				for (const p of receiveResult.Points) {
					const staminaPattern = macroService.ClonePattern(patterns.mailbox.stamina, { CenterY: p.Y, Height: 50, PathSuffix: `_${p.Y}y` });
					const staminaPatternResult = macroService.FindPattern(staminaPattern);
					if (!staminaPatternResult.IsSuccess) {	// only stamina
						continue;
					}

					const dPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { CenterX: p.X, CenterY: p.Y - 75, Width: 110, Height: 40, PathSuffix: `_x${p.X}_y${p.Y - 75}` });
					const dPatternResult = macroService.FindPattern(dPattern);
					if (!dPatternResult.IsSuccess) {
						macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.mailbox.receive });
						sleep(2_000);
						done = false;
						continue;
					}

					const numberPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { X: dPatternResult.Point.X - 58, Y: dPatternResult.Point.Y - 10, Width: 50, Height: 31, Path: `patterns.mailbox.expiration.number_text_x${dPatternResult.Point.X - 30}_y${dPatternResult.Point.Y}` });
					const numberText = macroService.FindText(numberPattern, "1234567890");

					if (numberText == 1 || numberText == 2 || numberText > 10) {
						macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.mailbox.receive });
						sleep(2_000);
						done = false;
					}
				}
			}

			if (macroService.IsRunning) {
				//daily.claimMailboxExpiringStamina.done.IsChecked = true;
				daily.claimMailboxExpiringStamina.count.Count++;
			}
			return;
	}
	sleep(1_000);
}