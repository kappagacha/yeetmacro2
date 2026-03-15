// @raw-script
// @position=1000

function refillStamina(targetStamina) {
	goToLobby();
	let currentStamina = getCurrentStaminaValue();
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
		macroService.PollPattern(targetMailboxItem, { SwipePattern: patterns.mailbox.swipeDown, TimeoutMs: 30_000 });
		const staminaResult = macroService.FindPattern(targetMailboxItem);
		const recievePattern = macroService.ClonePattern(patterns.mailbox.receive, { CenterY: staminaResult.Point.Y, Height: 60.0, PathSuffix: `_${staminaResult.Point.Y}y` });
		macroService.PollPattern(recievePattern, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
		sleep(500);
		currentStamina = getCurrentStaminaValue();
		logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	}
	goToLobby();
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

function refillStamina2(targetStamina) {
	goToLobby();

	let currentStamina = getCurrentStaminaValue();
	logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	if (currentStamina >= targetStamina) {
		return;
	}

	while (macroService.IsRunning && currentStamina < targetStamina) {
		const diffStamina = targetStamina - currentStamina;
		const chestToStaminaConversionRate = 6;
		const targetNumChests = Math.ceil(diffStamina / chestToStaminaConversionRate);
		macroService.PollPattern(patterns.tabs.inventory, { DoClick: true, PredicatePattern: patterns.titles.inventory });
		macroService.PollPattern(patterns.inventory.consumables, { DoClick: true, PredicatePattern: patterns.inventory.consumables.selected });
		const chestResult = macroService.FindPattern(patterns.inventory.consumables.chest, { Limit: 10 });
		if (!chestResult.IsSuccess) return new Error('Could not find chest pattern');

		const lastChestPoint = chestResult.Points.reduce((lastChest, p) => {
			const yDiff = p.Y - lastChest.Y;

			// If p is significantly lower (more than 5 pixels), choose p
			if (yDiff > 5) {
				return p;
			}
			// If p is significantly higher (more than 5 pixels), keep lastChest
			if (yDiff < -5) {
				return lastChest;
			}
			// Y values are within 5 pixels - find rightmost
			if (p.X >= lastChest.X) {
				return p;
			}
			return lastChest;
		});
		macroService.PollPoint(lastChestPoint, { PredicatePattern: patterns.inventory.consumables.chest.adventurer });
		macroService.PollPattern(patterns.inventory.consumables.use, { DoClick: true, PredicatePattern: patterns.inventory.consumables.use.ok });
		macroService.PollPattern(patterns.inventory.consumables.use.min, { DoClick: true, PredicatePattern: patterns.inventory.consumables.use.sliderMin });

		let currentAmount = macroService.FindText(patterns.inventory.consumables.use.currentAmount);
		while (macroService.IsRunning && currentAmount <= targetNumChests) {
			macroService.ClickPattern(patterns.inventory.consumables.use.plus);
			sleep(100);
			currentAmount = macroService.FindText(patterns.inventory.consumables.use.currentAmount);
		}
		macroService.PollPattern(patterns.inventory.consumables.use.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
		sleep(200);
		currentStamina = getCurrentStaminaValue();
	}
}