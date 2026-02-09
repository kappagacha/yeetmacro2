// @position=6
const loopPatterns = [patterns.lobby, patterns.shop.title];
const daily = dailyManager.GetCurrentDaily();
if (daily.doShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby':
			logger.info('doShop: click shop');
			macroService.ClickPattern(patterns.character, { ClickOffset: { X: -200 }});
			break;
		case 'shop.title':
			if (!daily.doShop.freepackage.IsChecked) {
				logger.info('doShop: claim free package');
				macroService.PollPattern(patterns.shop.product, { DoClick: true, PredicatePattern: patterns.shop.product.selected });
				macroService.PollPattern(patterns.shop.product.free, { DoClick: true, PredicatePattern: patterns.shop.product.free.free });
				macroService.PollPattern(patterns.shop.product.free.free, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, InversePredicatePattern: patterns.general.itemsAcquired });

				if (macroService.IsRunning) daily.doShop.freepackage.IsChecked = true;
			}

			if (settings.doShop.exchangeShop.eventCoin1.IsEnabled && !daily.doShop.eventCoin1.done.IsChecked) {
				macroService.PollPattern(patterns.shop.exchangeShop, { DoClick: true, PredicatePattern: patterns.shop.exchangeShop.selected });
				macroService.PollPattern(patterns.shop.coin, { SwipePattern: patterns.shop.leftPanelSwipeDown });
				macroService.SwipePattern(patterns.shop.leftPanelSwipeDown);
				sleep(2_000);
				const eventCoinType = settings.doShop.exchangeShop.eventCoin1.eventCoinType.Value;
				findCoinType(eventCoinType);
				doEventShop(settings.doShop.exchangeShop.eventCoin1, daily.doShop.eventCoin1, {
					rouletteTicket: 'Roulette Ticket',
					bossRaidTicket: 'Boss Raid Ticket',
					eventTicket: `${eventCoinType} Ticket`,
				});
				if (macroService.IsRunning) daily.doShop.eventCoin1.done.IsChecked = true;
			}

			if (settings.doShop.exchangeShop.eventCoin2.IsEnabled && !daily.doShop.eventCoin2.done.IsChecked) {
				macroService.PollPattern(patterns.shop.exchangeShop, { DoClick: true, PredicatePattern: patterns.shop.exchangeShop.selected });
				macroService.PollPattern(patterns.shop.coin, { SwipePattern: patterns.shop.leftPanelSwipeDown });
				macroService.SwipePattern(patterns.shop.leftPanelSwipeDown);
				sleep(2_000);

				const eventCoinType = settings.doShop.exchangeShop.eventCoin2.eventCoinType.Value;
				findCoinType(eventCoinType);
				doEventShop(settings.doShop.exchangeShop.eventCoin2, daily.doShop.eventCoin2, {
					rouletteTicket: 'Roulette Ticket',
					bossRaidTicket: 'Boss Raid Ticket',
					eventTicket: `${eventCoinType} Ticket`,
				});
				if (macroService.IsRunning) daily.doShop.eventCoin2.done.IsChecked = true;
			}

			if (macroService.IsRunning) daily.doShop.done.IsChecked = true;

			return;
	}
	sleep(1_000);
}

function doEventShop(shopSetting, dailySetting, shortItemNameToFullItemName) {
	for (let [shortItemName, fullItemName] of Object.entries(shortItemNameToFullItemName)) {
		if (shopSetting[shortItemName].Value && !dailySetting[shortItemName].IsChecked) {
			logger.info(`doShop: event ${fullItemName}`);
			const shopItemResult = findShopItem(fullItemName);
			//const goldClone = macroService.ClonePattern(patterns.shop.gold, {
			//	X: shopItemResult.point.X + 103,
			//	Y: shopItemResult.point.Y - 20,
			//	Padding: 10,
			//	OffsetCalcType: 'None',
			//	PathSuffix: `_${shopItemResult.point.X}x_${shopItemResult.point.Y}y`
			//});
			//if (!macroService.FindPattern(goldClone).IsSuccess) throw Error('Did not find gold icon');

			macroService.PollPoint(shopItemResult.point, { DoClick: true, PredicatePattern: [patterns.shop.buy, patterns.shop.buy2] });
			const shopItemPointResult = macroService.FindPattern([patterns.shop.buy, patterns.shop.buy2]);

			if (shopItemPointResult.Path === 'shop.buy') {
				macroService.PollPattern(patterns.shop.buy.maxButton, { DoClick: true, PredicatePattern: patterns.shop.buy.max });
				macroService.PollPattern(patterns.shop.buy, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, InversePredicatePattern: patterns.general.itemsAcquired });
			} else { // shop.buy2
				macroService.PollPattern(patterns.shop.buy2, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
				macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, InversePredicatePattern: patterns.general.itemsAcquired });
			}

			if (macroService.IsRunning) dailySetting[shortItemName].IsChecked = true;
		}
	}
}

function findCoinType(targetCoinType) {
	const topLeft = macroService.GetTopLeft();
	const coinResult = macroService.FindPattern(patterns.shop.coin, { Limit: 5 });
	let cointTypeResults = coinResult.Points.map(p => {
		const coinTypeBounds = {
			X: 80 + topLeft.X,
			Y: p.Y - 19,
			Height: 40.5,
			Width: p.X - 35 - 80 - topLeft.X
		};

		//while (macroService.IsRunning) {
		//	macroService.DebugRectangle(coinTypeBounds);
		//	sleep(1_000);
		//}
		
		return {
			point: { X: p.X, Y: p.Y },
			text: macroService.FindTextWithBounds(coinTypeBounds)
		};
	});

	const targetCoinTypeResult = cointTypeResults.find(ct => ct.text === targetCoinType);
	if (!targetCoinTypeResult) {
		throw new Error(`Could not find coin type [${targetCoinType}]`);
	}
	const leftPanelSelectedPattern = macroService.ClonePattern(patterns.shop.leftPanelSelected, { CenterY: targetCoinTypeResult.point.Y + 20, Padding: 10, PathSuffix: `_${targetCoinTypeResult.point.X}x_${targetCoinTypeResult.point.Y}y` });
	macroService.PollPoint(targetCoinTypeResult.point, { DoClick: true, PredicatePattern: leftPanelSelectedPattern });
}

function findShopItem(shopItemName) {
	const resolution = macroService.GetCurrentResolution();

	const regexString = shopItemName.replace(/ /g, "\\s*");
	const itemRegex = new RegExp(regexString, 'is');

	let findResult;
	let tryCount = 0;

	while (!findResult) {
		const lowerRightCornerResult = macroService.FindPattern(patterns.shop.lowerRightCorner, { Limit: 10 });
		let textResults = lowerRightCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
			const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X - 3, Y: p.Y - 352, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
			return {
				point: { X: p.X, Y: p.Y },
				text: macroService.FindText(itemTextPattern)
			};
		});

		findResult = textResults.find(tr => tr.text.match(itemRegex));

		tryCount++;
		if (!findResult && tryCount % 2 === 0) {	// scan twice before swiping
			macroService.SwipePattern(patterns.general.swipeRight);
			sleep(2000);
		}
	}

	if (!findResult) throw Error(`Could not find ${shopItemName}`);

	return findResult;
}