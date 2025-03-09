// @raw-script
// Apply equipmentPreset

function applyPreset(teamSlot) {
	const resolution = macroService.GetCurrentResolution();
	let isManualInvoke = false;
	let locationToPreset;
	let weaponSlot, accessorySlot;

	if (!teamSlot && settings.applyPreset.manual.IsEnabled) {
		isManualInvoke = true;
	} else if (!teamSlot) {
		const slots = [1, 2, 3, 4, 5, 6, 7, 8, 9];
		for (const s of slots) {
			if (macroService.FindPattern(patterns.battle.slot[s].selected).IsSuccess) {
				teamSlot = s;
				break;
			}
		}

		if (!teamSlot) throw new Error('Team slot not resolved');
	} else {
		// not ready yet
		//weaponSlot = settings.applyPreset[`teamSlot${teamSlot}`].weaponSlot.Value;
		//accessorySlot = settings.applyPreset[`teamSlot${teamSlot}`].accessorySlot.Value;
	}
	
	//logger.info(`applyPreset teamSlot ${teamSlot}`);

	if (!isManualInvoke && teamSlot == settings.applyPreset.lastApplied.Value) {
		return;
	} 

	if (!isManualInvoke && !settings.applyPreset[`teamSlot${teamSlot}`].IsEnabled) {
		return;
	}
	
	const battleTypeToCount = {
		defender: 0,
		striker: 0,
		ranger: 0,
		mage: 0,
		healer: 0
	};


	if (isManualInvoke) {
		locationToPreset = {
			left: settings.applyPreset.manual.left.IsEnabled && settings.applyPreset.manual.left.Value,
			top: settings.applyPreset.manual.top.IsEnabled && settings.applyPreset.manual.top.Value,
			right: settings.applyPreset.manual.right.IsEnabled && settings.applyPreset.manual.right.Value,
			bottom: settings.applyPreset.manual.bottom.IsEnabled && settings.applyPreset.manual.bottom.Value,
		};
	} else {
		locationToPreset = {
			left: settings.applyPreset[`teamSlot${teamSlot}`].left.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].left.Value,
			top: settings.applyPreset[`teamSlot${teamSlot}`].top.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].top.Value,
			right: settings.applyPreset[`teamSlot${teamSlot}`].right.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].right.Value,
			bottom: settings.applyPreset[`teamSlot${teamSlot}`].bottom.IsEnabled && settings.applyPreset[`teamSlot${teamSlot}`].bottom.Value,
		};
	}


	for (const [location, preset] of Object.entries(locationToPreset)) {
		if (!preset) continue;

		// make space optional and 1 can be T
		const presetRegex = new RegExp(preset.replace(/ /g, '\\s*').replace(/1/g, '[1T]'));

		if (macroService.FindPattern(patterns.battle.teamFormation[location].remove).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
		}
		if (macroService.FindPattern(patterns.battle.teamFormation[location].move).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].move, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].move });
		}
		
		macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, HoldDurationMs: 1_000, PredicatePattern: patterns.battle.teamFormation.preset });
		if (!isManualInvoke && !weaponSlot && !accessorySlot) {
			const currentPreset = macroService.GetText(patterns.battle.teamFormation.preset.current, preset);
			if (currentPreset.replace(/ /g, '').match(presetRegex, preset)) {
				macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60, Y: 50 }, PredicatePattern: patterns.general.back });
				continue;
			}
		}
		
		macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
		const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
			const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X + 13, Y: p.Y + 8, OffsetCalcType: 'None', Path: `battle.teamFormation.preset.name_x${p.X}_y${p.Y}` });
			return {
				point: { X: p.X, Y: p.Y },
				name: macroService.GetText(presetNamePattern, preset)
			};
		});
		const targetPreset = presetNames.sort((a, b) => a.point.Y - b.point.Y).find(pn => pn.name.match(presetRegex));
		if (!targetPreset) throw new Error(`Unable to find target preset '${preset}' for slot ${teamSlot} ${location}`);

		macroService.PollPoint(targetPreset.point, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.ok });
		macroService.PollPattern(patterns.battle.teamFormation.preset.ok, { DoClick: true, ClickPattern: patterns.battle.teamFormation.preset.ok2, PredicatePattern: patterns.battle.teamFormation.preset.presetList });

		if (isManualInvoke) {
			const battleTypes = ['defender', 'striker', 'ranger', 'mage', 'healer'].map(el => patterns.battle.teamFormation.battleTypes[el]);
			const battleTypeResult = macroService.PollPattern(battleTypes);
			const battleType = battleTypeResult.Path?.split('.').pop();
			battleTypeToCount[battleType]++;
			
			weaponSlot = settings.applyPreset.manual[battleType][`weaponSlot${battleTypeToCount[battleType]}`].Value;
			accessorySlot = settings.applyPreset.manual[battleType][`accessorySlot${battleTypeToCount[battleType]}`].Value;
		}
		
		if (weaponSlot || accessorySlot) {
			macroService.PollPattern(patterns.battle.teamFormation.preset.manageGear, { DoClick: true, PredicatePattern: patterns.titles.manageGear });

			const cornersPattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.manageGear.corners, { X: 1340, Y: 100, Width: resolution.Width - 1920 + 420, Height: 450, OffsetCalcType: 'DockLeft' });

			if (weaponSlot) {
				const corners = macroService.FindPattern(cornersPattern, { Limit: 10 })
				const cornerPoints = corners.Points.map(p => ({ X: parseInt(p.X), Y: parseInt(p.Y) }));
				cornerPoints.sort((a, b) => {
					if (a.Y === b.Y) {
						return a.X - b.X; // If Y values are the same, sort by X
					}
					return a.Y - b.Y; // Otherwise, sort by Y
				});

				const point = cornerPoints[weaponSlot - 1];
				const centerPoint = { X: point.X + 60, Y: point.Y - 60 };
				const checkPattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.manageGear.check, { CenterX: centerPoint.X, CenterY: centerPoint.Y, Padding: 20, OffsetCalcType: 'None', Path: `patterns.battle.teamFormation.preset.manageGear.check_x${centerPoint.X}_y${centerPoint.Y}` })
				macroService.PollPoint({ X: point.X + 5, Y: point.Y - 5 }, { DoClick: true, PredicatePattern: checkPattern });
			}

			if (accessorySlot) {
				macroService.PollPattern(patterns.battle.teamFormation.preset.manageGear.accessory, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.manageGear.accessory.selected });

				const corners = macroService.FindPattern(cornersPattern, { Limit: 10 })
				const cornerPoints = corners.Points.map(p => ({ X: parseInt(p.X), Y: parseInt(p.Y) }));
				cornerPoints.sort((a, b) => {
					if (a.Y === b.Y) {
						return a.X - b.X; // If Y values are the same, sort by X
					}
					return a.Y - b.Y; // Otherwise, sort by Y
				});

				const point = cornerPoints[accessorySlot - 1];
				const centerPoint = { X: point.X + 60, Y: point.Y - 60 };
				const checkPattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.manageGear.check, { CenterX: centerPoint.X, CenterY: centerPoint.Y, Padding: 20, OffsetCalcType: 'None', Path: `patterns.battle.teamFormation.preset.manageGear.check_x${centerPoint.X}_y${centerPoint.Y}` })
				macroService.PollPoint({ X: point.X + 5, Y: point.Y - 5 }, { DoClick: true, PredicatePattern: checkPattern });
			}

			macroService.ClickPattern(patterns.battle.teamFormation.preset.manageGear.equipGear);
			sleep(500);
			macroService.ClickPattern(patterns.battle.teamFormation.preset.manageGear.equipGear);
			sleep(500);
			macroService.PollPattern(patterns.titles.manageGear);


			// only click back if you see manage gear title
			if (macroService.FindPattern(patterns.titles.manageGear).IsSuccess) {
				macroService.ClickPattern(patterns.general.back);
				sleep(1_000);
			}
		} else {
			macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60 }, PredicatePattern: patterns.general.back });
		}

	}

	macroService.IsRunning && (settings.applyPreset.lastApplied.Value = teamSlot);
}
