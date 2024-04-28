// @position=2
// Shop weekly items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const weekly = weeklyManager.GetCurrentWeekly();
const resolution = macroService.GetCurrentResolution();
if (weekly.doWeeklyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doWeeklyShop: click shop tab');
			macroService.ClickPattern(patterns.tabs.shop);
			sleep(500);
			break;
		case 'titles.shop':
			logger.info('doWeeklyShop: claim Normal');
			macroService.PollPattern(patterns.shop.normal, { DoClick: true, PredicatePattern: patterns.shop.normal.selected });
			macroService.PollPattern(patterns.shop.normal.weekly, { DoClick: true, PredicatePattern: patterns.shop.normal.weekly.selected });
			const normalFreeResult = macroService.PollPattern(patterns.shop.normal.free, { TimeoutMs: 2_000 });
			if (normalFreeResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.normal.free, { DoClick: true, PredicatePattern: patterns.shop.normal.free.confirm });
				macroService.PollPattern(patterns.shop.normal.free.confirm, { DoClick: true, InversePredicatePattern: patterns.shop.normal.free.confirm });
			}

			logger.info('doWeeklyShop: claim Resource');
			const swipeResult = macroService.SwipePollPattern(patterns.shop.resource, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find resource shop');
			}
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.resource.selected, { CenterY: swipeResult.Point.Y, Padding: 10 });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			doNormalItems();
			doFriendshipItems();
			doArenaItems();
			doStarMemoryItems();
			const swipeResult2 = macroService.SwipePollPattern(patterns.shop.shopList, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult2.IsSuccess) {
				throw new Error('Unable to find shop list');
			}
			macroService.PollPattern(patterns.shop.shopList, { DoClick: true, PredicatePattern: patterns.shop.shopList.guildShop });
			macroService.PollPattern(patterns.shop.shopList.guildShop, { DoClick: true, PredicatePattern: patterns.shop.shopList.guildShop.go });
			macroService.PollPattern(patterns.shop.shopList.guildShop.go, { DoClick: true, PredicatePattern: [patterns.guild.shop.weeklyProducts, patterns.guild.shop.weeklyProducts.selected] });
			doGuildItems();

			if (macroService.IsRunning) {
				weekly.doWeeklyShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function doNormalItems() {
	const startX = resolution.Width - 100;
	const endX = startX - 300;
	macroService.DoSwipe({ X: startX, Y: 500 }, { X: endX, Y: 500 });
	sleep(1_500);
	//const normalItems = ['gold', 'basicSkillManual', 'intermediateSkillManual'];
	const normalItems = ['basicSkillManual', 'intermediateSkillManual'];
	for (const normalItem of normalItems) {
		if (settings.doWeeklyShop.normalItems[normalItem].Value && !weekly.doWeeklyShop.normalItems[normalItem].IsChecked) {
			logger.info(`doWeeklyShop: purchase normal item ${normalItem}`);
			const normalItemPattern = macroService.ClonePattern(patterns.shop.resource.normal[normalItem], {
				X: 350,
				Y: 230,
				Width: resolution.Width - 550,
				Height: 800,
				Path: `patterns.shop.resource.normal.${normalItem}`,
				OffsetCalcType: 'DockLeft'
			});
			const normalItemResult = macroService.PollPattern(normalItemPattern);
			const normalItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: normalItemResult.Point.X - 70,
				Y: normalItemResult.Point.Y + 200,
				Width: 350,
				Height: 100,
				Path: `patterns.shop.resource.normal.${normalItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(normalItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				weekly.doWeeklyShop.normalItems[normalItem].IsChecked = true;
			}
		}
	}
}

function doFriendshipItems() {
	const endX = resolution.Width - 100;
	const startX = endX - 300;
	macroService.DoSwipe({ X: startX, Y: 500 }, { X: endX, Y: 500 });
	const friendshipItems = ['upgradeStoneSelectionChest', 'lowStarHeroPieceTicket'];
	for (const friendshipItem of friendshipItems) {
		if (settings.doWeeklyShop.useFriendshipPoints[friendshipItem].Value && !weekly.doWeeklyShop.useFriendshipPoints[friendshipItem].IsChecked) {
			logger.info(`doWeeklyShop: purchase friendshipItem ${friendshipItem}`);
			macroService.PollPattern(patterns.shop.resource.friendship, { DoClick: true, PredicatePattern: patterns.shop.resource.friendship.currency });

			const friendshipItemPattern = macroService.ClonePattern(patterns.shop.resource.friendship[friendshipItem], {
				X: 350,
				Y: 230,
				Width: resolution.Width - 550,
				Height: 800,
				Path: `patterns.shop.resource.friendship.${friendshipItem}`,
				OffsetCalcType: 'DockLeft'
			});
			const friendshipItemResult = macroService.PollPattern(friendshipItemPattern);
			const friendshipItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: friendshipItemResult.Point.X - 70,
				Y: friendshipItemResult.Point.Y + 200,
				Width: 350,
				Height: 100,
				Path: `patterns.shop.resource.friendship.${friendshipItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(friendshipItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				weekly.doWeeklyShop.useFriendshipPoints[friendshipItem].IsChecked = true;
			}
		}
	}
}

function doArenaItems() {
	const arenaItems = ['basicSkillManual', 'intermediateSkilManual', 'professionalSkillManual'];
	for (const arenaItem of arenaItems) {
		if (settings.doWeeklyShop.useArenaMedals[arenaItem].Value && !weekly.doWeeklyShop.useArenaMedals[arenaItem].IsChecked) {
			logger.info(`doWeeklyShop: purchase arenaItem ${arenaItem}`);
			macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });

			const arenaItemPattern = macroService.ClonePattern(patterns.shop.resource.arena[arenaItem], {
				X: 350,
				Y: 230,
				Width: resolution.Width - 550,
				Height: 800,
				Path: `patterns.shop.resource.arena.${arenaItem}`,
				OffsetCalcType: 'DockLeft'
			});
			const arenaItemResult = macroService.PollPattern(arenaItemPattern);
			const arenaItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: arenaItemResult.Point.X - 70,
				Y: arenaItemResult.Point.Y + 200,
				Width: 350,
				Height: 100,
				Path: `patterns.shop.resource.arena.${arenaItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(arenaItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				weekly.doWeeklyShop.useArenaMedals[arenaItem].IsChecked = true;
			}
		}
	}
}

function doStarMemoryItems() {
	const starMemoryItems = ['intermediateSkilManual', 'professionalSkillManual'];
	for (const starMemoryItem of starMemoryItems) {
		if (settings.doWeeklyShop.useStarMemory[starMemoryItem].Value && !weekly.doWeeklyShop.useStarMemory[starMemoryItem].IsChecked) {
			logger.info(`doWeeklyShop: purchase starMemoryItem ${starMemoryItem}`);
			macroService.PollPattern(patterns.shop.resource.starMemory, { DoClick: true, PredicatePattern: patterns.shop.resource.starMemory.currency });

			const starMemoryItemPattern = macroService.ClonePattern(patterns.shop.resource.starMemory[starMemoryItem], {
				X: 350,
				Y: 230,
				Width: resolution.Width - 550,
				Height: 800,
				Path: `patterns.shop.resource.starMemory.${starMemoryItem}`,
				OffsetCalcType: 'DockLeft'
			});
			const starMemoryItemResult = macroService.PollPattern(starMemoryItemPattern);
			const starMemoryItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: starMemoryItemResult.Point.X - 70,
				Y: starMemoryItemResult.Point.Y + 200,
				Width: 350,
				Height: 100,
				Path: `patterns.shop.resource.starMemory.${starMemoryItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(starMemoryItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				weekly.doWeeklyShop.useStarMemory[starMemoryItem].IsChecked = true;
			}
		}
	}
}

function doGuildItems() {
	const resolution = macroService.GetCurrentResolution();
	const weekly = weeklyManager.GetCurrentWeekly();
	macroService.PollPattern(patterns.guild.shop.weeklyProducts, { DoClick: true, PredicatePattern: patterns.guild.shop.weeklyProducts.selected });
	const startX = resolution.Width - 100;
	const endX = startX - 300;
	macroService.DoSwipe({ X: startX, Y: 500 }, { X: endX, Y: 500 });
	const guildItems = ['basicSkillManual', 'intermediateSkilManual', 'professionalSkillManual'];
	for (const guildItem of guildItems) {
		if (settings.doWeeklyShop.useGuildMedals[guildItem].Value && !weekly.doWeeklyShop.useGuildMedals[guildItem].IsChecked) {
			logger.info(`doWeeklyShop: purchase guildItem ${guildItem}`);

			const guildItemPattern = macroService.ClonePattern(patterns.guild.shop.weeklyProducts[guildItem], {
				X: 350,
				Y: 190,
				Width: resolution.Width - 550,
				Height: 800,
				Path: `patterns.guild.shop.weeklyProducts.${guildItem}`,
				OffsetCalcType: 'DockLeft'
			});
			const guildItemResult = macroService.PollPattern(guildItemPattern);
			const guildItemPurchasePattern = macroService.ClonePattern(patterns.shop.resource.purchase, {
				X: guildItemResult.Point.X - 70,
				Y: guildItemResult.Point.Y + 200,
				Width: 350,
				Height: 100,
				Path: `patterns.guild.shop.weeklyProducts.${guildItem}.purchase`,
				OffsetCalcType: 'None'
			});

			macroService.PollPattern(guildItemPurchasePattern, { DoClick: true, PredicatePattern: patterns.shop.resource.ok });
			const maxSliderResult = macroService.FindPattern(patterns.shop.resource.maxSlider);
			if (maxSliderResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.resource.maxSlider, { DoClick: true, InversePredicatePattern: patterns.shop.resource.maxSlider });
			}
			macroService.PollPattern(patterns.shop.resource.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.shop });
			if (macroService.IsRunning) {
				weekly.doWeeklyShop.useGuildMedals[guildItem].IsChecked = true;
			}
		}
	}
}