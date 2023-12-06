// @raw-script
function selectTeam(target) {
	if (!target || target === 'Current' || target < 1) return;

	const spacing = 90;
	const teamSlotStartY = patterns.battle.teamSlot.Pattern.Rect.Center.Y;
	const targetTeamSlot = macroService.ClonePattern(patterns.battle.teamSlot, { CenterY: teamSlotStartY + (spacing * (target - 1)) });
	const selectedTeamSlotStartY = patterns.battle.teamSlot.selected.Pattern.Rect.Center.Y;
	const targetTeamSlotSelected = macroService.ClonePattern(patterns.battle.teamSlot.selected, { CenterY: selectedTeamSlotStartY + (spacing * (target - 1)) });

	macroService.pollPattern(targetTeamSlot, { DoClick: true, PredicatePattern: targetTeamSlotSelected });
}
