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
} else if (settings.test.type.Value === 'applyPreset') {
	const location = settings.test.applyPreset.location.Value ?? 'top';
	const teamSlot = settings.test.applyPreset.teamSlot.Value;

	const locationToPreset = {
		left: settings.applyPreset[`teamSlot${teamSlot}`].left.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].left.Value,
		top: settings.applyPreset[`teamSlot${teamSlot}`].top.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].top.Value,
		right: settings.applyPreset[`teamSlot${teamSlot}`].right.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].right.Value,
		bottom: settings.applyPreset[`teamSlot${teamSlot}`].bottom.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].bottom.Value,
	};

	const preset = locationToPreset[location];
	const presetRegex = new RegExp(preset.replace(/ /g, '\\s*').replace(/1/g, '[1T]'));

	if (macroService.FindPattern(patterns.battle.teamFormation[location].remove).IsSuccess) {
		macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
	}
	if (macroService.FindPattern(patterns.battle.teamFormation[location].move).IsSuccess) {
		macroService.PollPattern(patterns.battle.teamFormation[location].move, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].move });
	}

	macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, HoldDurationMs: 1_000, PredicatePattern: patterns.battle.teamFormation.preset });

	macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
	const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
	const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
		const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X + 13, Y: p.Y + 8, OffsetCalcType: 'None', Path: `battle.teamFormation.preset.name_x${p.X}_y${p.Y}` });
		return {
			point: { X: p.X, Y: p.Y },
			name: macroService.FindText(presetNamePattern)
		};
	});

	return { preset, presetRegex, presetNames };
} else if (settings.test.type.Value === 'explorationOrders') {
	const cornerResult = macroService.FindPattern(patterns.terminusIsle.explorationOrder.corner, { Limit: 5 });
	const orderNames = cornerResult.Points.filter(p => p).map(p => {
		const orderNamePattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.orderName, { CenterY: p.Y + 25, Path: `patterns.terminusIsle.explorationOrder.orderName_x${p.X}_y${p.Y}` });

		return {
			point: { X: p.X, Y: p.Y + 25 },
			name: macroService.FindText(orderNamePattern)
		};
	});
	return orderNames;
} else if (settings.test.type.Value === 'mailboxExpiring') {
	const loopPatterns = [patterns.lobby.level, patterns.titles.mailbox];

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('test: click mailbox for expiring');
				macroService.ClickPattern(patterns.lobby.mailbox);
				sleep(500);
				break;
			case 'titles.mailbox':
				logger.info('test: get expiring items');
				macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
				sleep(1000);
				const receiveResult = macroService.FindPattern(patterns.mailbox.receive, { Limit: 10 });
				const expirations = receiveResult.Points.map(p => {
					const dPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { CenterX: p.X, CenterY: p.Y - 75, Width: 110, Height: 40, PathSuffix: `_x${p.X}_y${p.Y - 75}` });
					const dPatternResult = macroService.PollPattern(dPattern);
					const numberPattern = macroService.ClonePattern(patterns.mailbox.expiration.d, { X: dPatternResult.Point.X - 58, Y: dPatternResult.Point.Y - 10, Width: 50, Height: 31, Path: `patterns.mailbox.expiration.number_text_x${dPatternResult.Point.X - 30}_y${dPatternResult.Point.Y}` });
					const numberText = macroService.FindText(numberPattern, "1234567890");

					return {
						p,
						numberText: numberText
					};
				});
				return expirations;
		}
		sleep(1_000);
	}
} else if (settings.test.type.Value === 'shop') {
	const resolution = macroService.GetCurrentResolution();
	const itemCornerPattern = macroService.ClonePattern(patterns.shop.itemCorner, {
		X: 250,
		Y: 110,
		Width: resolution.Width - 300,
		Height: 920,
		OffsetCalcType: 'DockLeft'
	});

	let itemCornerResult = macroService.FindPattern(itemCornerPattern, { Limit: 12 });
	let textResults = itemCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
		const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X, Y: p.Y, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
		return {
			point: { X: p.X, Y: p.Y },
			text: macroService.FindText(itemTextPattern)
		};
	});

	return textResults;
}
