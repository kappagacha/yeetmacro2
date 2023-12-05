// @raw-script
function selectTeam(target) {
	if (!target || target === 'Current' || target < 1) return;

	const spacing = 90;
	const targetTeamSlot = macroService.ClonePattern(patterns.battle.teamSlot);
	targetTeamSlot.Path = 'battles.teamSlot' + target;
	for (const pattern of targetTeamSlot.Patterns) {
		pattern.Rect = { X: pattern.Rect.X, Y: pattern.Rect.Y + (spacing * (target - 1)), Width: pattern.Rect.Width, Height: pattern.Rect.Height };
		pattern.OffsetCalcType = "None";
		pattern.IsBoundsPattern = true;
	}

	const targetTeamSlotSelected = macroService.ClonePattern(patterns.battle.teamSlot.selected);
	targetTeamSlotSelected.Path = 'battles.teamSlot.selected' + target;
	for (const pattern of targetTeamSlotSelected.Patterns) {
		pattern.Rect = { X: pattern.Rect.X, Y: pattern.Rect.Y + (spacing * (target - 1)), Width: pattern.Rect.Width, Height: pattern.Rect.Height };
		pattern.OffsetCalcType = "None";
	}

	macroService.pollPattern(targetTeamSlot, { DoClick: true, PredicatePattern: targetTeamSlotSelected });
}
