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
				macroService.SwipePattern(patterns.battle.slot.swipeDown);
			} else if (targetTeamSlot < minSlot) {
				macroService.SwipePattern(patterns.battle.slot.swipeUp);
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

function setChainOrder(opts = { effectToPriority: {} }) {
	macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: [patterns.battle.chainPreview, patterns.battle.chainPreview.selected], PredicatePattern: [patterns.battle.chainPreview, patterns.battle.chainPreview.selected] });
	macroService.PollPattern(patterns.battle.chainPreview, { DoClick: true, PredicatePattern: patterns.battle.chainPreview.selected });
	if (macroService.FindPattern(patterns.battle.chainPreview.missingSlots).IsSuccess) return;

	const chainPreviewResult = { A: {}, B: {}, C: {}, D: {} };

	const chainPositionBasePatterns = [
		patterns.battle.chainPreview.starterExclusive,
		patterns.battle.chainPreview.starterExclusive.enabled,
		patterns.battle.chainPreview.companion,
		patterns.battle.chainPreview.finisherExclusive,
		patterns.battle.chainPreview.finisherExclusive.enabled
	];

	const effectsBasePatterns = [
		patterns.battle.chainPreview.effects.pctDmg,
		patterns.battle.chainPreview.effects.cdr,
		patterns.battle.chainPreview.effects.weaknessGuage,
		patterns.battle.chainPreview.effects.cursed,
	];

	const effectToPriority = {
		pctDmg: 1,
		cdr: 2,
		weaknessGuage: 3,
		cursed: 4,
		...opts.effectToPriority,
	};

	while (macroService.IsRunning) {
		for (let position of Object.keys(chainPreviewResult)) {
			// $patterns uses snapshot patterns json and the other uses actual C# class PatternNodeViewModel
			const originalRawBounds = patterns.battle.chainPreview[position]?.Pattern?.RawBounds || patterns.battle.chainPreview[position].$patterns[0].rawBounds;
			let rawBounds = { ...originalRawBounds };
			const cloneOpts = { RawBounds: rawBounds, PathSuffix: `_${position}` };
			const chainPositionPatterns = chainPositionBasePatterns.map(p => macroService.ClonePattern(p, cloneOpts));
			const effectPatterns = effectsBasePatterns.map(p => macroService.ClonePattern(p, cloneOpts));

			chainPreviewResult[position].chainEffectPosition = macroService.PollPattern(chainPositionPatterns).Path?.split('.').pop().slice(0, -2);
			chainPreviewResult[position].effect = macroService.FindPattern(effectPatterns).Path?.split('.').pop().slice(0, -2);
			chainPreviewResult[position].priority = chainPreviewResult[position].effect ? effectToPriority[chainPreviewResult[position].effect] : 100;
		}

		let targetStarterExclusive = 'A';
		let targetFinisherExclusive = 'D';
		let starterMin = Infinity;
		let finisherMin = Infinity;

		for (const [key, value] of Object.entries(chainPreviewResult)) {
			const { chainEffectPosition, priority } = value;

			// get first highest priority
			if (chainEffectPosition === "starterExclusive" && priority < starterMin) {
				starterMin = priority;
				targetStarterExclusive = key;
			}

			// get last hightest priority
			if (chainEffectPosition === "finisherExclusive" && priority <= finisherMin) {
				finisherMin = priority;
				targetFinisherExclusive = key;
			}
		}

		if (targetStarterExclusive === 'A' && targetFinisherExclusive === 'D') break;

		if (targetStarterExclusive !== 'A') {
			macroService.ClickPattern(patterns.battle.chainPreview.A);
			sleep(250);
			macroService.ClickPattern(patterns.battle.chainPreview[targetStarterExclusive]);
		}

		if (targetFinisherExclusive !== 'D' && targetStarterExclusive !== 'D') {
			macroService.ClickPattern(patterns.battle.chainPreview.D);
			sleep(250);
			macroService.ClickPattern(patterns.battle.chainPreview[targetFinisherExclusive]);
		}
	}
}