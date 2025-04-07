// @position=7
// Buy mana crystals and class enhance with gold
// Buy artifact memories
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetCurrentDaily();
if (daily.doShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doShop: click shop');
			macroService.ClickPattern(patterns.lobby.shop);
			sleep(500);
			break;
		case 'general.back':
			logger.info('doShop: general');

			if (settings.doShop.buyManaCrystalWithGold.Value) {
				macroService.PollPattern(patterns.shop.manaCrystal, { DoClick: true, PredicatePattern: patterns.shop.buy });
				macroService.PollPattern(patterns.shop.goldBuy, { DoClick: true, ClickOffset: { Y: 50 }, PredicatePattern: patterns.shop.manaCrystal.soldOut });
			}

			if (settings.doShop.buyClassEnhanceWithGold.Value) {
				macroService.PollPattern(patterns.shop.classEnhance, { DoClick: true, PredicatePattern: patterns.shop.buy });
				macroService.PollPattern(patterns.shop.goldBuy, { DoClick: true, ClickOffset: { Y: 50 }, PredicatePattern: patterns.shop.classEnhance.soldOut });
			}

			//if (settings.doShop.town.IsEnabled && !daily.doShop.town.done.IsChecked) {
			//	logger.info('doShop: town');

			//	macroService.PollPattern(patterns.shop.town, { DoClick: true, PredicatePattern: patterns.shop.town.selected });
			//	if (settings.doShop.town.ashOfProgress.Value && !daily.doShop.town.ashOfProgress.IsChecked) {
			//		let itemResult = findShopItem(/ash.*progress/is);
			//		if (!itemResult) {
			//			throw new Error('Could not find ash of progress.');
			//		}

			//		macroService.PollPoint(itemResult.point, { DoClick: true, PredicatePattern: patterns.shop.townBuy });
			//		macroService.PollPattern(patterns.shop.buy, { DoClick: true, PredicatePattern: patterns.general.back });

			//		macroService.IsRunning && (daily.doShop.town.ashOfProgress.IsChecked = true);
			//	}

			//	macroService.IsRunning && (daily.doShop.town.done.IsChecked = true);
			//}

			if (settings.doShop.doArtifact.Value && !daily.doShop.doArtifact.done.IsChecked) {
				logger.info('doShop: artifact');

				macroService.PollPattern(patterns.shop.artifact, { DoClick: true, PredicatePattern: patterns.shop.artifact.selected });
				macroService.DoSwipe({ X: 1600, Y: 850 }, { X: 1600, Y: 300 });
				sleep(2_000);
				macroService.DoSwipe({ X: 1600, Y: 850 }, { X: 1600, Y: 300 });
				sleep(2_000);

				const cost1500Result = macroService.FindPattern(patterns.shop.artifact.cost1500, { Limit: 4 });

				for (const p of cost1500Result.Points) {
					let shopBuyResult = { IsSuccess: false };
					while (macroService.IsRunning && !shopBuyResult.IsSuccess) {
						macroService.DoClick(p);
						sleep(1_000);
						shopBuyResult = macroService.FindPattern(patterns.shop.buy);
						sleep(1_000);
					}
					macroService.PollPattern(patterns.shop.artifact.buy, { DoClick: true, ClickOffset: { Y: 50 }, PredicatePattern: patterns.general.back });
				}

				//for (let i = 0; i < 4; i++) {
				//	macroService.DoSwipe({ X: 1600, Y: 850 }, { X: 1600, Y: 300 });
				//	sleep(2_000);

				//	const cost1500Result = macroService.PollPattern(patterns.shop.artifact.cost1500);
				//	let shopBuyResult = { IsSuccess: false };
				//	while (macroService.IsRunning && !shopBuyResult.IsSuccess) {
				//		macroService.DoClick(cost1500Result.Point);
				//		sleep(1_000);
				//		shopBuyResult = macroService.FindPattern(patterns.shop.buy);
				//		sleep(1_000);
				//	}
				//	macroService.PollPattern(patterns.shop.artifact.buy, { DoClick: true, ClickOffset: { Y: 50 }, PredicatePattern: patterns.general.back });

				//	sleep(1_000);
				//}

				if (macroService.IsRunning) {
					daily.doShop.doArtifact.done.IsChecked = true;
				}
			}
			
			if (settings.doShop.evilSoulShop.advancedKeepsakeEnhanceStone.Value && !daily.doShop.evilSoulShop.advancedKeepsakeEnhanceStone.IsChecked) {
				logger.info('doShop: evilSoulShop advancedKeepsakeEnhanceStone');
				const evilSoulShopSwipe = macroService.SwipePollPattern(patterns.shop.evilSoulShop, { Start: { X: 100, Y: 650 }, End: { X: 100, Y: 200 } });
				if (!evilSoulShopSwipe.IsSuccess) {
					throw new Error('Unable to find evil soul shop');
				}
				macroService.PollPattern(patterns.shop.evilSoulShop, { DoClick: true, PredicatePattern: patterns.shop.evilSoulShop.selected });
				sleep(1_000);

				const advancedKeepsakeEnhanceStoneResult = macroService.FindPattern(patterns.shop.evilSoulShop.advancedKeepsakeEnhanceStone, { Limit: 2 });

				for (const p of advancedKeepsakeEnhanceStoneResult.Points) {
					let shopBuyResult = { IsSuccess: false };
					while (macroService.IsRunning && !shopBuyResult.IsSuccess) {
						macroService.DoClick(p);
						sleep(1_000);
						shopBuyResult = macroService.FindPattern(patterns.shop.buy);
						sleep(1_000);
					}
					macroService.PollPattern(patterns.shop.evilSoulShop.buy, { DoClick: true, ClickOffset: { Y: 50 }, PredicatePattern: patterns.general.back });
				}
				sleep(1_000);

				if (macroService.IsRunning) {
					daily.doShop.evilSoulShop.advancedKeepsakeEnhanceStone.IsChecked = true;
				}
			}

			logger.info('doShop: done');
			if (macroService.IsRunning) {
				daily.doShop.done.IsChecked = true;
			}

			return;
	}

	sleep(1_000);
}

function findShopItem(itemRegex) {
	const itemCornerResult = macroService.FindPattern(patterns.shop.itemCorner, { Limit: 8 });

	let textResults = itemCornerResult.Points.map(p => {
		var rect = { X: p.X + 5, Y: p.Y - 340, Width: 225, Height: 200 };
		return {
			point: { X: p.X, Y: p.Y },
			text: macroService.FindTextWithBounds(rect)
		};
	});

	let itemResult = textResults.find(tr => tr.text.match(itemRegex));
	return itemResult;
}