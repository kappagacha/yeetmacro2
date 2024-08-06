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

			if (settings.doShop.doArtifact.Value && !daily.doShop.doArtifact.done.IsChecked) {
				logger.info('doShop: artifact');
				macroService.PollPattern(patterns.shop.artifact, { DoClick: true, PredicatePattern: patterns.shop.artifact.selected });
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
				sleep(1_000);
				if (macroService.IsRunning) {
					daily.doShop.doArtifact.done.IsChecked = true;
				}
			}
			
			if (settings.doShop.evilSoulShop.advancedKeepsakeEnhanceStone.Value && !daily.doShop.evilSoulShop.advancedKeepsakeEnhanceStone.IsChecked) {
				logger.info('doShop: evilSoulShop advancedKeepsakeEnhanceStone');
				const evilSoulShopSwipe = macroService.SwipePollPattern(patterns.shop.evilSoulShop, { Start: { X: 100, Y: 650 }, End: { X: 100, Y: 200 } });
				if (!evilSoulShopSwipe.IsSuccess) {
					throw new Error('Unable to find regular pack');
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