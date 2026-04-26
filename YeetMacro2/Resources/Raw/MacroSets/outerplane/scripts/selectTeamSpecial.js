// @raw-script
// @tags=favorites
// @position=-1

function selectTeamSpecial(type = '') {
	if (!type) type = settings.selectTeamSpecial.type.Value;

	if (type === 'general') {
		selectTeamGeneral();
	} else if (type === 'tower') {
		selectTeamTower();
	} else {
		throw new Error(`Invalid special team type: ${type}`);
	}
}

function selectTeamGeneral() {
	const bossTypePatterns = [
		'grandCalamari', 'unidentifiedChimera', 'schwartz', 'amadeus', 'masterlessGuardian',
		'epsilon', 'anubisGuardian', 'tyrantToddler', 'ziggsaron', 'vladiMax', 'glicys',
		'arsNova', 'ksai', 'forestKing', 'dekRilAndMekRil', 'archdemonShadow', 'meteos',
		'gustav', 'sacreedGuardian', 'assaultSuit',
		// irregular extermination project => infiltration operation
		'irregularCrawler', 'irregularRunner', 'irregularSpike', 'irregularScythe',
		'irregularMachineGun', 'irregularBlade', 'blockbuster', 'ironStretcher', 'mutatedWyvre',
		'irregularQueen',
		// guild raid
		'lightDelta', 'darkAstralGuardian'
	].map(bt => patterns.battle.boss[bt]);
	const bossTypeResult = macroService.PollPattern(bossTypePatterns);
	const bossType = bossTypeResult.Path?.split('.').pop();
	logger.info(`selectTeamGeneral: bossType ${bossType}`);

	if (bossType === 'gustav') {
		throw new Error('Gustav detected.');
	}

	const alternateEarthTeam = { left: { name: 'notia' }, top: { name: 'fran' }, right: { name: 'kappa' }, bottom: { name: 'ame' } };
	const alternateWaterTeam = { left: { name: 'mene' }, top: { name: 'fortuna' }, right: { name: 'summerEmber' }, bottom: { name: 'beth' } };
	const alternateFireTeam = { left: { name: 'christmasDianne' }, top: { name: 'iris' }, right: { name: 'kanon' }, bottom: { name: 'vlada' } };
	const alternateLightTeam = { left: { name: 'faenen' }, top: { name: 'drakhan' }, right: { name: 'gnosisNella' }, bottom: { name: 'regina' } };
	const alternateDarkTeam = { left: { name: 'nella' }, top: { name: 'maxwell' }, right: { name: 'omegaNadja' }, bottom: { name: 'iota' } };

	const bossTypeToTeam = {
		grandCalamari: {
			left: { name: 'akari', presetOverride: '#RAN#EFF#ATK' },
			top: { name: 'mysticSageAme' },
			right: { name: 'monadEva' },
			bottom: { name: 'demiurgeLuna' },
		},
		vladiMax: {
			left: undefined,
			top: { name: 'tamamoEternity' },
			right: undefined,
			bottom: { name: 'marian', presetOverride: '#GN NELLA' },
		},
		glicys: {
			left: { name: 'delta' },
			top: { name: 'ryu' },
			right: { name: 'monadEva' },
			bottom: { name: 'rey' },
		},
		ksai: {
			left: { name: 'tamara', presetOverride: '#RAN#EFF#ATK' },
			top: { name: 'fortuna' },
			right: { name: 'caren' },
			bottom: { name: 'summerRegina' },
		},
		forestKing: {
			left: { name: 'stella' },
			top: { name: 'mysticSageAme' },
			right: { name: 'monadEva' },
			bottom: { name: 'demiurgeLuna' },
		},
		gustav: {
			left: { name: 'monadEva' },
			top: { name: 'mysticSageAme' },
			right: { name: 'gnosisNella' },
			bottom: { name: 'monadIota' },
		},
		sacreedGuardian: {
			left: { name: 'demiurgeVlada', presetOverride: '#RAN#EFF#ATK' },
			top: { name: 'demiurgeAstei' },
			right: { name: 'maxwell', presetOverride: '#MAG#PEN#ATK' },
			bottom: { name: 'iota' },
		},
		assaultSuit: {
			left: { name: 'mene' },
			top: { name: 'ryu' },
			right: { name: 'delta' },
			bottom: { name: 'rey' },
		},
		irregularBlade: {
			left: { name: 'bryn' },
			top: { name: 'maxie' },
			right: { name: 'astei' },
			bottom: { name: 'eternal' },
		},
		mutatedWyvre: {
			left: { name: 'dianne' },
			top: { name: 'demiurgeAstei' },
			right: { name: 'maxwell', presetOverride: '#MAG#PEN#ATK' },
			bottom: { name: 'gnosisViella' },
		},
		irregularQueen: {
			left: { name: 'monadEva' },
			top: { name: 'mysticSageAme' },
			right: { name: 'demiurgeDrakhan' },
			bottom: { name: 'demiurgeLuna' },
		},
	};

	if (settings.selectTeamSpecial.useAlternateTeams.Value) {
		bossTypeToTeam.irregularCrawler = alternateWaterTeam;
		bossTypeToTeam.irregularRunner = alternateFireTeam;
		bossTypeToTeam.irregularSpike = alternateLightTeam;
		bossTypeToTeam.irregularScythe = alternateFireTeam;
		bossTypeToTeam.irregularMachineGun = alternateWaterTeam;
		bossTypeToTeam.blockbuster = alternateEarthTeam;
		bossTypeToTeam.ironStretcher = alternateWaterTeam;
	}

	logger.debug(`bossType: ${bossType}`);
	// use element advantage for non special boss types
	if (!bossTypeToTeam[bossType]) {
		selectTeam("RecommendedElement");
		applyPreset();
		setChainOrder();
		return;
	}

	applyTeamConfiguration(bossTypeToTeam[bossType]);
}

