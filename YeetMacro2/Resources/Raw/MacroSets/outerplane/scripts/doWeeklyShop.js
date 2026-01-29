// @position=-1
// @tags=weeklies
// Shop weekly items
const loopPatterns = [patterns.lobby.level, patterns.titles.adventurerShop];
const weekly = weeklyManager.GetCurrentWeekly();
if (weekly.doWeeklyShop.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doWeeklyShop: go to adventurerShop');
			macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.adventurer.move });
			macroService.PollPattern(patterns.shop.adventurer.move, { DoClick: true, PredicatePattern: patterns.titles.adventurerShop });
			sleep(500);
			break;
		case 'titles.adventurerShop':
			if (!weekly.doWeeklyShop.friendshipPoint.done.IsChecked) {
				macroService.PollPattern(patterns.shop.adventurer.friendshipPoint, { DoClick: true, PredicatePattern: patterns.shop.adventurer.friendshipPoint.selected });
				doFriendshipItems();
				swipeLeft();
				swipeLeft();
				if (macroService.IsRunning) weekly.doWeeklyShop.friendshipPoint.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.arena.done.IsChecked) {
				macroService.PollPattern(patterns.shop.adventurer.arena, { DoClick: true, PredicatePattern: patterns.shop.adventurer.arena.selected });
				macroService.PollPattern(patterns.shop.adventurer.arena.arena, { DoClick: true, PredicatePattern: patterns.shop.adventurer.arena.arena.selected });
				doArenaItems();
				swipeLeft();
				swipeLeft();
				if (macroService.IsRunning) weekly.doWeeklyShop.arena.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.starMemory.done.IsChecked) {
				macroService.PollPattern(patterns.shop.adventurer.starMemory, { DoClick: true, PredicatePattern: patterns.shop.adventurer.starMemory.selected });
				doStarMemoryItems();
				if (macroService.IsRunning) weekly.doWeeklyShop.starMemory.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.jointChallenge.done.IsChecked) {
				macroService.PollPattern(patterns.shop.adventurer.event, { DoClick: true, PredicatePattern: patterns.shop.adventurer.event.selected });
				const jointChallengeSwipeResult = macroService.PollPattern(patterns.shop.adventurer.event.jointChallenge, { SwipePattern: patterns.shop.subsubTabSwipeRight, TimeoutMs: 4_000 });
				if (!jointChallengeSwipeResult.IsSuccess) {
					throw new Error('Unable to find joint challenge');
				}
				macroService.PollPattern(patterns.shop.adventurer.event.jointChallenge, { DoClick: true, PredicatePattern: patterns.shop.adventurer.event.jointChallenge.selected });
				doJointChallengeItems();
				if (macroService.IsRunning) weekly.doWeeklyShop.jointChallenge.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.surveyHub.done.IsChecked) {
				macroService.PollPattern(patterns.shop.adventurer.surveyHub, { DoClick: true, PredicatePattern: patterns.shop.adventurer.surveyHub.selected });

				doSurveyHubItems();
				if (macroService.IsRunning) weekly.doWeeklyShop.surveyHub.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.resource.done.IsChecked) {
				const goldOrConsumablesResult = macroService.PollPattern(patterns.shop.adventurer.goldOrConsumables, { SwipePattern: patterns.shop.mainTabSwipeDown, TimeoutMs: 7_000 });
				if (!goldOrConsumablesResult.IsSuccess) {
					throw new Error('Unable to find skyward Gold/Consumables');
				}
				macroService.PollPattern(patterns.shop.adventurer.goldOrConsumables, { DoClick: true, PredicatePattern: patterns.shop.adventurer.goldOrConsumables.selected });

				doResourceItems();
				if (macroService.IsRunning) weekly.doWeeklyShop.resource.done.IsChecked = true;
			}

			if (!weekly.doWeeklyShop.guild.done.IsChecked) {
				goToLobby();
				macroService.PollPattern(patterns.tabs.guild, { DoClick: true, PredicatePattern: patterns.titles.guild });
				macroService.PollPattern(patterns.guild.shop, { DoClick: true, PredicatePattern: [patterns.guild.shop.weeklyProducts, patterns.guild.shop.weeklyProducts.selected] });
				doGuildItems();
				if (macroService.IsRunning) weekly.doWeeklyShop.guild.done.IsChecked = true;
			}

			if (macroService.IsRunning) weekly.doWeeklyShop.done.IsChecked = true;
			return;
	}
	sleep(1_000);
}

function swipeLeft() {
	macroService.SwipePattern(patterns.general.swipeLeft);
	sleep(1_500);
}

function doResourceItems() {
	const resourceItems = ['basicSkillManual', 'intermediateSkillManual'];
	doShopItems('doWeeklyShop', 'resource', resourceItems, true);
}

function doFriendshipItems() {
	const friendshipItems = ['threeStarHeroPieceTicket', 'upgradeStoneSelectionChest', 'lowStarHeroPieceTicket'];
	sleep(1000);
	doShopItems('doWeeklyShop', 'friendshipPoint', friendshipItems, true);
}

function doArenaItems() {
	const arenaItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	sleep(1000);
	doShopItems('doWeeklyShop', 'arena', arenaItems, true);
}

function doStarMemoryItems() {
	const starMemoryItems = ['intermediateSkillManual', 'professionalSkillManual'];
	doShopItems('doWeeklyShop', 'starMemory', starMemoryItems, true);
}

function doJointChallengeItems() {
	const jointChallengeItems = ['specialRecruitmentTicket', 'stage3GemChest', 'gold'];
	doShopItems('doWeeklyShop', 'jointChallenge', jointChallengeItems, true);
}

function doSurveyHubItems() {
	const surveyHubItems = ['30pctEpicAbrasive', 'epicReforgeCatalyst', 'superiorQualityPresentChest', 'basicSkillManual', 'intermediateSkillManual', '10pctLegendaryAbrasive',
		'stage2RandomGemChest', 'legendaryReforgeCatalyst', 'epicQualityPresentChest', 'professionalSkillManual', 'refinedGlunite'];
	doShopItems('doWeeklyShop', 'surveyHub', surveyHubItems, true);
}

function doGuildItems() {
	macroService.PollPattern(patterns.guild.shop.weeklyProducts, { DoClick: true, PredicatePattern: patterns.guild.shop.weeklyProducts.selected });
	sleep(1_000);
	const guildItems = ['gold', 'basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	doShopItems('doWeeklyShop', 'guild', guildItems, true);
}