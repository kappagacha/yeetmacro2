// Battle in arena until out of arena tickets
// Will prioritize Memorial Match
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.arena];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doArena.teamSlot.Value;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doArena: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doArena: click arena');
			macroService.ClickPattern(patterns.adventure.arena);
			sleep(500);
			break;
		case 'titles.arena':
			macroService.PollPattern(patterns.arena.leagueOfChallenge, { DoClick: true, ClickPattern: patterns.arena.defendReport.close, PredicatePattern: patterns.arena.challenge1 })
			const numArenaTickets = macroService.GetText(patterns.arena.numTickets);
			if (numArenaTickets.trim() === '0') {
				return;
			}
			const memorialMatchNotificationResult = macroService.PollPattern(patterns.arena.memorialMatch.notification, { TimoutMs: 1_500 });
			if (memorialMatchNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.arena.memorialMatch.notification, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.selected });
				macroService.PollPattern(patterns.arena.challenge1, { DoClick: true, PredicatePattern: patterns.arena.enter });
				selectTeam(teamSlot);
				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			} else {
				macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, PredicatePattern: patterns.arena.matchOpponent.selected });
				macroService.PollPattern(patterns.arena.challenge3, { DoClick: true, PredicatePattern: patterns.arena.enter });
				selectTeam(teamSlot);
				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				if (macroService.IsRunning) {
					daily.doArena.count.Count++;
				}
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, ClickPattern: patterns.arena.tapEmptySpace, PredicatePattern: patterns.titles.arena });
			}
			break;
	}
	sleep(1_000);
}
