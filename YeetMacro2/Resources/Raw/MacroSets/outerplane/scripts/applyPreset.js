// @raw-script
// @isFavorite
// Apply equipmentPreset

function applyPreset(teamSlot) {
	const resolution = macroService.GetCurrentResolution();
	const swipeX = resolution.Width - 300;

	//if (!teamSlot) {
	//	const slots = [1, 2, 3, 4, 5, 6, 7, 8, 9];
	//	for (const s of slots) {
	//		if (macroService.FindPattern(patterns.battle.slot[s].selected).IsSuccess) {
	//			teamSlot = s;
	//			break;
	//		}
	//	}
	//}
	
	if (teamSlot === settings.applyPreset.lastApplied.Value) {
		return;
	}
	
	const locationToDelimiter = { left: '[<(]', top: '\\*', right: '[>)]', bottom: '=' };
	const battleTypeToAbbreviation = { defender: 'DEF', striker: 'STR', ranger: 'RAN', mage: 'MAG', 'healer': 'HLR' };

	for (let [location, delimiter] of Object.entries(locationToDelimiter)) {
		if (macroService.FindPattern(patterns.battle.teamFormation[location].remove).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
		}
		if (macroService.FindPattern(patterns.battle.teamFormation[location].move).IsSuccess) {
			macroService.PollPattern(patterns.battle.teamFormation[location].move, { DoClick: true, ClickOffset: { X: -100 }, InversePredicatePattern: patterns.battle.teamFormation[location].move });
		}

		macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, HoldDurationMs: 1_000, PredicatePattern: patterns.battle.teamFormation.preset });
		const unitTitleAndName = macroService.FindText(patterns.battle.teamFormation.unitTitleAndName);
		const battleTypes = ['defender', 'striker', 'ranger', 'mage', 'healer'].map(el => patterns.battle.teamFormation.battleTypes2[el]);
		const battleTypeResult = macroService.PollPattern(battleTypes);
		const battleType = battleTypeResult.Path?.split('.').pop();
		const battleTypeAbbreviation = battleTypeToAbbreviation[battleType];
		const gearSet = "..."
		let primaryStat = "..."
		if (unitTitleAndName.match(/The.*?Memorizer.*?Caren/ism) || unitTitleAndName.match(/Blazing.*?Fighter.*?Kano/ism) || unitTitleAndName.match(/Honorable.*?Knight/ism)) {
			primaryStat = 'DEF';
			delimiter = '#';
		} else if (unitTitleAndName.match(/Self.*?Del/ism)) {
			primaryStat = 'HLT';
			delimiter = '#';
		} else if (unitTitleAndName.match(/Ruin.*T[oa]m[oa]m[ao]/ism)) {
			primaryStat = 'HLT';
			gearSet = 'PEN';
		}

		let strRegex = `\\s?${delimiter}\\s?${battleTypeAbbreviation}\\s?${delimiter}\\s?${gearSet}\\s?${delimiter}\\s?${primaryStat}\\s?`;

		if (unitTitleAndName.match(/Gnosis.*Ne[lI][lI]/ism)) {
			strRegex = '#GN.*NELLA#';
		} else if (unitTitleAndName.match(/Eter.*T[oa]m[oa]m[ao]/ism)) {
			strRegex = '#TAMAMOE#';
		}

		const presetRegex = new RegExp(strRegex);
		const currentPreset = macroService.FindText(patterns.battle.teamFormation.preset.current);
		//logger.info(unitTitleAndName.replace(/\n/g, ' '));
		logger.info(`regex: ${presetRegex}    ||   currentPreset: ${currentPreset}`);
		if (currentPreset.match(presetRegex)) {
			macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60, Y: 50 }, PredicatePattern: patterns.general.back });
			continue;
		}

		macroService.PollPattern(patterns.battle.teamFormation.preset, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.presetList });
		
		if (currentPreset.toLowerCase() !== 'unused') {
			macroService.DoSwipe({ X: swipeX, Y: 300 }, { X: swipeX, Y: 800 });
			sleep(1_000);
			macroService.DoSwipe({ X: swipeX, Y: 300 }, { X: swipeX, Y: 800 });
			sleep(1_000);
			macroService.DoSwipe({ X: swipeX, Y: 300 }, { X: swipeX, Y: 800 });
			sleep(1_000);
		}

		const presetNameList = [];
		let targetPreset = findTargetPreset(presetRegex, presetNameList);
		let swipeCount = 0;
		while (!targetPreset) {
			//macroService.DoSwipe({ X: swipeX, Y: 800 }, { X: swipeX, Y: 300 });
			macroService.DoSwipe({ X: swipeX, Y: 750 }, { X: swipeX, Y: 350 });
			sleep(1_000);
			targetPreset = findTargetPreset(presetRegex, presetNameList);
			swipeCount++;

			if (swipeCount === 5) {
				throw presetNameList;
				//throw new Error(`Could not find preset regex: ${presetRegex}`)
			}
		}

		macroService.PollPoint(targetPreset.point, { DoClick: true, PredicatePattern: patterns.battle.teamFormation.preset.ok });
		macroService.PollPattern(patterns.battle.teamFormation.preset.ok, { DoClick: true, ClickPattern: patterns.battle.teamFormation.preset.ok2, PredicatePattern: patterns.battle.teamFormation.preset.presetList });

		macroService.PollPattern(patterns.battle.teamFormation.preset.topLeft, { DoClick: true, ClickOffset: { X: -60 }, PredicatePattern: patterns.general.back });
	}
	
	macroService.IsRunning && (settings.applyPreset.lastApplied.Value = teamSlot);
}

function findTargetPreset(presetRegex, presetNameList) {
	const presetCornerResult = macroService.FindPattern(patterns.battle.teamFormation.preset.corner, { Limit: 10 });
	const presetNames = presetCornerResult.Points.filter(p => p).map(p => {
		const presetNamePattern = macroService.ClonePattern(patterns.battle.teamFormation.preset.name, { X: p.X + 13, Y: p.Y + 8, OffsetCalcType: 'None', Path: `battle.teamFormation.preset.name_x${p.X}_y${p.Y}` });
		let name;
		for (let i = 0; i < 5 && !name; i++) {
			name = macroService.FindText(presetNamePattern)
		}

		return {
			point: { X: p.X, Y: p.Y },
			name: name
		};
	});
	const targetPreset = presetNames.sort((a, b) => a.point.Y - b.point.Y).find(pn => pn.name.match(presetRegex));
	presetNameList.push(...presetNames);
	return targetPreset;
}