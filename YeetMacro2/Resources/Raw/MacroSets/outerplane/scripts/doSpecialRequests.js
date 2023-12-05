// Skip all special requests once
const loopPatterns = [patterns.lobby.message, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetDaily();

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.message':
			logger.info('doSpecialRequests: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doSpecialRequests: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			if (!doSpecialRequests.ecologyStudy.done) {
				doEcologyStudy();
			}
			if (!doSpecialRequests.identification.done) {
				doIdentification();
			}
			//const numArenaTickets = macroService.GetText(patterns.arena.numTickets);
			//if (numArenaTickets.trim() === '0') {
			//	return;
			//}
			//const memorialMatchNotificationResult = macroService.PollPattern(patterns.arena.memorialMatch.notification, { TimoutMs: 1_500 });
			//if (memorialMatchNotificationResult.IsSuccess) {
			//	macroService.PollPattern(patterns.arena.memorialMatch.notification, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.selected });
			//	macroService.PollPattern(patterns.arena.challenge1, { DoClick: true, PredicatePattern: patterns.arena.enter });
			//	selectTeam(settings.doArena.teamSlot.Value);
			//	macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
			//	macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
			//	daily.doArena.count++;
			//	dailyManager.UpdateDaily(daily);
			//	macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			//} else {
			//	macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, PredicatePattern: patterns.arena.matchOpponent.selected });
			//	macroService.PollPattern(patterns.arena.challenge3, { DoClick: true, PredicatePattern: patterns.arena.enter });
			//	selectTeam(settings.doArena.teamSlot.Value);
			//	macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
			//	macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
			//	daily.doArena.count++;
			//	dailyManager.UpdateDaily(daily);
			//	macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			//}
			//break;
	}
	sleep(1_000);
}

function doEcologyStudy() {
	macroService.PollPattern(patterns.challenge.ecologyStudy, { DoClick: true, PredicatePattern: patterns.titles.ecologyStudy });
}

function doIdentification() {

}