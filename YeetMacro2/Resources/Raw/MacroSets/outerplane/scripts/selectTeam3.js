// @tags=favorites
// This onw will be for challenge skyward tower

const topLeft = macroService.GetTopLeft();
const bossTypePatterns = [
    'glicys',               // hard 34
    'darkDemiurgeVladaHard',// hard 35
    'darkDemiurgeVlada',    // very hard 10
    'darkEliza',            // very hard 12
    'fireTamamo',           // very hard 13
    'lightStella',          // very hard 15
    'waterLaplace',         // very hard 17
    'darkIota',             // very hard 20
].map(bt => patterns.battle.boss[bt]);
const bossTypeResult = macroService.FindPattern(bossTypePatterns);
const bossType = bossTypeResult.Path?.split('.').pop();
logger.info(`selectTeam3: bossType ${bossType}`);

const idealTeam = {
    //left: { name: 'monadIota' },            // dark ranger
    left: { name: 'gnosisNella' },          // light mage    -> starter pct damage
    top: { name: 'mysticSageAme' },         // light ranger  -> dps + slight pct damage with autos
    right: { name: 'monadEva' },            // light healer  -> double dual attack + healer
    bottom: { name: 'demiurgeStella' },     // light striker -> ender pct damage
    //bottom: { name: 'demiurgeLuna' },
    // light mage
};
const chainOrderOpts = { effectToPriority: {} };
const findPatternOpts = { OverrideBounds: { X: 185, Y: 340, Width: 180, Height: 135 }, OverrideOffsetCalcType: 'DockLeft' };

const all_water = macroService.FindPattern(patterns.battle.conditions.all_water, findPatternOpts).IsSuccess;
const all_light = macroService.FindPattern(patterns.battle.conditions.all_light, findPatternOpts).IsSuccess;

const no_ranger = macroService.FindPattern(patterns.battle.conditions.no_ranger, findPatternOpts).IsSuccess;
const no_healers = macroService.FindPattern(patterns.battle.conditions.no_healers, findPatternOpts).IsSuccess;
//const no_defenders = macroService.FindPattern(patterns.battle.conditions.no_defenders, findPatternOpts).IsSuccess;
const no_mages = macroService.FindPattern(patterns.battle.conditions.no_mages, findPatternOpts).IsSuccess;
const no_strikers = macroService.FindPattern(patterns.battle.conditions.no_strikers, findPatternOpts).IsSuccess;

const no_light = macroService.FindPattern(patterns.battle.conditions.no_light, findPatternOpts).IsSuccess;
const no_dark = macroService.FindPattern(patterns.battle.conditions.no_dark, findPatternOpts).IsSuccess;
//const no_earth = macroService.FindPattern(patterns.battle.conditions.no_earth, findPatternOpts).IsSuccess;

const atLeastOne_twoStar = macroService.FindPattern(patterns.battle.conditions.atLeastOne_twoStar, findPatternOpts).IsSuccess;
//const atLeastOne_ranger = macroService.FindPattern(patterns.battle.conditions.atLeastOne_ranger, findPatternOpts).IsSuccess;
//const atLeastOne_striker = macroService.FindPattern(patterns.battle.conditions.atLeastOne_striker, findPatternOpts).IsSuccess;
//const atLeastOne_healer = macroService.FindPattern(patterns.battle.conditions.atLeastOne_healer, findPatternOpts).IsSuccess;

//const atLeastOne_light = macroService.FindPattern(patterns.battle.conditions.atLeastOne_light, findPatternOpts).IsSuccess;
const atLeastOne_earth = macroService.FindPattern(patterns.battle.conditions.atLeastOne_earth, findPatternOpts).IsSuccess;
const atLeastOne_dark = macroService.FindPattern(patterns.battle.conditions.atLeastOne_dark, findPatternOpts).IsSuccess;


if (no_strikers) {
    idealTeam.bottom = { name: 'gnosisNella' };
    idealTeam.left = { name: 'monadIota' };
}

if (atLeastOne_dark) {
    idealTeam.left = { name: 'monadIota' };
}

if (atLeastOne_twoStar && no_light) {
    idealTeam.left = { name: 'monadIota' };
    idealTeam.top = { name: 'gnosisViella' };
    idealTeam.right = { name: 'nella' };
    idealTeam.bottom = { name: 'vera' };
} else if (atLeastOne_twoStar) {
    idealTeam.right = { name: 'faenen' };
}

if (no_mages && atLeastOne_twoStar) {
    idealTeam.left = { name: 'faenen' };
    idealTeam.right = { name: 'demiurgeDrakhan' };
} else if (no_mages) {
    idealTeam.left = { name: 'monadEva' };
    idealTeam.right = { name: 'demiurgeDrakhan' };
}

if (atLeastOne_earth && bossType == 'fireTamamo') { 
    idealTeam.left = { name: 'viella' };
    idealTeam.top = { name: 'summerRegina' };
    idealTeam.right = { name: 'summerEmber' };
    idealTeam.bottom = { name: 'tamara' };
} else if (atLeastOne_earth) {
    idealTeam.top = { name: 'delta' };
}

if (no_healers) {
    idealTeam.right = { name: 'demiurgeDrakhan' };
}

if (no_light && no_dark && bossType === 'glicys') {
    idealTeam.left = { name: 'viella' };
    idealTeam.top = { name: 'notia' };
    idealTeam.right = { name: 'delta' };
    idealTeam.bottom = { name: 'rey' };
    chainOrderOpts.effectToPriority.cdr = 0;
}



if (all_water && no_ranger) {
    idealTeam.left = { name: 'mene' };
    idealTeam.top = { name: 'summerRegina' };
    idealTeam.right = { name: 'summerEmber' };
    idealTeam.bottom = { name: 'caren' };
}

if (bossType === 'darkDemiurgeVlada' || bossType === 'darkEliza' || bossType === 'waterLaplace') {
    idealTeam.left = { name: 'monadIota' };
    idealTeam.top = { name: 'gnosisViella' };
    idealTeam.bottom = { name: 'demiurgeLuna' };
}

if (bossType === 'lightStella') {
    idealTeam.left = { name: 'nella' };
    idealTeam.top = { name: 'gnosisViella' };
    idealTeam.bottom = { name: 'demiurgeLuna' };
}

if (bossType === 'darkIota') {
    idealTeam.left = { name: 'iota' };
    idealTeam.top = { name: 'akari' };
    idealTeam.right = { name: 'monadEva' },
    idealTeam.bottom = { name: 'demiurgeLuna' };
}

if (bossType === 'darkDemiurgeVladaHard') {
    idealTeam.left = { name: 'tamamoEternity' };
    idealTeam.top = { name: 'drakhan' };
    idealTeam.right = { name: 'monadEva' };
    idealTeam.bottom = { name: 'marian', presetOverride: '#GN NELLA' };
}


selectTeam(9);
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

macroService.PollPattern(patterns.battle.owned, { DoClick: true, PredicatePattern: [patterns.battle.characterFilter, patterns.battle.characterFilter.applied] });
//const team = bossTypeToTeam[bossType];
const team = idealTeam;

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

    const allCharacterResult = macroService.PollPattern(allCharacterPattern, { DoClick: true, ClickPattern: patterns.battle.teamFormation[location].add, PredicatePattern: characterPattern, TimeoutMs: 3_000 });
    if (!allCharacterResult.IsSuccess) {
        throw new Error(`Could not find ${character.name}`);
    }
}

applyPreset(undefined, { presetOverride });
setChainOrder(chainOrderOpts);