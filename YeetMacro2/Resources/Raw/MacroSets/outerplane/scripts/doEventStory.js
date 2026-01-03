// @tags=event
// Sweep event story hard
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.notice.doNotShowStoryForADay, patterns.battle.enter, patterns.event.story.retry];
const teamSlot = settings.doEventStory.teamSlot.Value;
const daily = dailyManager.GetCurrentDaily();
const clickPatterns = [patterns.event.story.rewardEnter, patterns.event.story.enter2, patterns.event.story.skip, patterns.event.story.next, patterns.event.story.nextStage];

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

			if (!daily.doEventStory.firstDoNotShowStoryForADaySkipped.IsChecked) {
				daily.doEventStory.firstDoNotShowStoryForADaySkipped.IsChecked = true;
			} else {
				macroService.PollPattern(patterns.event.story.notice.doNotShowStoryForADay.unselected, { DoClick: true, PredicatePattern: patterns.event.story.notice.doNotShowStoryForADay.selected });
			}

			macroService.PollPattern(patterns.event.story.notice.ok, { DoClick: true, ClickPattern: patterns.event.story.next, PredicatePattern: patterns.event.story.nextStage });
			macroService.PollPattern(patterns.event.story.nextStage, { DoClick: true, PredicatePattern: patterns.battle.enter });	
			break;
		case 'battle.enter':
			logger.info('doEventStory: do battle');
			if (!daily.doEventStory.isTeamResolved.IsChecked) {
				selectTeam(teamSlot);
				teamUnselectAll();
				teamSelectBonus();
				applyPreset();
				daily.doEventStory.isTeamResolved.IsChecked = true;
			}
			macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: [patterns.event.story.skip, patterns.event.story.next] });
			break;
		case 'event.story.retry':
			logger.info('doEventStory: done');
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
		mage: ['bottom', 'top', 'left'],
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
}