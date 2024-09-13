// @raw-script
// Apply equipmentPreset

function applyPreset(teamSlot) {
	if (!teamSlot) {
		const slots = [1, 2, 3, 4, 5, 6, 7, 8, 9];
		for (const s of slots) {
			if (macroService.FindPattern(patterns.battle.slot[s].selected).IsSuccess) {
				teamSlot = s;
				break;
			}
		}
	}

	if (!teamSlot) throw new Error('Team slot not resolved');

	//logger.info(`applyPreset teamSlot ${teamSlot}`);

	if (settings.applyPreset.lastApplied.IsEnabled && teamSlot == settings.applyPreset.lastApplied.Value) {
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

		const presetRegex = new RegExp(preset.replace(/ /g, '\s*'));

		if (macroService.FindPattern(patterns.battle.teamFormation[location].remove).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
		}
		if (macroService.FindPattern(patterns.battle.teamFormation[location].move).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].move, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].move });
		}
		
		macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, HoldDurationMs: 1_000, PredicatePattern: patterns.battle.teamFormation.preset });
		const currentPreset = macroService.GetText(patterns.battle.teamFormation.preset.current, preset + '0123456789');
		if (currentPreset.replace(/ /g, '').match(presetRegex)) {
			macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60, Y: 50 }, PredicatePattern: patterns.general.back });
			continue;
		}

		macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
		const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
			const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X + 13, Y: p.Y + 10, OffsetCalcType: 'None', Path: `battle.teamFormation.preset.name_x${p.X}_y${p.Y}` });
			return {
				point: { X: p.X, Y: p.Y },
				name: macroService.GetText(presetNamePattern, preset + '0123456789')
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
