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

			if (settings.doShop.event.IsEnabled) {
				macroService.PollPattern(patterns.shop.event, { DoClick: true, PredicatePattern: patterns.shop.event.selected });

				if (settings.doShop.event.vacationTicket.Value && !daily.doShop.event.vacationTicket.IsChecked) {
					logger.info('doShop: event.vacationTicket');
					const shopItemResult = findShopItem('Vacation Ticket');
					const goldClone = macroService.ClonePattern(patterns.shop.gold, {
						X: shopItemResult.point.X + 103,
						Y: shopItemResult.point.Y - 20,
						Padding: 10,
						OffsetCalcType: 'None',
						PathSuffix: `_${shopItemResult.point.X}x_${shopItemResult.point.Y}y`
					});
					if (!macroService.FindPattern(goldClone).IsSuccess) throw Error('Did not find gold icon');

					macroService.PollPoint(shopItemResult.point, { DoClick: true, PredicatePattern: patterns.shop.buy });
					macroService.PollPattern(patterns.shop.buy.maxButton, { DoClick: true, PredicatePattern: patterns.shop.buy.max });
					macroService.PollPattern(patterns.shop.buy, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
					macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, InversePredicatePattern: patterns.general.itemsAcquired });

					if (macroService.IsRunning) daily.doShop.event.vacationTicket.IsChecked = true;
				}
			}

			if (macroService.IsRunning) daily.doShop.done.IsChecked = true;

			return;
	}
	sleep(1_000);
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