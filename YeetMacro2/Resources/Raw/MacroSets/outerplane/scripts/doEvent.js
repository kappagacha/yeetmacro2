// @raw-script
// @tags=event

function doEvent(type) {
	if (!type) type = settings.doEvent.type.Value;

	if (type === 'story') {
		return doEventStory();
	} else if (type === 'storyHard1') {
		return sweepEventStoryHard(1);
	} else if (type === 'storyHard2') {
		return sweepEventStoryHard(2);
	} else {
		throw new Error(`Invalid event type: ${type}. Expected 'story', 'storyHard1', or 'storyHard2'.`);
	}
}

function doEventStory() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.notice.doNotShowStoryForADay, patterns.battle.enter, patterns.event.story.retry, patterns.event.story.exit];
	const teamSlot = settings.doEvent.story.teamSlot.Value;
	const daily = dailyManager.GetCurrentDaily();
	const clickPatterns = [patterns.event.story.rewardEnter, patterns.event.story.enter2, patterns.event.story.skip, patterns.event.story.next, patterns.event.story.nextStage, patterns.event.story.selectTeam];

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
		switch (loopResult.Path) {
			case 'lobby.level':
				refillStamina(80);
				logger.info('doEventStory: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doEventStory: click event');
				macroService.ClickPattern(patterns.adventure.event);
				sleep(500);
				break;
			case 'event.story.notice.doNotShowStoryForADay':
				logger.info('doEventStory: handle do not show story for a day');

				if (!daily.doEvent.story.firstDoNotShowStoryForADaySkipped.IsChecked) {
					daily.doEvent.story.firstDoNotShowStoryForADaySkipped.IsChecked = true;
				} else {
					macroService.PollPattern(patterns.event.story.notice.doNotShowStoryForADay.unselected, { DoClick: true, PredicatePattern: patterns.event.story.notice.doNotShowStoryForADay.selected });
				}

				macroService.PollPattern(patterns.event.story.notice.ok, { DoClick: true, ClickPattern: [patterns.event.story.next, patterns.event.story.rewardEnter], PredicatePattern: [patterns.event.story.nextStage, patterns.event.story.skip] });
				macroService.PollPattern(patterns.event.story.nextStage, { DoClick: true, ClickPattern: patterns.event.story.rewardEnter, PredicatePattern: [patterns.battle.enter, patterns.event.story.skip] });
				break;
			case 'battle.enter':
				logger.info('doEventStory: do battle');
				if (!daily.doEvent.story.isTeamResolved.IsChecked) {
					selectTeam(teamSlot);
					teamUnselectAll();
					const locationsToBonusCharacters = teamSelectBonus();
					if (locationsToBonusCharacters['right']?.battleType === 'mage') {
						applyPreset(undefined, { presetOverride: { right: '#MAG#PEN#ATK' } });
					} else {
						applyPreset();
					}
					daily.doEvent.story.isTeamResolved.IsChecked = true;
				}
				macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: [patterns.event.story.skip, patterns.event.story.next] });
				break;
			case 'event.story.selectTeam':
				break;
			case 'event.story.exit':
				macroService.PollPattern(patterns.event.story.exit, { DoClick: true, PredicatePattern: patterns.event.story.rewardNotification });
				macroService.PollPattern(patterns.event.story.rewardNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.general.back });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.event.story.title, PredicatePattern: patterns.event.story.title });
				macroService.PollPattern(patterns.event.story.incompleteHard, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
				break;
			case 'event.story.retry':
				macroService.PollPattern(patterns.event.story.exit2, { DoClick: true, PredicatePattern: patterns.event.story.rewardNotification });
				macroService.PollPattern(patterns.event.story.rewardNotification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.general.back });
				return 'doEventStory complete';
		}
		sleep(1_000);
	}

	function teamUnselectAll() {
		const locations = ['top', 'left', 'right', 'bottom'];
		macroService.PollPattern(patterns.battle.owned, { DoClick: true, PredicatePattern: patterns.battle.owned.filter });
		for (let location of locations) {
			const isOccupied = macroService.FindPattern(patterns.battle.teamFormation[location].occupied).IsSuccess;
			if (!isOccupied) continue;
			macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, PredicatePattern: patterns.battle.teamFormation[location].remove, PrimaryClickInversePredicatePattern: patterns.battle.teamFormation[location].remove });
			macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
		}
	}

	function teamSelectBonus() {
		const bonusResult = macroService.FindPattern(patterns.event.story.bonus, { Limit: 4 });
		if (!bonusResult.IsSuccess) {
			throw Error('Could not find bonus characters');
		}

		const bonusCharacters = [];
		const battleTypes = ['striker', 'mage', 'ranger', 'healer', 'defender'];
		const sortedBonusPoints = bonusResult.Points.map(p => p).sort((a, b) => a.X - b.X);

		for (let bonusPoint of sortedBonusPoints) {
			const cloneOpts = { X: bonusPoint.X + 95, Y: bonusPoint.Y + 30, Width: 40, Height: 40, PathSuffix: `_${bonusPoint.X}x_${bonusPoint.Y}y`, OffsetCalcType: 'None' };
			const battleTypePatterns = battleTypes.map(bt => macroService.ClonePattern(patterns.battle.battleType[bt], cloneOpts));
			const battleTypeResult = macroService.PollPattern(battleTypePatterns);
			const battleType = battleTypeResult.Path.split('_')[0].split('.').pop();
			bonusCharacters.push({ point: bonusPoint, battleType });
		}

		const locationsToBonusCharacters = { top: null, left: null, right: null, bottom: null };

		// Battle type preferences for locations
		const battleTypePreferences = {
			striker: ['top', 'bottom'],
			mage: ['bottom', 'top', 'left', 'right'],
			healer: ['left', 'right'],
			defender: ['right'],
			ranger: ['left', 'top', 'bottom']
		};

		// Assign bonus characters to locations based on battle type preferences
		for (let bonusChar of bonusCharacters) {
			const preferredLocations = battleTypePreferences[bonusChar.battleType];

			// Find first available preferred location
			for (let location of preferredLocations) {
				if (locationsToBonusCharacters[location] === null) {
					locationsToBonusCharacters[location] = bonusChar;
					break;
				}
			}
		}

		// Place bonus characters first in the order they appear in sortedBonusPoints
		for (let bonusChar of bonusCharacters) {
			// Find which location this bonusChar is assigned to
			const location = Object.keys(locationsToBonusCharacters).find(loc => locationsToBonusCharacters[loc] === bonusChar);
			if (location) {
				macroService.PollPoint(bonusChar.point, { DoClick: true, PredicatePattern: patterns.battle.teamFormation[location].add });
				macroService.PollPattern(patterns.battle.teamFormation[location].add, { DoClick: true, InversePredicatePattern: patterns.battle.teamFormation[location].add });
			}
		}

		// Then fill remaining empty slots
		let charactersPlaced = bonusCharacters.length;
		for (let [location, bonusChar] of Object.entries(locationsToBonusCharacters)) {
			if (!bonusChar) {
				const cloneOpts = { X: 70 + (charactersPlaced * 160), Y: 880, Width: 1700, Height: 120, PathSuffix: '_all', OffsetCalcType: 'None', BoundsCalcType: 'FillWidth' };
				// Find preferred battle types for this location (in order of priority)
				const preferredBattleTypesForLocation = [];
				for (let [battleType, locations] of Object.entries(battleTypePreferences)) {
					const priority = locations.indexOf(location);
					if (priority !== -1) {
						preferredBattleTypesForLocation.push({ battleType, priority });
					}
				}
				// Sort by priority (lower index = higher priority)
				preferredBattleTypesForLocation.sort((a, b) => a.priority - b.priority);
				const battleTypePatterns = preferredBattleTypesForLocation.map(pbt => pbt.battleType).map(bt => macroService.ClonePattern(patterns.battle.battleType[bt], cloneOpts));
				const preferedBattleTypeResult = macroService.PollPattern(battleTypePatterns);
				macroService.PollPoint(preferedBattleTypeResult.Point, { DoClick: true, PredicatePattern: patterns.battle.teamFormation[location].add });
				macroService.PollPattern(patterns.battle.teamFormation[location].add, { DoClick: true, InversePredicatePattern: patterns.battle.teamFormation[location].add });
				charactersPlaced++;
			}
		}

		return locationsToBonusCharacters;
	}
}

