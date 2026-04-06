// @raw-script
// @position=1000

function selectTeam(targetTeamSlot, opts = {}) {
	//logger.info(`selectTeam teamSlot ${targetTeamSlot}`);

	if (!targetTeamSlot || targetTeamSlot === 'Current' || targetTeamSlot < 1) return;

	if (targetTeamSlot === 'RecommendedElement') {
		const recommendedElement = {
			earth: 'fire',
			water: 'earth',
			fire: 'water',
			light: 'dark',
			dark: 'light'
		};
		const bossType = detectBossType();
		targetTeamSlot = recommendedElement[bossType];
	}

	if (['earth', 'water', 'fire', 'light', 'dark'].includes(targetTeamSlot)) {
		targetTeamSlot = settings.selectTeam[targetTeamSlot].Value;
	}


	const slots = [1, 2, 3, 4, 5, 6, 7, 8, 9];

	let targetSlotSelectedResult = macroService.FindPattern(patterns.battle.slot[targetTeamSlot].selected);
	if (!targetSlotSelectedResult.IsSuccess) {
		while (macroService.IsRunning) {
			const slotsResult = Object.assign(...slots.map(s => ({ [s]: macroService.FindPattern(patterns.battle.slot[s]).IsSuccess })));
			if (slotsResult[targetTeamSlot]) {	// found slot
				macroService.PollPattern(patterns.battle.slot[targetTeamSlot], { DoClick: true, PredicatePattern: patterns.battle.slot[targetTeamSlot].selected });
				break;
			}

			// slot not found so need to swipe
			const minSlot = Object.entries(slotsResult).reduce((min, [key, val]) => val && key < min ? key : min, 9);
			const maxSlot = Object.entries(slotsResult).reduce((max, [key, val]) => val && key > max ? key : max, 1);
			if (targetTeamSlot > maxSlot) {
				macroService.SwipePattern(patterns.battle.slot.swipeDown);
			} else if (targetTeamSlot < minSlot) {
				macroService.SwipePattern(patterns.battle.slot.swipeUp);
			}
			sleep(2_500);
		}
	}

	if (opts.applyPreset) {
		applyPreset(targetTeamSlot);
	}
}

function selectTeamAndBattle(teamSlot, opts = {}) {
	selectTeam(teamSlot, opts);
	const autoResult = macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: [patterns.battle.setup.sweep, patterns.battle.setup.enter] });

	let numBattles = macroService.FindText(patterns.battle.setup.numBattles);
	if (opts.targetNumBattles) {
		macroService.PollPattern(patterns.battle.setup.minBattle, { DoClick: true, PredicatePattern: patterns.battle.setup.numBattles.one });

		numBattles = 1
		while (Number(numBattles) < opts.targetNumBattles) {
			macroService.ClickPattern(patterns.battle.setup.addBattle)
			sleep(250)
			numBattles = macroService.FindText(patterns.battle.setup.numBattles);
			sleep(250)
		}
	}

	if (autoResult.PredicatePath === 'battle.setup.enter') {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
		return numBattles;
	}

	macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
	macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });

	return numBattles;
}

function detectBossType() {
	const bossElements = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.battle.bossType[el]);
	const bossElementsResult = macroService.PollPattern(bossElements);
	const detectedElement = bossElementsResult.Path?.split('.').pop();

	return detectedElement;
}

