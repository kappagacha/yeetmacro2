// @raw-script
// Apply equipmentPreset

function applyPreset(teamSlot) {
	if (!applyPreset) {

	}

	const teamFormationLocations = ['left', 'top', 'right', 'bottom'];
	for (const teamFormationLocation of teamFormationLocations) {
		macroService.PollPattern(patterns.battle.teamFormation[teamFormationLocation], { DoClick: true, HoldDurationMs: 750, PredicatePattern: patterns.battle.teamFormation.preset });
		macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
		const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
			const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X, Y: p.Y, OffsetCalcType: 'None' });
			return {
				point: { X: p.X, Y: p.Y },
				name: macroService.GetText(presetNamePattern)
			};
		});
	}


	return presetNames;
}
