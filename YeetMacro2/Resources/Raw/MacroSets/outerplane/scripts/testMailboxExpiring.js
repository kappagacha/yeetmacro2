// @position=9999
// Test mailbox expirations
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('testMailboxExpiring: click mailbox');
			macroService.ClickPattern(patterns.lobby.mailbox);
			sleep(500);
			break;
		case 'titles.mailbox':
			logger.info('testMailboxExpiring: claim non stamina product items');
			macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
			sleep(1000);
			const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
			const expirations = receiveResult.Points.map(p => {
				const expirationPattern = macroService.ClonePattern(patterns.mailbox.expiration, { CenterY: p.Y - 75 });
				const text = macroService.GetText(expirationPattern);
				const dPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { CenterX: p.X, CenterY: p.Y - 75, Width: 100, Height: 40, Path: `patterns.mailbox.expiration.d_x${p.X}_y${p.Y - 75}` });
				const dPatternResult = macroService.FindPattern(dPattern);
				if (!dPatternResult.IsSuccess) {
					return { text, numDays: 0 };
				}

				const numberPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { X: dPatternResult.Point.X - 60, Y: dPatternResult.Point.Y - 15, Width: 50, Path: `patterns.mailbox.expiration.number_text_x${dPatternResult.Point.X - 30}_y${dPatternResult.Point.Y}` });
				const numberText = macroService.GetText(numberPattern, "1234567890");
				return {
					text,
					numDays: numberText
				};
			});
			return expirations;
	}
	sleep(1_000);
}