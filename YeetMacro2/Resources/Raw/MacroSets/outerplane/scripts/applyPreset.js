// @raw-script
// Apply equipmentPreset

function applyPreset(teamSlot) {
	if (!teamSlot) {
		teamSlot = settings.applyPreset.teamSlot.Value;
	}

	if (teamSlot == settings.applyPreset.lastApplied.Value) {
		return;
	} 

	const locationToPreset = {
		left: settings.applyPreset[`teamSlot${teamSlot}`].left.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].left.Value,
		top: settings.applyPreset[`teamSlot${teamSlot}`].top.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].top.Value,
		right: settings.applyPreset[`teamSlot${teamSlot}`].right.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].right.Value,
		bottom: settings.applyPreset[`teamSlot${teamSlot}`].bottom.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].bottom.Value,
	};

	for (const [location, preset] of Object.entries(locationToPreset)) {
		if (!preset) continue;

		const presetRegex = new RegExp(preset.replace(/ /g, ''));
		
		macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, HoldDurationMs: 750, PredicatePattern: patterns.battle.teamFormation.preset });
		const currentPreset = macroService.GetText(patterns.battle.teamFormation.preset.current);
		if (currentPreset.replace(/ /g, '').match(presetRegex)) {
			macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60, Y: 50 }, PredicatePattern: patterns.general.back });
			continue;
		}

		macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
		const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
			const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X, Y: p.Y, OffsetCalcType: 'None' });
			return {
				point: { X: p.X, Y: p.Y },
				name: macroService.GetText(presetNamePattern)
			};
		});
		const targetPreset = presetNames.find(pn => pn.name.replace(/ /g, '').match(presetRegex));
		if (!targetPreset) throw new Error(`Unable to find target preset '${preset}' for slot ${teamSlot} ${location}`);

		macroService.PollPoint(targetPreset.point, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.ok });
		macroService.PollPattern(patterns.battle.teamFormation.preset.ok, { DoClick: true, ClickPattern: patterns.battle.teamFormation.preset.ok2, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60 }, PredicatePattern: patterns.general.back });		
	}

	if (macroService.IsRunning) {
		settings.applyPreset.lastApplied.Value = teamSlot;
	}
}