function setChainOrder(opts = {}) {
	macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: [patterns.battle.chainPreview, patterns.battle.chainPreview.selected], PredicatePattern: [patterns.battle.chainPreview, patterns.battle.chainPreview.selected] });
	macroService.PollPattern(patterns.battle.chainPreview, { DoClick: true, PredicatePattern: patterns.battle.chainPreview.selected });
	if (macroService.FindPattern(patterns.battle.chainPreview.missingSlots).IsSuccess) return;

	const chainPreviewResult = { A: {}, B: {}, C: {}, D: {} };

	const chainPositionBasePatterns = [
		patterns.battle.chainPreview.starterExclusive,
		patterns.battle.chainPreview.starterExclusive.enabled,
		patterns.battle.chainPreview.companion,
		patterns.battle.chainPreview.finisherExclusive,
		patterns.battle.chainPreview.finisherExclusive.enabled
	];

	const effectsBasePatterns = [
		patterns.battle.chainPreview.effects.pctDmg,
		patterns.battle.chainPreview.effects.cdr,
		patterns.battle.chainPreview.effects.weaknessGuage,
		patterns.battle.chainPreview.effects.cursed,
	];

	const effectToPriority = {
		pctDmg: 1,
		cdr: 2,
		weaknessGuage: 3,
		cursed: 4,
		...(opts?.effectToPriority ?? {}),
	};

	while (macroService.IsRunning) {
		for (let position of Object.keys(chainPreviewResult)) {
			// $patterns uses snapshot patterns json and the other uses actual C# class PatternNodeViewModel
			const originalRawBounds = patterns.battle.chainPreview[position]?.Pattern?.RawBounds || patterns.battle.chainPreview[position].$patterns[0].rawBounds;
			let rawBounds = { ...originalRawBounds };
			const cloneOpts = { RawBounds: rawBounds, PathSuffix: `_${position}` };
			const chainPositionPatterns = chainPositionBasePatterns.map(p => macroService.ClonePattern(p, cloneOpts));
			const effectPatterns = effectsBasePatterns.map(p => macroService.ClonePattern(p, cloneOpts));

			chainPreviewResult[position].chainEffectPosition = macroService.PollPattern(chainPositionPatterns).Path?.split('.').pop().slice(0, -2);
			chainPreviewResult[position].effect = macroService.FindPattern(effectPatterns).Path?.split('.').pop().slice(0, -2);
			chainPreviewResult[position].priority = chainPreviewResult[position].effect ? effectToPriority[chainPreviewResult[position].effect] : 100;
		}

		let targetStarterExclusive = 'A';
		let targetFinisherExclusive = 'D';
		let starterMin = Infinity;
		let finisherMin = Infinity;

		for (const [key, value] of Object.entries(chainPreviewResult)) {
			const { chainEffectPosition, priority } = value;

			// get first highest priority
			if (chainEffectPosition === "starterExclusive" && priority < starterMin) {
				starterMin = priority;
				targetStarterExclusive = key;
			}

			// get last hightest priority
			if (chainEffectPosition === "finisherExclusive" && priority <= finisherMin) {
				finisherMin = priority;
				targetFinisherExclusive = key;
			}
		}

		if (targetStarterExclusive === 'A' && targetFinisherExclusive === 'D') break;

		if (targetStarterExclusive !== 'A') {
			macroService.ClickPattern(patterns.battle.chainPreview.A);
			sleep(250);
			macroService.ClickPattern(patterns.battle.chainPreview[targetStarterExclusive]);
		}

		if (targetFinisherExclusive !== 'D' && targetStarterExclusive !== 'D') {
			macroService.ClickPattern(patterns.battle.chainPreview.D);
			sleep(250);
			macroService.ClickPattern(patterns.battle.chainPreview[targetFinisherExclusive]);
		}
	}
}

function getCurrentStaminaValue() {
	//const staminaResult = macroService.PollPattern(patterns.general.stamina);
	//const staminaValue = macroService.ClonePattern(patterns.general.staminaValue, { X: staminaResult.Point.X + 24, Y: staminaResult.Point.Y - 10, PathSuffix: `_x${staminaResult.Point.X}`, OffsetCalcType: 'None' });
	//let currentStamina = macroService.FindText(staminaValue);

	//for (let i = 0; i < 30 && !currentStamina; i++) {
	//	sleep(100);
	//	currentStamina = macroService.FindText(staminaValue);
	//}
	//return Number(currentStamina);

	const staminaResult = macroService.PollPattern(patterns.general.stamina);
	const slashPattern = macroService.ClonePattern(patterns.general.stamina.slash, { X: staminaResult.Point.X + 50, Y: staminaResult.Point.Y - 10, Height: 40, Width: 60, Padding: 5, PathSuffix: `_x${staminaResult.Point.X}`, OffsetCalcType: 'None' });
	const slashResult = macroService.PollPattern(slashPattern);
	const valueBounds = {
		X: staminaResult.Point.X + 25,
		Y: staminaResult.Point.Y - 10,
		Height: 26,
		Width: slashResult.Point.X - staminaResult.Point.X - 22
	};
	const currentStamina = macroService.FindTextWithBounds(valueBounds, '0123456789').slice(0, -1);
	return Number(currentStamina);
}

