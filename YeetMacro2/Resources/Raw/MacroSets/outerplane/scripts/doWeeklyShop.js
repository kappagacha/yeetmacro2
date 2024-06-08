// @position=2
// Shop weekly items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const weekly = weeklyManager.GetCurrentWeekly();
const resolution = macroService.GetCurrentResolution();
if (weekly.doWeeklyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

const swipeLeftEndX = resolution.Width - 100;
const swipeLeftStartX = swipeLeftEndX - 500;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doWeeklyShop: click shop tab');
			macroService.ClickPattern(patterns.tabs.shop);
			sleep(500);
			break;
		case 'titles.shop':
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
	sleep(1000);
	doShopItems('doWeeklyShop', 'friendshipPoint', friendshipItems, true);
}

function doArenaItems() {
	const arenaItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.resource.arena, { DoClick: true, PredicatePattern: patterns.shop.resource.arena.currency });
	sleep(1000);
	doShopItems('doWeeklyShop', 'arena', arenaItems, true);
}

function doStarMemoryItems() {
	const starMemoryItems = ['intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.resource.starMemory, { DoClick: true, PredicatePattern: patterns.shop.resource.starMemory.currency });
	sleep(1000);
	doShopItems('doWeeklyShop', 'starMemory', starMemoryItems, true);
}

function doSurveyHubItems() {
	const season1surveyHubItems = ['epicReforgeCatalyst1', 'epicReforgeCatalyst2', 'epicReforgeCatalyst3', 'epicReforgeCatalyst4', 'epicReforgeCatalyst5', 'superiorQualityPresentChest', 'basicSkillManual', 'intermediateSkillManual', '10pctLegendaryAbrasive'];
	doShopItems('doWeeklyShop', 'surveyHub', season1surveyHubItems, true);
	const season2Result = macroService.PollPattern(patterns.surveyHub.surveyHubItems.season2, { DoClick: true, PredicatePattern: patterns.surveyHub.surveyHubItems.season2.enabled, TimeoutMs: 3_000 });
	if (!season2Result.IsSuccess) {
		return;
	}
	sleep(1000);
	swipeLeft();
	swipeLeft();
	swipeLeft();
	const season2surveyHubItems = ['stage2RandomGemChest', 'legendaryReforgeCatalyst', 'epicQualityPresentChest', 'professionalSkillManual', 'refinedGlunite'];
	doShopItems('doWeeklyShop', 'surveyHub', season2surveyHubItems, true);
}

function doGuildItems() {
	macroService.PollPattern(patterns.guild.shop.weeklyProducts, { DoClick: true, PredicatePattern: patterns.guild.shop.weeklyProducts.selected });
	sleep(1_000);
	const guildItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	doShopItems('doWeeklyShop', 'guild', guildItems, true);
}