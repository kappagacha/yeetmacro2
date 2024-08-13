// @raw-script
// @position=1000
function selectTeam(targetTeamSlot, returnCurrentCp) {
	if (!targetTeamSlot || targetTeamSlot === 'Current' || targetTeamSlot < 1) return;

	const topLeft = macroService.GetTopLeft();
	const xLocation = topLeft.X + 90;
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
				macroService.DoSwipe({ X: xLocation, Y: 400 }, { X: xLocation, Y: 200 });	// scroll down
			} else if (targetTeamSlot < minSlot) {
				macroService.DoSwipe({ X: xLocation, Y: 200 }, { X: xLocation, Y: 400 });	// scroll up
			}
			sleep(2_500);
		}
	}

	if (macroService.IsRunning && returnCurrentCp) {
		const cpText = macroService.GetText(patterns.battle.cp);
		return Number(cpText.slice(0, -4).slice(1) + cpText.slice(-3));
	}
}

function selectTeamAndBattle(teamSlot, sweepBattle) {
	selectTeam(teamSlot);
	macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	const numBattles = macroService.GetText(patterns.battle.setup.numBattles);
	if (sweepBattle) {
		macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
		macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });
	} else {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
	}
	return numBattles;
}

function refillStamina(targetStamina) {
	goToLobby();
	let currentStamina = macroService.GetText(patterns.lobby.staminaValue);
	logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	if (currentStamina >= targetStamina) {
		return;
	}

	macroService.PollPattern(patterns.lobby.mailbox, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
	macroService.PollPattern(patterns.mailbox.normal, { DoClick: true, PredicatePattern: patterns.mailbox.normal.selected });
	//macroService.PollPattern(patterns.mailbox.product, { DoClick: true, PredicatePattern: patterns.mailbox.product.selected });
	sleep(1000);

	const targetMailboxItem = patterns.mailbox.stamina;
	while (macroService.IsRunning && currentStamina < targetStamina) {
		macroService.SwipePollPattern(targetMailboxItem, { MaxSwipes: 20, Start: { X: 1400, Y: 900 }, End: { X: 1400, Y: 150 } });
		const staminaResult = macroService.FindPattern(targetMailboxItem);
		const recievePattern = macroService.ClonePattern(patterns.mailbox.receive, { CenterY: staminaResult.Point.Y, Height: 60.0 });
		macroService.PollPattern(recievePattern, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
		sleep(500);
		currentStamina = macroService.GetText(patterns.general.staminaValue);
		logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	}
	goToLobby();
}

function findShopItem(shopItemName) {
	const resolution = macroService.GetCurrentResolution();
	const swipeStartX = resolution.Width - 500;
	const swipeEndX = swipeStartX - 800;

	// troublesome characters: t,a,l,i
	const shopItemNameToRegex = {
		stamina: /s..m.n/is,
		gold: /go.d/is,
		clearTicket: /c.e.r.*icke/is,
		arenaTicket: /aren.*icke/is,
		hammer: /h[au]mmer/is,
		stoneFragment: /s.*ne.*fr[au]g/is,
		stonePiece: /s.*ne.*piece/is,
		basicSkillManual: /basic.*m.nu/is,
		intermediateSkillManual: /.n.er.*m/is,
		professionalSkillManual: /pro.*m/is,
		cakeSlice: /c.ke/is,
		upgradeStoneSelectionChest: /^(?!.*piece)upgr.de.*s..ne/is,		//does not contain piece (using negative look ahead)
		lowStarHeroPieceTicket: /2.*He.*r.ndom/is,
		// season 1 survey hub items
		epicReforgeCatalyst1: /ep.c.*refo.ge/is,
		epicReforgeCatalyst2: /ep.c.*refo.ge/is,
		epicReforgeCatalyst3: /ep.c.*refo.ge/is,
		epicReforgeCatalyst4: /ep.c.*refo.ge/is,
		epicReforgeCatalyst5: /ep.c.*refo.ge/is,
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
			//X: 350,
			//Y: 130,
			//Width: 1370,	//resolution.Width - 550,
			//Height: 900
			X: 250,
			Y: 130,
			Width: 1470,	//resolution.Width - 550,
			Height: 900
		});

		let itemCornerResult = macroService.FindPattern(itemCornerPattern, { Limit: 12 });
		let textResults = itemCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
			const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X, Y: p.Y, OffsetCalcType: 'None' });
			return {
				point: { X: p.X, Y: p.Y },
				text: macroService.GetText(itemTextPattern)
			};
		});

		//return textResults;

		findResult = textResults.find(tr => tr.text.match(shopItemNameToRegex[shopItemName]));

		tryCount++;
		if (!findResult && tryCount % 2 === 0) {	// scan twice before swiping
			macroService.DoSwipe({ X: swipeStartX, Y: 500 }, { X: swipeEndX, Y: 500 });
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
			const shopItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: findShopItemResult.point.X + 100,
				Y: findShopItemResult.point.Y + 200,
				Width: 250,
				Height: 100,
				Path: `patterns.shop.resource.${shopType}.${shopItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(shopItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				todo[scriptName][shopType][shopItem].IsChecked = true;
			}
		}
	}
}
