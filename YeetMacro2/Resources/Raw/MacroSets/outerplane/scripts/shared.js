// @raw-script
// @position=1000
function selectTeam(teamSlot, returnCurrentCp) {
	if (!teamSlot || teamSlot === 'Current' || teamSlot < 1) return;

	const spacing = 90;
	const teamSlotStartY = patterns.battle.teamSlot.Pattern.Rect.Center.Y;
	const targetTeamSlot = macroService.ClonePattern(patterns.battle.teamSlot, { CenterY: teamSlotStartY + (spacing * (teamSlot - 1)) });
	const selectedTeamSlotStartY = patterns.battle.teamSlot.selected.Pattern.Rect.Center.Y;
	const targetTeamSlotSelected = macroService.ClonePattern(patterns.battle.teamSlot.selected, { CenterY: selectedTeamSlotStartY + (spacing * (teamSlot - 1)) });

	macroService.pollPattern(targetTeamSlot, { DoClick: true, PredicatePattern: targetTeamSlotSelected });

	if (macroService.IsRunning && returnCurrentCp) {
		const cpText = macroService.GetText(patterns.battle.cp);
		return Number(cpText.slice(0, -4).slice(1) + cpText.slice(-3));
	}
}

function selectTeamAndBattle(teamSlot, sweepBattle) {
	selectTeam(teamSlot);
	macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	if (sweepBattle) {
		macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
		macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });
	} else {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
	}
}