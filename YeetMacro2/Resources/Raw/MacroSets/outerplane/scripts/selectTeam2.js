// @tags=favorites


const topLeft = macroService.GetTopLeft();
const bossTypePatterns = ['grandCalamari', 'unidentifiedChimera', 'schwartz', 'amadeus'].map(bt => patterns.battle.boss[bt]);;
const bossTypeResult = macroService.PollPattern(bossTypePatterns);
const bossType = bossTypeResult.Path?.split('.').pop();
logger.debug(`bossType: ${bossType}`);
const specialBossTypes = ['grandCalamari'];

// use element advantage for non special boss types
if (!specialBossTypes.includes(bossType)) {
    selectTeam("RecommendedElement");
    applyPreset();
    setChainOrder();
    return;
}

selectTeam(9);
const bossTypeToTeam = {
    grandCalamari: {
        left: {
            name: 'akari',
            presetOverride: '#RAN#EFF#ATK'
        },
        top: {
            name: 'mysticSageAme'
        },
        right: {
            name: 'monadEva'
        },
        bottom: {
            name: 'regina'
        },
    }
};

const characterToFilter = {
    akari: {
        element: 'light',
        battleType: 'ranger'
    },
    mysticSageAme: {
        element: 'light',
        battleType: 'ranger'
    },
    monadEva: {
        element: 'light',
        battleType: 'healer'
    },
    regina: {
        element: 'light',
        battleType: 'mage'
    },
};

const locationToCharacterCloneOpts = {
    left: { X: topLeft.X + 371.25, Y: 466.25, Width: 161.5, Height: 164.5, PathSuffix: '_left', OffsetCalcType: 'None' },
    top: { X: topLeft.X + 551.25, Y: 311.25, Width: 161.5, Height: 164.5, PathSuffix: '_top', OffsetCalcType: 'None' },
    right: { X: topLeft.X + 736.25, Y: 466.25, Width: 161.5, Height: 164.5, PathSuffix: '_right', OffsetCalcType: 'None' },
    bottom: { X: topLeft.X + 551.25, Y: 635.25, Width: 161.5, Height: 164.5, PathSuffix: '_bottom', OffsetCalcType: 'None' },
};

macroService.PollPattern(patterns.battle.owned, { DoClick: true, PredicatePattern: [patterns.battle.characterFilter, patterns.battle.characterFilter.applied] });
const team = bossTypeToTeam[bossType];
let lastElementFilter, lastElementBattleType;
const presetOverride = {};

for ([location, character] of Object.entries(team)) {
    if (!character) {
        macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, PredicatePattern: patterns.battle.teamFormation[location].remove });
        macroService.PollPattern(patterns.battle.teamFormation[location].remove, { DoClick: true, InversePredicatePattern: patterns.battle.teamFormation[location].remove, PrimaryClickInversePredicatePattern: patterns.battle.teamFormation[location].remove });
        continue;
    }

    const filter = characterToFilter[character.name];
    const characterCloneOpst = locationToCharacterCloneOpts[location];
    const characterPattern = macroService.ClonePattern(patterns.battle.character[character.name], characterCloneOpst);
    const characterPatternResult = macroService.FindPattern(characterPattern);

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
    macroService.PollPattern(patterns.battle.teamFormation[location], { DoClick: true, PredicatePattern: [patterns.battle.teamFormation[location].remove, patterns.battle.teamFormation[location].add], PrimaryClickInversePredicatePattern: patterns.battle.teamFormation[location].remove });
    macroService.PollPattern(allCharacterPattern, { DoClick: true, PredicatePattern: characterPattern });
}

applyPreset(undefined, { presetOverride });
setChainOrder();