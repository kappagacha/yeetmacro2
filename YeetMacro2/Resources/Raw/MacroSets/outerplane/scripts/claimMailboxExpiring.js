// Claim mailbox items close to expiration
const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];
let done = false;
const numDaysRegex = /(?<numDays>\d+)\s*d/;
const daily = dailyManager.GetCurrentDaily();

if (daily.claimMailboxExpiring.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimMailboxExpiring: click mailbox');
			macroService.ClickPattern(patterns.lobby.mailbox);
			sleep(500);
			break;
		case 'titles.mailbox':
			logger.info('claimMailboxExpiring: claim mailbox items that are almost expiring');
			
			while (!done) {
				macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
				sleep(1000);
				const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });

				done = true;
				for (const p of receiveResult.Points) {
					const expirationPattern = macroService.ClonePattern(patterns.mailbox.expiration, { CenterY: p.Y - 75 });
					const text = macroService.GetText(expirationPattern);
					const regexResult = numDaysRegex.exec(text);
					const numDays = regexResult?.groups?.numDays;
					if (!numDays || numDays == 1) {
						macroService.PollPoint(p, { PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
						sleep(500);
						done = false;
					}
				}
			}

			if (macroService.IsRunning) {
				daily.claimMailboxExpiring.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}