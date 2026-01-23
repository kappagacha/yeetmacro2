// @tags=test

//const unitTitleAndName = macroService.FindText(patterns.battle.teamFormation.unitTitleAndName);
//return unitTitleAndName;

if (settings.test.type.Value === 'arenaTicketCount') {
	const ticketResult = macroService.PollPattern(patterns.arena.ticket);
	const slashPattern = macroService.ClonePattern(patterns.arena.ticket.slash, { X: ticketResult.Point.X + 60, Width: 100, Padding: 5, PathSuffix: `_x${ticketResult.Point.X}`, OffsetCalcType: 'None' });
	const slashResult = macroService.PollPattern(slashPattern);
	const valueBounds = {
		X: ticketResult.Point.X + 60,
		Y: ticketResult.Point.Y - 3,
		Height: 20,
		Width: slashResult.Point.X - ticketResult.Point.X - 70
	};
	while (macroService.IsRunning) {
		macroService.DebugRectangle(valueBounds);
		sleep(1_000);
	}

	arenaTicketCount = macroService.FindTextWithBounds(valueBounds);

	return { arenaTicketCount };
} else if (settings.test.type.Value === 'currentStaminaValue') {
	const staminaResult = macroService.PollPattern(patterns.general.stamina);
	const staminaValue = macroService.ClonePattern(patterns.general.staminaValue, { X: staminaResult.Point.X + 25, PathSuffix: `_x${staminaResult.Point.X}`, OffsetCalcType: 'None' });
	let currentStamina = macroService.FindText(staminaValue);

	if (settings.test.debugBounds.Value) {
		while (macroService.IsRunning) {
			macroService.FindPattern(staminaValue);
			sleep(1_000);
		}
	}

	for (let i = 0; i < 30 && !currentStamina; i++) {
		sleep(100);
		currentStamina = macroService.FindText(staminaValue);
	}
	return { currentStamina };
}

