// @raw-script
// @tags=weeklies
// @position=-1

function weeklies(type = '') {
	if (!type) type = settings.weeklies.type.Value;

	switch (type) {
		case 'weeklies':
			if (settings.doWeeklies.claimMissions.Value) {
				claimWeeklyMissions();
				goToLobby();
			}

			if (settings.doWeeklies.craft.Value) {
				doWeeklyCraft();
				goToLobby();
			}

			if (settings.doWeeklies.shop.Value) {
				doWeeklyShop();
				goToLobby();
			}
			break;
		case 'claimWeeklyMissions':
			doClaimWeeklyMissions();
			break;
		case 'craft':
			doWeeklyCraft();
			break;
		case 'shop':
			doWeeklyShop();
			break;
		case 'adventureLicence':
			doAdventureLicense();
			break;
		case 'monadGate':
			doNormalObservation();
			break;
	}
}

function doClaimWeeklyMissions() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.mission];
	const weekly = weeklyManager.GetCurrentWeekly();
	const dayOfWeek = weeklyManager.GetDayOfWeek();

	// dayOfWeek is UTC which is a day forward; 0 is UTC Sunday, which is local Saturday
	if (dayOfWeek !== 0 && dayOfWeek < 3) return;

	if (weekly.claimMissions.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('claimWeeklyMissions: click mission');
				macroService.ClickPattern(patterns.tabs.mission);
				sleep(500);
				break;
			case 'titles.mission':
				logger.info('claimWeeklyMissions: claim final reward');
				macroService.PollPattern(patterns.mission.weekly, { DoClick: true, PredicatePattern: patterns.mission.weekly.selected });
				const finalNotificationResult = macroService.PollPattern(patterns.mission.finalNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				if (!finalNotificationResult.IsSuccess) {
					return;
				}
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mission });
				if (macroService.IsRunning) {
					weekly.claimMissions.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function doWeeklyCraft() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.base, patterns.titles.katesWorkshop];
	const weekly = weeklyManager.GetCurrentWeekly();
	if (weekly.doWeeklyCraft.done.IsChecked) {
		return "Script already completed. Uncheck done to override weekly flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doWeeklyCraft: click base tab');
				macroService.ClickPattern(patterns.tabs.base);
				break;
			case 'titles.base':
				logger.info('doWeeklyCraft: click kate\'s workshop');
				macroService.ClickPattern(patterns.base.katesWorkshop);
				break;
			case 'titles.katesWorkshop':
				logger.info('doWeeklyCraft: click craft consumable');
				macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable, { DoClick: true, ClickOffset: { X: 100, Y: -200 }, PredicatePattern: patterns.base.katesWorkshop.craft });
				macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.selected });

				macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.armorGlunite, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.armorGlunite.selected });
				if (settings.doWeeklyCraft.specialEnhancement.armorGlunite.Value && !weekly.doWeeklyCraft.specialEnhancement.armorGlunite.IsChecked) {
					for (let i = 0; i < 2; i++) {
						macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.katesWorkshop.craft, patterns.base.katesWorkshop.craft.disabled] });
					}
					macroService.IsRunning && (weekly.doWeeklyCraft.specialEnhancement.armorGlunite.IsChecked = true);
				}

				if (settings.doWeeklyCraft.specialEnhancement.specialGearEnhancement.talisman.Value && !weekly.doWeeklyCraft.specialEnhancement.specialGearEnhancement.talisman.IsChecked) {
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.specialGearEnhancement, { SwipePattern: patterns.base.katesWorkshop.tabsSwipeRight });
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.specialGearEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.specialGearEnhancement.selected });
					for (let i = 0; i < 4; i++) {
						macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.katesWorkshop.craft, patterns.base.katesWorkshop.craft.disabled] });
					}
					macroService.IsRunning && (weekly.doWeeklyCraft.specialEnhancement.specialGearEnhancement.talisman.IsChecked = true);
				}

				if (settings.doWeeklyCraft.specialEnhancement.specialGearEnhancement.gear.Value && !weekly.doWeeklyCraft.specialEnhancement.specialGearEnhancement.gear.IsChecked) {
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.memoryStone, { SwipePattern: patterns.base.katesWorkshop.tabsSwipeRight });
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.memoryStone, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.memoryStone.selected });
					for (let i = 0; i < 4; i++) {
						macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
						macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.katesWorkshop.craft, patterns.base.katesWorkshop.craft.disabled] });
					}
					macroService.IsRunning && (weekly.doWeeklyCraft.specialEnhancement.specialGearEnhancement.gear.IsChecked = true);
				}

				if (settings.doWeeklyCraft.specialEnhancement.greaterEnhancement.talisman.Value && !weekly.doWeeklyCraft.specialEnhancement.greaterEnhancement.talisman.IsChecked) {
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement, { SwipePattern: patterns.base.katesWorkshop.tabsSwipeRight });
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement.selected });
					macroService.PollPattern(patterns.base.katesWorkshop.talisman, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.talisman.selected });
					macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.katesWorkshop.craft, patterns.base.katesWorkshop.craft.disabled] });
					macroService.IsRunning && (weekly.doWeeklyCraft.specialEnhancement.greaterEnhancement.talisman.IsChecked = true);
				}

				if (settings.doWeeklyCraft.specialEnhancement.greaterEnhancement.gear.Value && !weekly.doWeeklyCraft.specialEnhancement.greaterEnhancement.gear.IsChecked) {
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement2, { SwipePattern: patterns.base.katesWorkshop.tabsSwipeRight });
					macroService.PollPattern(patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement2, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.craftConsumable.specialEnhancement.greaterEnhancement2.selected });
					macroService.PollPattern(patterns.base.katesWorkshop.gear, { DoClick: true, PredicatePattern: patterns.base.katesWorkshop.gear.selected });
					macroService.PollPattern(patterns.base.katesWorkshop.craft, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.base.katesWorkshop.craft, patterns.base.katesWorkshop.craft.disabled] });
					macroService.IsRunning && (weekly.doWeeklyCraft.specialEnhancement.greaterEnhancement.gear.IsChecked = true);
				}

				macroService.IsRunning && (weekly.doWeeklyCraft.done.IsChecked = true);
				return;
		}
		sleep(1_000);
	}
}