function findShopItem(shopItemName) {
	const resolution = macroService.GetCurrentResolution();

	// troublesome characters: t,a,l,i
	const shopItemNameToRegex = {
		stamina: /s..m.n/is,
		gold: /go.d/is,
		clearTicket: /c.e.r.*icke/is,
		arenaTicket: /aren.*icke/is,
		hammer: /h[au]mmer/is,
		stoneFragment: /s.*ne.*fr.g/is,
		stonePiece: /s.*ne.*piece/is,
		basicSkillManual: /basic.*m.nu/is,
		intermediateSkillManual: /.n.er.*m/is,
		professionalSkillManual: /pro.*m/is,
		cakeSlice: /c.ke/is,
		upgradeStoneSelectionChest: /^(?!.*piece)upgr.de.*s..ne/is,		//does not contain piece (using negative look ahead)
		lowStarHeroPieceTicket: /2.*He.*r.ndom/is,
		threeStarHeroPieceTicket: /3.*He.*selec/is,
		// season 1 survey hub items
		epicReforgeCatalyst: /ep.c.*refo.ge/is,
		'30pctEpicAbrasive': /3[0o].*ep.c.*/is,
		superiorQualityPresentChest: /sup.*pres/is,
		'10pctLegendaryAbrasive': /1[0o].*[lt]eg/is,
		// season 2 survey hub items
		stage2RandomGemChest: /r.ndom.*gem/is,
		legendaryReforgeCatalyst: /.eg.*ref/is,
		epicQualityPresentChest: /epic.*pres/is,
		refinedGlunite: /ref.*?g.un/is,
		// joint challenge items
		specialRecruitmentTicket: /spec.a..*recru/is,
		normalRecruitmentTicket: /norma..*recru/is,
		stage3GemChest: /3.*chest/is
	}

	let findResult;
	let tryCount = 0;

	while (!findResult) {
		const itemCornerPattern = macroService.ClonePattern(patterns.shop.itemCorner, {
			X: 250,
			Y: 110,
			//Width: 1450,	//resolution.Width - 550,
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

		//return textResults;

		findResult = textResults.find(tr => tr.text.match(shopItemNameToRegex[shopItemName]));

		tryCount++;
		if (!findResult && tryCount % 2 === 0) {	// scan twice before swiping
			macroService.SwipePattern(patterns.general.swipeRight);
			sleep(2000);
		}
	}

	return findResult;
}

function doShopItems(scriptName, shopType, shopItems, isWeekly = false) {
	const weekly = weeklyManager.GetCurrentWeekly();
	const daily = dailyManager.GetCurrentDaily();
	const todo = isWeekly ? weekly : daily;

	for (const shopItem of shopItems) {
		if (settings[scriptName][shopType][shopItem].Value && !todo[scriptName][shopType][shopItem].IsChecked) {
			logger.info(`${scriptName}: purchase shopItem ${shopItem}`);

			const findShopItemResult = findShopItem(shopItem);
			const shopItemPurchasePattern = macroService.ClonePattern(patterns.shop.purchase, {
				X: findShopItemResult.point.X + 100,
				Y: findShopItemResult.point.Y + 200,
				Width: 250,
				Height: 200,
				PathSuffix: `_${shopType}_${shopItem}_${findShopItemResult.point.X}x_${findShopItemResult.point.Y}y`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(shopItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.purchase.ok });
			const maxResult = macroService.FindPattern(patterns.shop.purchase.max);
			if (maxResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.purchase.max, { DoClick: true, PredicatePattern: patterns.shop.purchase.sliderMax });
			}
			macroService.PollPattern(patterns.shop.purchase.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.titles.adventurerShop, patterns.titles.shop, patterns.shop.premium.title] });
			if (macroService.IsRunning) {
				todo[scriptName][shopType][shopItem].IsChecked = true;
			}
		}
	}
}
