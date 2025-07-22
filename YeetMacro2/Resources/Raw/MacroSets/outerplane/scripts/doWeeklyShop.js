// @position=-1
// Shop weekly items
const loopPatterns = [patterns.lobby.level, patterns.titles.shop];
const weekly = weeklyManager.GetCurrentWeekly();
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
			logger.info('doWeeklyShop: claim Resource');
			macroService.PollPattern(patterns.shop.resources, { DoClick: true, PredicatePattern: patterns.shop.resources.normal });

			if (!weekly.doWeeklyShop.normal.done.IsChecked) {
				doNormalItems();
				swipeLeft();
				swipeLeft();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.normal.done.IsChecked = true;
				}
			}

			macroService.PollPattern(patterns.shop.contents, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints });
			if (!weekly.doWeeklyShop.friendshipPoint.done.IsChecked) {
				doFriendshipItems();
				swipeLeft();
				swipeLeft();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.friendshipPoint.done.IsChecked = true;
				}
			}

			if (!weekly.doWeeklyShop.arena.done.IsChecked) {
				doArenaItems();
				swipeLeft();
				swipeLeft();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.arena.done.IsChecked = true;
				}
			}

			if (!weekly.doWeeklyShop.starMemory.done.IsChecked) {
				doStarMemoryItems();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.starMemory.done.IsChecked = true;
				}
			}

			if (!weekly.doWeeklyShop.jointChallenge.done.IsChecked) {
				doJointChallengeItems();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.jointChallenge.done.IsChecked = true;
				}
			}

			if (!weekly.doWeeklyShop.surveyHub.done.IsChecked) {
				const surveyHubSwipeResult = macroService.PollPattern(patterns.shop.contents.surveyHub, { SwipePattern: patterns.shop.subTabSwipeDown, TimeoutMs: 7_000 });
				if (!surveyHubSwipeResult.IsSuccess) {
					throw new Error('Unable to find survey hub');
				}
				macroService.PollPattern(patterns.shop.contents.surveyHub, { DoClick: true, PredicatePattern: patterns.shop.contents.surveyHub.selected });

				doSurveyHubItems();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.surveyHub.done.IsChecked = true;
				}
			}
			
			if (!weekly.doWeeklyShop.guild.done.IsChecked) {
				goToLobby();
				macroService.PollPattern(patterns.tabs.guild, { DoClick: true, PredicatePattern: patterns.titles.guild });
				macroService.PollPattern(patterns.guild.shop, { DoClick: true, PredicatePattern: [patterns.guild.shop.weeklyProducts, patterns.guild.shop.weeklyProducts.selected] });
				doGuildItems();
				if (macroService.IsRunning) {
					weekly.doWeeklyShop.guild.done.IsChecked = true;
				}
			}

			if (macroService.IsRunning) {
				weekly.doWeeklyShop.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function swipeLeft() {
	macroService.SwipePattern(patterns.general.swipeLeft);
	sleep(1_500);
}

function doNormalItems() {
	const normalItems = ['basicSkillManual', 'intermediateSkillManual'];
	macroService.PollPattern(patterns.shop.resources.resources, { DoClick: true, PredicatePattern: patterns.shop.resources.resources.selected });
	doShopItems('doWeeklyShop', 'normal', normalItems, true);
}

function doFriendshipItems() {
	const friendshipItems = ['threeStarHeroPieceTicket', 'upgradeStoneSelectionChest', 'lowStarHeroPieceTicket'];
	macroService.PollPattern(patterns.shop.contents.friendshipPoints, { DoClick: true, PredicatePattern: patterns.shop.contents.friendshipPoints.selected });
	sleep(1000);
	doShopItems('doWeeklyShop', 'friendshipPoint', friendshipItems, true);
}

function doArenaItems() {
	const arenaItems = ['basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.contents.arena, { DoClick: true, PredicatePattern: patterns.shop.contents.arena.selected });
	sleep(1000);
	doShopItems('doWeeklyShop', 'arena', arenaItems, true);
}

function doStarMemoryItems() {
	const starMemoryItems = ['intermediateSkillManual', 'professionalSkillManual'];
	macroService.PollPattern(patterns.shop.contents.starMemory, { DoClick: true, PredicatePattern: patterns.shop.contents.starMemory.selected });
	sleep(1000);
	doShopItems('doWeeklyShop', 'starMemory', starMemoryItems, true);
}

function doJointChallengeItems() {
	const jointChallengeItems = ['specialRecruitmentTicket', 'stage3GemChest', 'gold'];
	macroService.PollPattern(patterns.shop.contents.event, { DoClick: true, PredicatePattern: patterns.shop.contents.event.selected });
	macroService.PollPattern(patterns.shop.contents.event.jointChallenge, { DoClick: true, PredicatePattern: patterns.shop.contents.event.jointChallenge.selected });

	doShopItems('doWeeklyShop', 'jointChallenge', jointChallengeItems, true);
}

function doSurveyHubItems() {
	const season1surveyHubItems = ['30pctEpicAbrasive', 'epicReforgeCatalyst', 'superiorQualityPresentChest', 'basicSkillManual', 'intermediateSkillManual', '10pctLegendaryAbrasive'];
	doShopItems('doWeeklyShop', 'surveyHub', season1surveyHubItems, true);
	//const season2Result = macroService.PollPattern(patterns.surveyHub.surveyHubItems.season2, { DoClick: true, PredicatePattern: patterns.surveyHub.surveyHubItems.season2.enabled, TimeoutMs: 3_000 });
	//if (!season2Result.IsSuccess) {
	//	return;
	//}
	sleep(1000);
	//swipeLeft();
	//swipeLeft();
	//swipeLeft();
	const season2surveyHubItems = ['stage2RandomGemChest', 'legendaryReforgeCatalyst', 'epicQualityPresentChest', 'professionalSkillManual', 'refinedGlunite'];
	doShopItems('doWeeklyShop', 'surveyHub', season2surveyHubItems, true);
}

function doGuildItems() {
	macroService.PollPattern(patterns.guild.shop.weeklyProducts, { DoClick: true, PredicatePattern: patterns.guild.shop.weeklyProducts.selected });
	sleep(1_000);
	const guildItems = ['gold', 'basicSkillManual', 'intermediateSkillManual', 'professionalSkillManual'];
	doShopItems('doWeeklyShop', 'guild', guildItems, true);
}