function selectTeamTower() {
	const bossTypePatterns = [
		'glicys',               // hard 34
		'darkDemiurgeVladaHard',// hard 35
		'shichifujasShadow',    // hard 37
		'darkDemiurgeVladaVeryHard',    // very hard 10
		'darkEliza',            // very hard 12
		'fireTamamo',           // very hard 13
		'lightStella',          // very hard 15
		'waterLaplace',         // very hard 17
		'darkIota',             // very hard 20
	].map(bt => patterns.battle.boss[bt]);
	const bossTypeResult = macroService.FindPattern(bossTypePatterns);
	const bossType = bossTypeResult.Path?.split('.').pop();
	logger.info(`selectTeamTower: bossType ${bossType}`);

	let idealTeam = {
		left: { name: 'monadIota' },
		top: { name: 'mysticSageAme' },
		right: { name: 'monadEva' },
		bottom: { name: 'demiurgeLuna' },
	};
	const chainOrderOpts = { effectToPriority: {} };
	const findPatternOpts = { OverrideBounds: { X: 185, Y: 340, Width: 180, Height: 135 }, OverrideOffsetCalcType: 'DockLeft' };

	const all_water = macroService.FindPattern(patterns.battle.conditions.all_water, findPatternOpts).IsSuccess;
	const all_light = macroService.FindPattern(patterns.battle.conditions.all_light, findPatternOpts).IsSuccess;

	const no_rangers = macroService.FindPattern(patterns.battle.conditions.no_rangers, findPatternOpts).IsSuccess;
	const no_healers = macroService.FindPattern(patterns.battle.conditions.no_healers, findPatternOpts).IsSuccess;
	//const no_defenders = macroService.FindPattern(patterns.battle.conditions.no_defenders, findPatternOpts).IsSuccess;
	const no_mages = macroService.FindPattern(patterns.battle.conditions.no_mages, findPatternOpts).IsSuccess;
	//const no_strikers = macroService.FindPattern(patterns.battle.conditions.no_strikers, findPatternOpts).IsSuccess;

	const no_light = macroService.FindPattern(patterns.battle.conditions.no_light, findPatternOpts).IsSuccess;
	const no_dark = macroService.FindPattern(patterns.battle.conditions.no_dark, findPatternOpts).IsSuccess;
	//const no_earth = macroService.FindPattern(patterns.battle.conditions.no_earth, findPatternOpts).IsSuccess;

	const atLeastOne_twoStar = macroService.FindPattern(patterns.battle.conditions.atLeastOne_twoStar, findPatternOpts).IsSuccess;
	//const atLeastOne_threeStar = macroService.FindPattern(patterns.battle.conditions.atLeastOne_threeStar, findPatternOpts).IsSuccess;
	//const atLeastOne_ranger = macroService.FindPattern(patterns.battle.conditions.atLeastOne_ranger, findPatternOpts).IsSuccess;
	const atLeastOne_striker = macroService.FindPattern(patterns.battle.conditions.atLeastOne_striker, findPatternOpts).IsSuccess;
	//const atLeastOne_healer = macroService.FindPattern(patterns.battle.conditions.atLeastOne_healer, findPatternOpts).IsSuccess;
	const atLeastOne_defender = macroService.FindPattern(patterns.battle.conditions.atLeastOne_defender, findPatternOpts).IsSuccess;

	//const atLeastOne_light = macroService.FindPattern(patterns.battle.conditions.atLeastOne_light, findPatternOpts).IsSuccess;
	const atLeastOne_earth = macroService.FindPattern(patterns.battle.conditions.atLeastOne_earth, findPatternOpts).IsSuccess;
	const atLeastOne_fire = macroService.FindPattern(patterns.battle.conditions.atLeastOne_fire, findPatternOpts).IsSuccess;
	//const atLeastOne_dark = macroService.FindPattern(patterns.battle.conditions.atLeastOne_dark, findPatternOpts).IsSuccess;

	if (atLeastOne_twoStar) {
		idealTeam.right = { name: 'faenen' };
	}
	if (no_light) {
		idealTeam.top = { name: 'demiurgeAstei' };
		idealTeam.right = { name: 'nella' };
		idealTeam.bottom = { name: 'gnosisDahlia' };
	}
	if (no_rangers) {
		idealTeam.left = { name: 'monadEva' };
		idealTeam.top = { name: 'gnosisNella' };
		idealTeam.right = { name: 'demiurgeDrakhan' };
	}
	if (no_dark || all_light) {
		idealTeam.left = { name: 'gnosisNella' };
	}
	if (atLeastOne_fire) {
		idealTeam.bottom = { name: 'bryn' };
	}
	if (atLeastOne_earth) {
		idealTeam.bottom = { name: 'delta' };
	}
	if (atLeastOne_defender) {
		idealTeam.left = { name: 'monadEva' };
		idealTeam.right = { name: 'demiurgeDrakhan' };
	}
	if (no_mages) {
		idealTeam.bottom = { name: 'gnosisDahlia' };
	}
	if (no_healers) {
		idealTeam.right = { name: 'demiurgeDrakhan' };
	}
	if (atLeastOne_striker) {
		idealTeam.bottom = { name: 'gnosisDahlia' };
	}
	if (atLeastOne_earth && bossType == 'fireTamamo') {
		idealTeam.left = { name: 'viella' };
		idealTeam.top = { name: 'summerRegina' };
		idealTeam.right = { name: 'summerEmber' };
		idealTeam.bottom = { name: 'tamara' };
	}
	if (no_light && no_dark && bossType === 'glicys') {
		idealTeam.left = { name: 'viella' };
		idealTeam.top = { name: 'notia' };
		idealTeam.right = { name: 'delta' };
		idealTeam.bottom = { name: 'rey' };
		chainOrderOpts.effectToPriority.cdr = 0;
	}
	if (atLeastOne_twoStar && no_light) {
		idealTeam.left = { name: 'monadIota' };
		idealTeam.top = { name: 'demiurgeAstei' };
		idealTeam.right = { name: 'nella' };
		idealTeam.bottom = { name: 'vera' };
	}
	if (all_water && no_rangers) {
		idealTeam.left = { name: 'mene' };
		idealTeam.top = { name: 'summerRegina' };
		idealTeam.right = { name: 'summerEmber' };
		idealTeam.bottom = { name: 'caren' };
	}
	if (bossType === 'shichifujasShadow' && atLeastOne_twoStar) {
		idealTeam.top = { name: 'delta' };
		idealTeam.left = { name: 'gnosisNella' };
		idealTeam.right = { name: 'faenen' };
		idealTeam.bottom = { name: 'kanon' };
	} else if (bossType === 'shichifujasShadow') {
		idealTeam.top = { name: 'delta' };
		idealTeam.left = { name: 'gnosisNella' };
		idealTeam.bottom = { name: 'kanon' };
	}
	if (bossType === 'lightStella') {
		idealTeam.left = { name: 'nella' };
		idealTeam.top = { name: 'gnosisViella' };
		idealTeam.bottom = { name: 'demiurgeLuna' };
	}
	if (bossType === 'darkIota') {
		idealTeam.left = { name: 'iota' };
		idealTeam.top = { name: 'akari' };
		idealTeam.right = { name: 'monadEva' };
		idealTeam.bottom = { name: 'demiurgeLuna' };
	}
	if (bossType === 'darkDemiurgeVladaHard') {
		idealTeam.left = { name: 'tamamoEternity' };
		idealTeam.top = { name: 'drakhan' };
		idealTeam.right = { name: 'monadEva' };
		idealTeam.bottom = { name: 'marian', presetOverride: '#GN NELLA' };
	}

	applyTeamConfiguration(idealTeam, chainOrderOpts);
}

