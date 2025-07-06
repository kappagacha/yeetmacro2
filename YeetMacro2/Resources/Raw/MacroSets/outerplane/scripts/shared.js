// @raw-script
// @position=1000
function refillStamina(targetStamina) {
	goToLobby();
	let currentStamina = macroService.FindText(patterns.lobby.staminaValue);
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
		const recievePattern = macroService.ClonePattern(patterns.mailbox.receive, { CenterY: staminaResult.Point.Y, Height: 60.0, PathSuffix: `_${staminaResult.Point.Y}y` });
		macroService.PollPattern(recievePattern, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
		sleep(500);
		currentStamina = macroService.FindText(patterns.general.staminaValue);
		logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	}
	goToLobby();
}

function getCurrentStaminaValue() {
	const staminaResult = macroService.PollPattern(patterns.general.stamina);
	const staminaValue = macroService.ClonePattern(patterns.general.staminaValue, { X: staminaResult.Point.X + 58, PathSuffix: `_y${staminaResult.Point.Y}`, OffsetCalcType: 'None' })
	const currentStamina = macroService.FindText(staminaValue);
	return Number(currentStamina);
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
		threeStarHeroPieceTicket: /3.*He.*select/is,
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
				Height: 200,
				PathSuffix: `_${shopType}_${shopItem}_${findShopItemResult.point.X}x_${findShopItemResult.point.Y}y`,
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
