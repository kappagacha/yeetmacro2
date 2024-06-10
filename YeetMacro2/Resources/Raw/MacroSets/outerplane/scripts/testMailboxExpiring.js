// Test mailbox expirations
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
const numDaysRegex = /(?<numDays>\d+)\s*d/;

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
				const regexResult = numDaysRegex.exec(text);
				return {
					text,
					groups: regexResult?.groups,
					numDays: regexResult?.groups?.numDays
				};
			});
			return expirations;
	}
	sleep(1_000);
}