function applyTeamConfiguration(team, chainOrderOpts = {}) {
	const topLeft = macroService.GetTopLeft();

	// Shared character filter
	const characterToFilter = {
		akari: { element: 'light', battleType: 'ranger' },
		mysticSageAme: { element: 'light', battleType: 'ranger' },
		monadEva: { element: 'light', battleType: 'healer' },
		demiurgeLuna: { element: 'light', battleType: 'mage' },
		regina: { element: 'light', battleType: 'mage' },
		tamamoEternity: { element: 'earth', battleType: 'mage' },
		marian: { element: 'water', battleType: 'mage' },
		delta: { element: 'earth', battleType: 'striker' },
		rey: { element: 'earth', battleType: 'striker' },
		ame: { element: 'earth', battleType: 'mage' },
		sterope: { element: 'dark', battleType: 'defender' },
		tamara: { element: 'water', battleType: 'ranger' },
		caren: { element: 'water', battleType: 'striker' },
		summerRegina: { element: 'water', battleType: 'striker' },
		stella: { element: 'light', battleType: 'ranger' },
		gnosisViella: { element: 'dark', battleType: 'striker' },
		nella: { element: 'dark', battleType: 'healer' },
		ryu: { element: 'earth', battleType: 'ranger' },
		demiurgeVlada: { element: 'dark', battleType: 'ranger' },
		demiurgeAstei: { element: 'dark', battleType: 'mage' },
		maxwell: { element: 'dark', battleType: 'mage' },
		iota: { element: 'dark', battleType: 'mage' },
		mene: { element: 'water', battleType: 'healer' },
		fortuna: { element: 'water', battleType: 'mage' },
		demiurgeDrakhan: { element: 'light', battleType: 'defender' },
		gnosisNella: { element: 'light', battleType: 'mage' },
		valentine: { element: 'fire', battleType: 'ranger' },
		maxie: { element: 'fire', battleType: 'ranger' },
		bryn: { element: 'fire', battleType: 'mage' },
		eternal: { element: 'fire', battleType: 'mage' },
		notia: { element: 'earth', battleType: 'ranger' },
		fran: { element: 'earth', battleType: 'ranger' },
		kappa: { element: 'earth', battleType: 'defender' },
		christmasDianne: { element: 'fire', battleType: 'ranger' },
		iris: { element: 'fire', battleType: 'mage' },
		kanon: { element: 'fire', battleType: 'striker' },
		vlada: { element: 'fire', battleType: 'striker' },
		summerEmber: { element: 'water', battleType: 'defender' },
		beth: { element: 'water', battleType: 'striker' },
		dianne: { element: 'light', battleType: 'healer' },
		drakhan: { element: 'light', battleType: 'striker' },
		omegaNadja: { element: 'dark', battleType: 'defender' },
		faenen: { element: 'light', battleType: 'healer' },
		astie: { element: 'fire', battleType: 'healer' },
		monadIota: { element: 'dark', battleType: 'ranger' },
		astei: { element: 'fire', battleType: 'healer' },
		gnosisDahlia: { element: 'dark', battleType: 'striker' },
		tamamo: { element: 'fire', battleType: 'mage' },
		viella: { element: 'earth', battleType: 'healer' },
		vera: { element: 'dark', battleType: 'striker' },
		demiurgeStella: { element: 'light', battleType: 'striker' },
	};

	const locationToCharacterCloneOpts = {
		left: { X: topLeft.X + 371.25, Y: 466.25, Width: 161.5, Height: 164.5, PathSuffix: '_left', OffsetCalcType: 'None' },
		top: { X: topLeft.X + 551.25, Y: 311.25, Width: 161.5, Height: 164.5, PathSuffix: '_top', OffsetCalcType: 'None' },
		right: { X: topLeft.X + 736.25, Y: 466.25, Width: 161.5, Height: 164.5, PathSuffix: '_right', OffsetCalcType: 'None' },
		bottom: { X: topLeft.X + 551.25, Y: 635.25, Width: 161.5, Height: 164.5, PathSuffix: '_bottom', OffsetCalcType: 'None' },
	};

	selectTeam(9);
	macroService.PollPattern(patterns.battle.owned, { DoClick: true, PredicatePattern: [patterns.battle.characterFilter, patterns.battle.characterFilter.applied] });

	let lastElementFilter, lastElementBattleType;
	const presetOverride = {};

	for ([location, character] of Object.entries(team)) {
		const isOccupied = macroService.FindPattern(patterns.battle.teamFormation[location].occupied).IsSuccess;
		if (!character && isOccupied) {
			macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, PredicatePattern: patterns.battle.teamFormation[location].remove, PrimaryClickInversePredicatePattern: patterns.battle.teamFormation[location].remove });
			macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, InversePredicatePattern: patterns.battle.teamFormation[location].remove });
			continue;
		} else if (!character) {
			continue;
		}

		const filter = characterToFilter[character.name];
		if (!filter)
			throw new Error(`Filter was not found for ${character.name}`);

		const characterCloneOpst = locationToCharacterCloneOpts[location];
		const characterPattern = macroService.ClonePattern(patterns.battle.character[character.name], characterCloneOpst);
		const characterPatternResult = macroService.PollPattern(characterPattern, { TimeoutMs: 500 });

		if (character.presetOverride) presetOverride[location] = character.presetOverride;
		if (characterPatternResult.IsSuccess) continue;

		macroService.PollPattern([patterns.battle.characterFilter, patterns.battle.characterFilter.applied], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.ok });
		macroService.PollPattern(patterns.battle.characterFilter.battleType, { SwipePattern: patterns.battle.characterFilter.swipeDown });

		if (lastElementFilter != filter.element) {
			macroService.PollPattern(patterns.battle.characterFilter.element.all, { DoClick: true, PredicatePattern: patterns.battle.characterFilter.element.all.selected });
			macroService.PollPattern(patterns.battle.characterFilter.element[filter.element], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.element[filter.element].selected });
			lastElementFilter = filter.element;
		}

		if (lastElementBattleType != filter.battleType) {
			macroService.PollPattern(patterns.battle.characterFilter.battleType.all, { DoClick: true, PredicatePattern: patterns.battle.characterFilter.battleType.all.selected });
			macroService.PollPattern(patterns.battle.characterFilter.battleType[filter.battleType], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.battleType[filter.battleType].selected });
			lastElementBattleType = filter.battleType;
		}

		macroService.PollPattern(patterns.battle.characterFilter.ok, { DoClick: true, PredicatePattern: [patterns.battle.characterFilter, patterns.battle.characterFilter.applied] });
		const allCharacterCloneOpts = { X: 70, Y: 880, Width: 1700, Height: 130, PathSuffix: '_all', OffsetCalcType: 'None', BoundsCalcType: 'FillWidth' };
		const allCharacterPattern = macroService.ClonePattern(patterns.battle.character[character.name], allCharacterCloneOpts);
		if (isOccupied) {
			macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, PredicatePattern: [patterns.battle.teamFormation[location].remove, patterns.battle.teamFormation[location].add], PrimaryClickInversePredicatePattern: patterns.battle.teamFormation[location].remove });
		}

		const allCharacterResult = macroService.PollPattern(allCharacterPattern, { DoClick: true, ClickPattern: [patterns.battle.teamFormation[location].add, patterns.battle.teamFormation[location].remove], PredicatePattern: characterPattern, TimeoutMs: 3_000 });
		if (!allCharacterResult.IsSuccess) {
			throw new Error(`Could not find ${character.name}`);
		}
	}

	applyPreset(undefined, { presetOverride });
	setChainOrder(chainOrderOpts);
}
