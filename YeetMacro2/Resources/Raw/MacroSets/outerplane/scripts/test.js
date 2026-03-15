// @tags=test

//const unitTitleAndName = macroService.FindText(patterns.battle.teamFormation.unitTitleAndName);
//return unitTitleAndName;

if (settings.test.type.Value === 'arenaTicketCount') {
	const ticketResult = macroService.PollPattern(patterns.arena.ticket);
	const slashPattern = macroService.ClonePattern(patterns.arena.ticket.slash, { X: ticketResult.Point.X + 60, Y: 35, Height: 100, Width: 100, Padding: 5, PathSuffix: `_x${ticketResult.Point.X}`, OffsetCalcType: 'None' });
	const slashResult = macroService.PollPattern(slashPattern);
	const valueBounds = {
		X: ticketResult.Point.X + 60,
		Y: ticketResult.Point.Y - 6,
		Height: 26,
		Width: slashResult.Point.X - ticketResult.Point.X - 63
	};
	while (macroService.IsRunning) {
		macroService.DebugRectangle(valueBounds);
		sleep(1_000);
	}

	//arenaTicketCount = macroService.FindTextWithBounds(valueBounds, '0123456789').slice(0, -1);
	const arenaTicketCount = macroService.FindTextWithBounds(valueBounds, '0123456789');

	return { arenaTicketCount };
} else if (settings.test.type.Value === 'currentStaminaValue') {
	const staminaResult = macroService.PollPattern(patterns.general.stamina);
	const slashPattern = macroService.ClonePattern(patterns.general.stamina.slash, { X: staminaResult.Point.X + 50, Y: staminaResult.Point.Y - 10, Height: 40, Width: 60, Padding: 5, PathSuffix: `_x${staminaResult.Point.X}`, OffsetCalcType: 'None' });
	const slashResult = macroService.PollPattern(slashPattern);
	const valueBounds = {
		X: staminaResult.Point.X + 25,
		Y: staminaResult.Point.Y - 10,
		Height: 26,
		Width: slashResult.Point.X - staminaResult.Point.X - 22
	};
	const currentStamina = macroService.FindTextWithBounds(valueBounds, '0123456789');
	if (settings.test.debugBounds.Value) {
		while (macroService.IsRunning) {
			macroService.DebugRectangle(valueBounds);
			sleep(1_000);
		}
	}
	return { currentStamina };
} else if (settings.test.type.Value === 'bossType') {
	const bossTypePatterns = [
		'grandCalamari', 'unidentifiedChimera', 'schwartz', 'amadeus', 'masterlessGuardian',
		'epsilon', 'anubisGuardian', 'tyrantToddler', 'ziggsaron', 'vladiMax', 'glicys',
		'arsNova', 'ksai', 'forestKing', 'dekRilAndMekRil', 'archdemonShadow', 'meteos',
		'gustav', 'sacreedGuardian', 'assaultSuit',
		// irregular extermination project => infiltration operation
		'irregularCrawler', 'irregularRunner', 'irregularSpike', 'irregularScythe',
		'irregularMachineGun', 'irregularBlade', 'blockbuster', 'ironStretcher', 'mutatedWyvre',
		'irregularQueen'
	].map(bt => patterns.battle.boss[bt]);
	const bossTypeResult = macroService.PollPattern(bossTypePatterns);
	const bossType = bossTypeResult.Path?.split('.').pop();
	return { bossType };
} else if (settings.test.type.Value === 'sweepAllStamina') {
	const staminaSlashResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.staminaSlash);
	const staminaPattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.currentStamina, { X: staminaSlashResult.Point.X - 69, OffsetCalcType: 'None', PathSuffix: `_${staminaSlashResult.Point.X}` });
	let staminaText = macroService.FindText(staminaPattern, '0123456789');

	if (settings.test.debugBounds.Value) {
		while (macroService.IsRunning) {
			macroService.FindPattern(staminaPattern);
			sleep(1_000);
		}
	}
	return { staminaText };
}