function sweepEventStoryHard(number) {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.enter];
	const daily = dailyManager.GetCurrentDaily();
	const resolution = macroService.GetCurrentResolution();
	const settingPrefix = `storyHard${number}`;
	const dailyPrefix = `storyHard${number}`;
	const logPrefix = `storyHard${number}`;
	const teamSlot = settings.doEvent[settingPrefix].teamSlot.Value;

	if (daily.doEvent[dailyPrefix].done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	const targetEventPattern = macroService.ClonePattern(settings.doEvent[settingPrefix].targetEventPattern.Value, {
		X: 80,
		Y: 215,
		Width: resolution.Width - 100,
		Height: 785,
		Path: `settings.${settingPrefix}.targetEventPattern`,
		OffsetCalcType: 'DockLeft'
	});

	const targetPartPattern = macroService.ClonePattern(settings.doEvent[settingPrefix].targetPartPattern.Value, {
		X: 80,
		Y: 215,
		Width: resolution.Width - 100,
		Height: 785,
		Path: `settings.${settingPrefix}.targetPartPattern`,
		OffsetCalcType: 'DockLeft'
	});

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [targetEventPattern, patterns.adventure.doNotSeeFor3days] });
		switch (loopResult.Path) {
			case 'lobby.level':
				refillStamina(80);
				goToLobby();
				logger.info(`${logPrefix}: click adventure tab`);
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info(`${logPrefix}: click event`);
				macroService.ClickPattern(patterns.adventure.event);
				sleep(500);
				break;
			case 'event.story.enter':
				logger.info(`${logPrefix}: claim rewards`);
				handleRewards();
				const done = handleEventShop(settings.doEvent[settingPrefix].shopNumber.Value);
				macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickPredicatePattern: patterns.titles.adventurerShop, PredicatePattern: patterns.event.story.enter });

				if (done) {
					settings.doDailies.doEvent[settingPrefix].Value = false;
					return;
				}

				logger.info(`${logPrefix}: sweep event hard stages`);
				macroService.PollPattern(patterns.event.story.enter, { DoClick: true, InversePredicatePattern: patterns.event.story.enter });
				const storyPartSwipeResult = macroService.PollPattern(targetPartPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });

				if (!storyPartSwipeResult.IsSuccess) {
					throw Error(`Unable to find pattern: settings.doEvent.${settingPrefix}.targetPartPattern`);
				}

				macroService.PollPattern(targetPartPattern, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
				macroService.PollPattern(patterns.event.story.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
				selectTeamAndBattle(teamSlot);

				if (macroService.IsRunning) {
					daily.doEvent[dailyPrefix].done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}

	// if return true, the this shop is complete
	function handleEventShop(shopNumber) {
		let xOffset = 230;
		if (shopNumber === '2') xOffset = 440;
		let staminaResult = macroService.PollPattern(patterns.general.stamina);
		const currencyBounds = {
			X: staminaResult.Point.X + xOffset,
			Y: staminaResult.Point.Y - 17,
			Height: 36,
			Width: 30
		};
		const currencyPattern = macroService.CapturePatternWithBounds(currencyBounds);

		macroService.PollPattern(patterns.event.story.eventShop, { DoClick: true, PredicatePattern: patterns.titles.adventurerShop });
		sleep(500);
		staminaResult = macroService.PollPattern(patterns.general.stamina);
		const shopCurrencyBounds = {
			X: staminaResult.Point.X + 220,
			Y: staminaResult.Point.Y - 17,
			Height: 32,
			Width: 30,
			Padding: 20,
			OffsetCalcType: 'None',
		};

		const currencyBoundedPattern = macroService.ClonePattern(currencyPattern, shopCurrencyBounds);
		let currencyResult = macroService.FindPattern(currencyBoundedPattern);
		let jointChallengeSelectedResult = macroService.FindPattern(patterns.shop.adventurer.event.jointChallenge.selected);
		if (!currencyResult.IsSuccess || jointChallengeSelectedResult.IsSuccess) {
			const subTabShopResult = macroService.FindPattern(patterns.shop.subTabShop, { Limit: 5 });
			if (!subTabShopResult.IsSuccess) {
				throw new Error('Could not find shop(s)');
			}

			for (const p of subTabShopResult.Points) {
				macroService.DoClick(p);
				sleep(500);
				currencyResult = macroService.FindPattern(currencyBoundedPattern);
				jointChallengeSelectedResult = macroService.FindPattern(patterns.shop.adventurer.event.jointChallenge.selected);
				if (currencyResult.IsSuccess && !jointChallengeSelectedResult.IsSuccess) break;
			}

			if (!currencyResult.IsSuccess) {
				throw new Error('Could not find target currency');
			}
		}

		let purchaseResult = macroService.PollPattern(patterns.shop.purchase1, { TimeoutMs: 3_000 });
		while (purchaseResult.IsSuccess) {
			macroService.PollPattern(patterns.shop.purchase1, { DoClick: true, PredicatePattern: patterns.shop.purchase.ok });
			const maxResult = macroService.FindPattern(patterns.shop.purchase.max);
			if (maxResult.IsSuccess) {
				const maxResult = macroService.PollPattern(patterns.shop.purchase.max, { DoClick: true, PredicatePattern: patterns.shop.purchase.sliderMax, TimeoutMs: 2_000 });
				if (!maxResult.IsSuccess) {
					macroService.PollPattern(patterns.shop.purchase1.cancel, { DoClick: true, PredicatePattern: [patterns.shop.purchase1, patterns.shop.purchase1.disabled] });
					purchaseResult = { IsSuccess: false };
					continue;
				}
			}
			const okResult = macroService.PollPattern(patterns.shop.purchase.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
			if (!okResult.IsSuccess) break;

			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.titles.adventurerShop, patterns.titles.shop, patterns.shop.premium.title] });
			purchaseResult = macroService.PollPattern(patterns.shop.purchase1, { TimeoutMs: 3_000 });
		}

		purchaseResult = macroService.PollPattern([patterns.shop.purchase1, patterns.shop.purchase1.disabled]);
		return purchaseResult.Path === 'shop.purchase1.disabled';
	}

	function handleRewards() {
		let moveNotification = macroService.PollPattern(patterns.event.move.notification, { TimeoutMs: 2_000 });
		if (moveNotification.IsSuccess) {
			macroService.PollPattern(patterns.event.move, { DoClick: true, PredicatePattern: patterns.event.move.close });

			let notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.move.recieve, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.move.close });
				notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
			}

			macroService.PollPattern(patterns.event.move.close, { DoClick: true, PredicatePattern: patterns.event.story.enter });
		}
	}
}
