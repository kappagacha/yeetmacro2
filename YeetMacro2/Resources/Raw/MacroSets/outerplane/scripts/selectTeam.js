// @raw-script
// @position=1000
function selectTeam(targetTeamSlot, returnCurrentCp) {
	//logger.info(`selectTeam teamSlot ${targetTeamSlot}`);

	if (!targetTeamSlot || targetTeamSlot === 'Current' || targetTeamSlot < 1) return;

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

	applyPreset(targetTeamSlot);

	if (macroService.IsRunning && returnCurrentCp) {
		const cpText = macroService.GetText(patterns.battle.cp);
		return Number(cpText.slice(0, -4).slice(1) + cpText.slice(-3));
	}
}

function selectTeamAndBattle(teamSlot, sweepBattle) {
	selectTeam(teamSlot);
	macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	const numBattles = macroService.GetText(patterns.battle.setup.numBattles);
	if (sweepBattle) {
		macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
		macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });
	} else {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
	}
	return numBattles;
}