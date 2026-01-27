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