function doWeeklyShop() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventurerShop, patterns.shop.premium.title];
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
			case 'shop.premium.title':
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

				if (!weekly.doWeeklyShop.jointChallenge.done.IsChecked) {
					macroService.PollPattern(patterns.shop.adventurer.event, { DoClick: true, PredicatePattern: patterns.shop.adventurer.event.selected });
					const jointChallengeSwipeResult = macroService.PollPattern(patterns.shop.adventurer.event.jointChallenge, { SwipePattern: patterns.shop.subsubTabSwipeRight, TimeoutMs: 6_000 });
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

				if (!weekly.doWeeklyShop.starMemory.done.IsChecked) {
					goToLobby();
					macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.premium.move });
					macroService.PollPattern(patterns.shop.premium.move, { DoClick: true, PredicatePattern: patterns.shop.premium.title });
					macroService.PollPattern(patterns.shop.premium.normal, { DoClick: true, PredicatePattern: patterns.shop.premium.normal.selected });
					macroService.PollPattern(patterns.shop.premium.normal.starMemory, { DoClick: true, PredicatePattern: patterns.shop.premium.normal.starMemory.selected });
					doStarMemoryItems();
					if (macroService.IsRunning) weekly.doWeeklyShop.starMemory.done.IsChecked = true;
					goToLobby();
				}

				if (!weekly.doWeeklyShop.resource.done.IsChecked) {
					goToLobby();
					macroService.PollPattern(patterns.tabs.shop, { DoClick: true, PredicatePattern: patterns.shop.premium.move });
					macroService.PollPattern(patterns.shop.premium.move, { DoClick: true, PredicatePattern: patterns.shop.premium.title });
					macroService.PollPattern(patterns.shop.premium.normal, { DoClick: true, PredicatePattern: patterns.shop.premium.normal.selected });
					macroService.PollPattern(patterns.shop.premium.normal.goldOrConsumables, { DoClick: true, PredicatePattern: patterns.shop.premium.normal.goldOrConsumables.selected });
					doResourceItems();
					if (macroService.IsRunning) weekly.doWeeklyShop.resource.done.IsChecked = true;
					goToLobby();
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
}

function doAdventureLicense() {
	const loopPatterns = [patterns.lobby.level, patterns.adventure.adventureLicense.title, patterns.adventure.adventureLicense.weeklyConquest.selected, patterns.titles.adventure];

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doAdventureLicense: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doAdventureLicense: click adventure license');
				macroService.ClickPattern(patterns.adventure.adventureLicense);
				sleep(500);
				break;
			case 'adventure.adventureLicense.title':
			case 'adventure.adventureLicense.weeklyConquest.selected':
				logger.info('doAdventureLicense: do weekly conquest');
				macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.selected });

				macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.wanted);
				sleep(1_000);
				let incompleteWanted = findIncompleteWanted();

				if (!incompleteWanted) {
					macroService.SwipePattern(patterns.adventure.adventureLicense.weeklyConquest.swipeRight);
					sleep(3_500);
					incompleteWanted = findIncompleteWanted();
				}

				if (!incompleteWanted) {
					throw Error('Could not find wanted that is not complete');
				}
				macroService.PollPoint(incompleteWanted, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.heroDeployment });
				macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });

				try {
					selectTeamGeneral();
				} catch (err) {
					if (err.message.includes('Could not find')) {
						return err.message;
					}
					throw err;
				}

				macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.next });
				macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.next, { DoClick: true, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.exit });
				macroService.PollPattern(patterns.adventure.adventureLicense.weeklyConquest.exit, { DoClick: true, ClickPattern: patterns.adventure.adventureLicense.weeklyConquest.ok, PredicatePattern: patterns.adventure.adventureLicense.weeklyConquest.selected });
				sleep(3_000);
		}
		sleep(1_000);
	}

	function findIncompleteWanted() {
		const wantedResults = macroService.FindPattern(patterns.adventure.adventureLicense.weeklyConquest.wanted, { Limit: 4 });
		for (const p of wantedResults.Points.sort((a, b) => a.X - b.X)) {
			const completedPattern = macroService.ClonePattern(patterns.adventure.adventureLicense.weeklyConquest.completed,
				{ CenterX: p.X - 60, CenterY: p.Y - 150, Height: 270, Width: 280, PathSuffix: `_${p.X}x`, OffsetCalcType: 'None' });
			if (!macroService.PollPattern(completedPattern, { TimeoutMs: 2_500 }).IsSuccess) {
				return p;
			}
		}
	}
}
