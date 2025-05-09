// @raw-script
// @position=1000
function selectTeam(targetTeamSlot, opts = {}) {
	//logger.info(`selectTeam teamSlot ${targetTeamSlot}`);

	if (!targetTeamSlot || targetTeamSlot === 'Current' || targetTeamSlot < 1) return;

	if (targetTeamSlot === 'RecommendedElement') {
		const recommendedElement = {
			earth: 'fire',
			water: 'earth',
			fire: 'water',
			light: 'dark',
			dark: 'light'
		};
		const bossType = detectBossType();
		targetTeamSlot = recommendedElement[bossType];
	}

	if (['earth', 'water', 'fire', 'light', 'dark'].includes(targetTeamSlot)) {
		targetTeamSlot = settings.selectTeam[targetTeamSlot].Value;
	}

	const topLeft = macroService.GetTopLeft();
	const xLocation = topLeft.X + 90;
	const slots = [1, 2, 3, 4, 5, 6, 7, 8, 9];

	let targetSlotSelectedResult = macroService.FindPattern(patterns.battle.slot[targetTeamSlot].selected);
	if (!targetSlotSelectedResult.IsSuccess) {
		while (macroService.IsRunning) {
			const slotsResult = Object.assign(...slots.map(s => ({ [s]: macroService.FindPattern(patterns.battle.slot[s]).IsSuccess })));
			if (slotsResult[targetTeamSlot]) {	// found slot
				macroService.PollPattern(patterns.battle.slot[targetTeamSlot], { DoClick: true, PredicatePattern: patterns.battle.slot[targetTeamSlot].selected });
				break;
			}

			// slot not found so need to swipe
			const minSlot = Object.entries(slotsResult).reduce((min, [key, val]) => val && key < min ? key : min, 9);
			const maxSlot = Object.entries(slotsResult).reduce((max, [key, val]) => val && key > max ? key : max, 1);
			if (targetTeamSlot > maxSlot) {
				macroService.DoSwipe({ X: xLocation, Y: 400 }, { X: xLocation, Y: 200 });	// scroll down
			} else if (targetTeamSlot < minSlot) {
				macroService.DoSwipe({ X: xLocation, Y: 200 }, { X: xLocation, Y: 400 });	// scroll up
			}
			sleep(2_500);
		}
	}

	if (opts.applyPreset) {
		applyPreset(targetTeamSlot);
	}
}

function selectTeamAndBattle(teamSlot, opts = {}) {
	selectTeam(teamSlot, opts);
	const autoResult = macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: [patterns.battle.setup.sweep, patterns.battle.setup.enter] });

	let numBattles = macroService.FindText(patterns.battle.setup.numBattles);
	if (opts.targetNumBattles) {
		macroService.PollPattern(patterns.battle.setup.minBattle, { DoClick: true, PredicatePattern: patterns.battle.setup.numBattles.one });

		numBattles = 1
		while (Number(numBattles) < opts.targetNumBattles) {
			macroService.ClickPattern(patterns.battle.setup.addBattle)
			sleep(250)
			numBattles = macroService.FindText(patterns.battle.setup.numBattles);
			sleep(250)
		}
	}

	if (autoResult.PredicatePath === 'battle.setup.enter') {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
		return numBattles;
	}

	macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
	macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });

	return numBattles;
}

function detectBossType() {
	const bossElements = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.battle.bossType[el]);
	const bossElementsResult = macroService.PollPattern(bossElements);
	const detectedElement = bossElementsResult.Path?.split('.').pop();

	return detectedElement;
}
