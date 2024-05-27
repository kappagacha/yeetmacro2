// @position=2
// Shop weekly items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const weekly = weeklyManager.GetCurrentWeekly();
const resolution = macroService.GetCurrentResolution();
if (weekly.doWeeklyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

const swipeLeftEndX = resolution.Width - 100;
const swipeLeftStartX = swipeLeftEndX - 300;

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
			const selectedResourcePattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: swipeResult.Point.Y, Padding: 20 });
			macroService.PollPattern(patterns.shop.resource, { DoClick: true, PredicatePattern: selectedResourcePattern });

			doNormalItems();

			doFriendshipItems();
			swipeLeft();

			doArenaItems();
			swipeLeft();

			doStarMemoryItems();

			const swipeResult2 = macroService.SwipePollPattern(patterns.shop.surveyHub, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult2.IsSuccess) {
				throw new Error('Unable to find surveyhub shop');
			}

			const selectedSurveyHubPattern = macroService.ClonePattern(patterns.shop.selected, { CenterY: swipeResult2.Point.Y, Padding: 20 });
			macroService.PollPattern(patterns.shop.surveyHub, { DoClick: true, PredicatePattern: selectedSurveyHubPattern });
			doSurveyHubItems();


			const swipeResult3 = macroService.SwipePollPattern(patterns.shop.shopList, { MaxSwipes: 2, Start: { X: 180, Y: 650 }, End: { X: 180, Y: 250 } });
			if (!swipeResult3.IsSuccess) {
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

function swipeLeft() {
	macroService.DoSwipe({ X: swipeLeftStartX, Y: 500 }, { X: swipeLeftEndX, Y: 500 });
	sleep(1_500);
}

function doNormalItems() {
	const normalItems = ['basicSkillManual', 'intermediateSkillManual'];
	doShopItems('doWeeklyShop', 'normal', normalItems, true);
}

function doFriendshipItems() {
	const friendshipItems = ['upgradeStoneSelectionChest', 'lowStarHeroPieceTicket'];
	macroService.PollPattern(patterns.shop.resource.friendship, { DoClick: true, PredicatePattern: patterns.shop.resource.friendship.currency });
	doShopItems('doWeeklyShop', 'friendshipPoint', friendshipItems, true);
}

function doArenaItems() {
	const arenaItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });
	doShopItems('doWeeklyShop', 'arena', arenaItems, true);
}

function doStarMemoryItems() {
	const starMemoryItems = ['intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.resource.starMemory, { DoClick: true, PredicatePattern: patterns.shop.resource.starMemory.currency });
	doShopItems('doWeeklyShop', 'starMemory', starMemoryItems, true);
}

function doSurveyHubItems() {
	const season1surveyHubItems = ['epicReforgeCatalyst', 'superiorQualityPresentChest', 'basicSkillManual', 'intermediateSkillManual', '10pctLegendaryAbrasive'];
	doShopItems('doWeeklyShop', 'surveyHub', season1surveyHubItems, true);
	macroService.PollPattern(patterns.surveyHub.surveyHubItems.season2, { DoClick: true, PredicatePattern: patterns.surveyHub.surveyHubItems.season2.enabled });
	const season2surveyHubItems = ['legendaryReforgeCatalyst', 'epicQualityPresentChest', 'professionalSkillManual', 'refinedGlunite'];
	doShopItems('doWeeklyShop', 'surveyHub', season2surveyHubItems, true);
}

function doGuildItems() {
	macroService.PollPattern(patterns.guild.shop.weeklyProducts, { DoClick: true, PredicatePattern: patterns.guild.shop.weeklyProducts.selected });
	sleep(1_000);
	const guildItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	doShopItems('doWeeklyShop', 'guild', guildItems, true);
}