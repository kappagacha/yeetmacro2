// Battle in arena until out of arena tickets
// Will prioritize Memorial Match
const loopPatterns = [patterns.lobby.message, patterns.titles.adventure, patterns.titles.arena];
const daily = dailyManager.GetDaily();
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.message':
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
			const numArenaTickets = macroService.GetText(patterns.arena.numTickets);
			if (numArenaTickets.trim() === '0') {
				return;
			}
			const memorialMatchNotificationResult = macroService.PollPattern(patterns.arena.memorialMatch.notification, { TimoutMs: 1_500 });
			if (memorialMatchNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.arena.memorialMatch.notification, { DoClick: true, PredicatePattern: patterns.arena.memorialMatch.selected });
				macroService.PollPattern(patterns.arena.challenge1, { DoClick: true, PredicatePattern: patterns.arena.enter });
				selectTeam(settings.doArena.teamSlot.Value);
				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				daily.doArena.count++;
				dailyManager.UpdateDaily(daily);
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			} else {
				macroService.PollPattern(patterns.arena.matchOpponent, { DoClick: true, PredicatePattern: patterns.arena.matchOpponent.selected });
				macroService.PollPattern(patterns.arena.challenge3, { DoClick: true, PredicatePattern: patterns.arena.enter });
				selectTeam(settings.doArena.teamSlot.Value);
				macroService.PollPattern(patterns.arena.enter, { DoClick: true, PredicatePattern: patterns.arena.auto.disabled });
				macroService.PollPattern(patterns.arena.auto.disabled, { DoClick: true, PredicatePattern: patterns.arena.matchResult });
				daily.doArena.count++;
				dailyManager.UpdateDaily(daily);
				macroService.PollPattern(patterns.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.arena });
			}
			break;
	}
	sleep(1_000);
}

function selectTeam(target) {
	if (!target || target === 'Current' || target < 1) return;

	const spacing = 90;
	const targetTeamSlot = macroService.ClonePattern(patterns.battle.teamSlot);
	targetTeamSlot.Path = 'battles.teamSlot' + target;
	for (const pattern of targetTeamSlot.Patterns) {
		pattern.Rect = { X: pattern.Rect.X, Y: pattern.Rect.Y + (spacing * (target - 1)), Width: pattern.Rect.Width, Height: pattern.Rect.Height };
		pattern.OffsetCalcType = "None";
		pattern.IsBoundsPattern = true;
	}

	const targetTeamSlotSelected = macroService.ClonePattern(patterns.battle.teamSlot.selected);
	targetTeamSlotSelected.Path = 'battles.teamSlot.selected' + target;
	for (const pattern of targetTeamSlotSelected.Patterns) {
		pattern.Rect = { X: pattern.Rect.X, Y: pattern.Rect.Y + (spacing * (target - 1)), Width: pattern.Rect.Width, Height: pattern.Rect.Height };
		pattern.OffsetCalcType = "None";
	}

	macroService.pollPattern(targetTeamSlot, { DoClick: true, PredicatePattern: targetTeamSlotSelected });
}
