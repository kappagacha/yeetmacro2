// @tags=inventory
// @position=-20

const topLeft = macroService.GetTopLeft();
const item = { totalPoints: 0, desiredPoints: 0, desiredStats: [] };
const itemGradePatterns = ['legendary', 'epic','superior'].map(ig => patterns.inventory.item.stat2.grade[ig]);
const itemTypePatterns = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'].map(it => patterns.inventory.item.stat2.type[it]);
const itemStatTypePatterns = ['health', 'speed', 'attack', 'defence', 'critChance', 'critDmg', 'dmgIncrease', 'dmgReduction', 'effectiveness', 'resilience', 'penetration', 'healsWhenHit'].map(ist => patterns.inventory.item.stat2.statType[ist]);
item.itemGrade = macroService.PollPattern(itemGradePatterns).Path?.split('.').pop();
item.itemType = macroService.PollPattern(itemTypePatterns).Path?.split('.').pop();

const primary1RawBounds = patterns.inventory.item.stat2.primary1?.Pattern?.RawBounds || patterns.inventory.item.stat2.primary1.$patterns[0].rawBounds;
const primary1CalcOffsetType = patterns.inventory.item.stat2.primary1?.Pattern?.OffsetCalcType ?? patterns.inventory.item.stat2.primary1.$patterns[0].offsetCalcType;
const primary1CloneOpts = {
	PathSuffix: '_primary1',
	OffsetCalcType: primary1CalcOffsetType,
	RawBounds: {
		X: primary1RawBounds.X ?? primary1RawBounds.x,
		Y: primary1RawBounds.Y ?? primary1RawBounds.y,
		Width: primary1RawBounds.Width ?? primary1RawBounds.width,
		Height: primary1RawBounds.Height ?? primary1RawBounds.height
	}
};
const primary1Patterns = itemStatTypePatterns.map(p => macroService.ClonePattern(p, primary1CloneOpts));
item.primary1 = macroService.PollPattern(primary1Patterns).Path?.split('.').pop()?.split('_')[0];

if (item.itemType === 'weapon') {
	const primary2RawBounds = patterns.inventory.item.stat2.primary2?.Pattern?.RawBounds || patterns.inventory.item.stat2.primary2.$patterns[0].rawBounds;
	const primary2CalcOffsetType = patterns.inventory.item.stat2.primary2?.Pattern?.OffsetCalcType ?? patterns.inventory.item.stat2.primary2.$patterns[0].offsetCalcType;
	const primary2CloneOpts = {
		PathSuffix: '_primary2',
		OffsetCalcType: primary2CalcOffsetType,
		RawBounds: {
			X: primary2RawBounds.X ?? primary2RawBounds.x,
			Y: primary2RawBounds.Y ?? primary2RawBounds.y,
			Width: primary2RawBounds.Width ?? primary2RawBounds.width,
			Height: primary2RawBounds.Height ?? primary2RawBounds.height
		}
	};
	const primary2Patterns = itemStatTypePatterns.map(p => macroService.ClonePattern(p, primary2CloneOpts));
	item.primary2 = macroService.PollPattern(primary2Patterns).Path?.split('.').pop()?.split('_')[0];
} else {
	item.primary2 = '';
}

for (let i = 1; i <= 4; i++) {
	logger.info(`secondary${i}`);
	const secondaryRawBounds = patterns.inventory.item.stat2[`secondary${i}`]?.Pattern?.RawBounds || patterns.inventory.item.stat2[`secondary${i}`].$patterns[0].rawBounds;
	const secondaryCalcOffsetType = patterns.inventory.item.stat2[`secondary${i}`]?.Pattern?.OffsetCalcType ?? patterns.inventory.item.stat2[`secondary${i}`].$patterns[0].offsetCalcType;
	const secondaryCloneOpts = {
		PathSuffix: `_secondary${i}`,
		OffsetCalcType: secondaryCalcOffsetType,
		RawBounds: {
			X: secondaryRawBounds.X ?? secondaryRawBounds.x,
			Y: secondaryRawBounds.Y ?? secondaryRawBounds.y,
			Width: secondaryRawBounds.Width ?? secondaryRawBounds.width,
			Height: secondaryRawBounds.Height ?? secondaryRawBounds.height
		}
    };
	const plusPattern = macroService.ClonePattern(patterns.inventory.item.stat2.plus, secondaryCloneOpts);
	const plusResult = macroService.FindPattern(plusPattern);
	if (!plusResult.IsSuccess) {
		item[`secondary${i}`] = '';
		item[`secondary${i}ValueType`] = '';
		continue;
	}

	const percentPattern = macroService.ClonePattern(patterns.inventory.item.stat2.percent, secondaryCloneOpts);
    const percentResult = macroService.FindPattern(percentPattern);
	const isPct = percentResult.IsSuccess;
	item[`secondary${i}ValueType`] = isPct ? 'pct' : 'flat';

	const plusWidth = patterns.inventory.item.stat2.plus?.Pattern?.RawBounds?.Width || patterns.inventory.item.stat2.plus.$patterns[0].rawBounds.width;
	const percentWidth = patterns.inventory.item.stat2.percent?.Pattern?.RawBounds?.Width || patterns.inventory.item.stat2.percent.$patterns[0].rawBounds.width;
	const plusHeight = patterns.inventory.item.stat2.plus?.Pattern?.RawBounds?.Height || patterns.inventory.item.stat2.plus.$patterns[0].rawBounds.height;
	const valueBounds = {
		X: plusResult.Point.X + (plusWidth / 2.0) + 4,
		Y: plusResult.Point.Y - (plusHeight / 2.0) - 2,
		Height: plusHeight + 4,
		Width: isPct ?
			(percentResult.Point.X - plusResult.Point.X - (plusWidth / 2.0) - (percentWidth / 2.0) - 5) :
            (secondaryCloneOpts.RawBounds.X + secondaryCloneOpts.RawBounds.Width - plusResult.Point.X - (plusWidth / 2.0)) + topLeft.X
    }

	item[`secondary${i}Value`] = macroService.FindTextWithBounds(valueBounds, '.0123456789');
	if (!item[`secondary${i}Value`] || item[`secondary${i}Value`].trim() === '') {
		throw { message: `Failed to read secondary${i}Value from OCR`, item, valueBounds };
	}

	const secondaryPatterns = itemStatTypePatterns.map(p => macroService.ClonePattern(p, secondaryCloneOpts));
	item[`secondary${i}`] = macroService.PollPattern(secondaryPatterns).Path?.split('.').pop()?.split('_')[0];
}

const armorItemTypes = ['helmet', 'chestArmor', 'gloves', 'boots'];
if (['legendary', 'epic'].includes(item.itemGrade) && armorItemTypes.includes(item.itemType)) {
    const armorSets = ['attack', 'defense', 'life', 'lifesteal', 'speed', 'criticalHit', 'criticalStrike', 'dmgIncrease',
		'dmgReduction', 'effectiveness', 'resilience', 'counterattack', 'penetration', 'revenge', 'patience',
		'pulverization', 'immunity', 'swiftness', 'weakness', 'augmentation'];
	const armorSetPatterns = armorSets.map(as => patterns.inventory.item.stat2.sets[as]);
	item.itemEffect = macroService.PollPattern(armorSetPatterns).Path?.split('.').pop();
}

validateItem(item);
calculatePoints(item);

return item;

// Validation function
function validateItem(item) {
    const validGrades = ['legendary', 'epic', 'superior'];
    const validTypes = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'];
    const validStats = ['health', 'speed', 'attack', 'defence', 'critChance', 'critDmg',
        'dmgIncrease', 'dmgReduction', 'effectiveness', 'resilience', 'penetration', 'healsWhenHit'];

    // Helper function to create error with item attached
    function createError(message) {
        return { message, item };
    }

    // Check item grade
    if (item.itemGrade && !validGrades.includes(item.itemGrade)) {
        throw createError(`Unexpected item grade: "${item.itemGrade}". Expected: ${validGrades.join(' or ')}`);
    }

    // Check item type
    if (item.itemType && !validTypes.includes(item.itemType)) {
        throw createError(`Unexpected item type: "${item.itemType}". Expected: ${validTypes.join(' or ')}`);
    }

    // Check primary stats
    if (item.primary1 && !validStats.includes(item.primary1)) {
        throw createError(`Unexpected primary1 stat: "${item.primary1}". Valid stats: ${validStats.join(', ')}`);
    }
    if (item.primary2 && !validStats.includes(item.primary2)) {
        throw createError(`Unexpected primary2 stat: "${item.primary2}". Valid stats: ${validStats.join(', ')}`);
    }

    // Check secondary stats
    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statType = item[`secondary${i}ValueType`];

        if (statName && statName !== '') {
            // Check if stat name is valid
            if (!validStats.includes(statName)) {
                throw createError(`Unexpected secondary${i} stat: "${statName}". Valid stats: ${validStats.join(', ')}`);
            }
        }

        // Check value type is valid
        if (statType && statType !== 'flat' && statType !== 'pct') {
            throw createError(`Unexpected value type for secondary${i}: "${statType}". Expected: flat or pct`);
        }
    }

    // Validate item effect for armor pieces
    const armorTypes = ['helmet', 'chestArmor', 'gloves', 'boots'];
    const validEffects = ['attack', 'defense', 'life', 'lifesteal', 'speed', 'criticalHit', 'criticalStrike', 'dmgIncrease', 'dmgReduction', 'effectiveness', 'resilience', 'counterattack', 'penetration', 'revenge', 'patience', 'pulverization', 'immunity', 'swiftness', 'weakness', 'augmentation'];
    if (armorTypes.includes(item.itemType) && item.itemEffect) {
        if (!validEffects.includes(item.itemEffect)) {
            throw createError(`Unexpected item effect: "${item.itemEffect}". Expected: ${validEffects.join(', ')}`);
        }
    }
}

// Calculate points function
function calculatePoints(item) {
    // Calculate points based on stat values
    const pointValues = {
        // Flat values
        'health_flat': 1 / 73,
        'speed_flat': 1 / 3,
        'attack_flat': 1 / 40,
        'defence_flat': 1 / 40,  // Assuming similar to Attack flat

        // Percentage values
        'health_pct': 1 / 3,
        'attack_pct': 1 / 4,
        'defence_pct': 1 / 4,
        'critChance_pct': 1 / 3,
        'critDmg_pct': 1 / 4,
        'dmgIncrease_pct': 1 / 2,
        'dmgReduction_pct': 1 / 2.4,
        'effectiveness_pct': 1 / 2.5,
        'resilience_pct': 1 / 2.5
    };

    // Calculate points for each secondary stat
    let totalPoints = 0;
    let desiredPoints = 0;
    let desiredStats = ['critDmg', 'speed', 'critChance', 'dmgIncrease'];

    // Add primary1 stat as desired stat for Accessories only (if not already in list)
    if (item.itemType === 'accessory' && item.primary1 && !desiredStats.includes(item.primary1)) {
        desiredStats.push(item.primary1);
    }

    // For non-main stat accessories, find highest percent Attack/Defense/Health
    if (item.itemType === 'accessory' && !['attack', 'defence', 'health'].includes(item.primary1)) {
        const mainStats = ['attack', 'defence', 'health'];
        let highestMainStat = null;
        let highestMainStatValue = 0;

        for (let i = 1; i <= 4; i++) {
            const statName = item[`secondary${i}`];
            const statValue = parseFloat(item[`secondary${i}Value`]);
            const statType = item[`secondary${i}ValueType`];

            if (mainStats.includes(statName) && statType === 'pct' && statValue > highestMainStatValue) {
                highestMainStat = statName;
                highestMainStatValue = statValue;
            }
        }

        if (highestMainStat) {
            desiredStats.push(highestMainStat);
        }
    }

    // For armor pieces, handle set effects
    const armorTypes = ['helmet', 'chestArmor', 'gloves', 'boots'];
    if (armorTypes.includes(item.itemType)) {
        if (item.itemEffect === 'attack') {
            // If it's Attack Set armor, add Attack as desired stat
            if (!desiredStats.includes('attack')) {
                desiredStats.push('attack');
            }
        } else if (item.itemEffect === 'defense') {
            // If it's Defense Set armor, add Defense as desired stat
            if (!desiredStats.includes('defence')) {
                desiredStats.push('defence');
            }
        } else if (item.itemEffect === 'life') {
            // If it's Life Set armor, add Health as desired stat
            if (!desiredStats.includes('health')) {
                desiredStats.push('health');
            }
        } else if (item.itemEffect === 'lifesteal') {
            // If it's Lifesteal Set armor, add highest main stat
            addHighestMainStat(item, desiredStats);
        //} else if (item.itemEffect === 'accuracy') {
        //    // If it's Accuracy Set armor, add Accuracy as desired stat
        //    if (!desiredStats.includes('dmgIncrease')) {
        //        desiredStats.push('accuracy');
        //    }
        //    // Also add highest main stat for Accuracy Set
        //    addHighestMainStat(item, desiredStats);
        } else if (item.itemEffect === 'evasion') {
            // If it's Evasion Set armor, add Evasion as desired stat
            if (!desiredStats.includes('dmgReduction')) {
                desiredStats.push('dmgReduction');
            }
            // Also add highest main stat for Evasion Set
            addHighestMainStat(item, desiredStats);
        } else if (item.itemEffect === 'effectiveness') {
            // If it's Effectiveness Set armor, add Effectiveness as desired stat
            if (!desiredStats.includes('effectiveness')) {
                desiredStats.push('effectiveness');
            }
            // Also add highest main stat for Effectiveness Set
            addHighestMainStat(item, desiredStats);
        } else if (item.itemEffect === 'revenge') {
            // If it's Revenge Set armor, add Attack as desired stat
            if (!desiredStats.includes('attack')) {
                desiredStats.push('attack');
            }
        } else if (item.itemEffect === 'patience') {
            // If it's Patience Set armor, add Defense as desired stat
            if (!desiredStats.includes('defence')) {
                desiredStats.push('defence');
            }
        } else if (item.itemEffect === 'resilience') {
            // If it's Resilience Set armor, add Resilience as desired stat
            if (!desiredStats.includes('resilience')) {
                desiredStats.push('resilience');
            }
            // Also add highest main stat for Resilience Set
            addHighestMainStat(item, desiredStats);
        } else {
            // For Critical Hit Set, Counterattack Set, and armor without set effects, find highest percent Attack/Defense/Health
            addHighestMainStat(item, desiredStats);
        }
    }

    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statValue = item[`secondary${i}Value`];
        const statType = item[`secondary${i}ValueType`];

        if (statName && statValue !== undefined && statType) {
            const key = `${statName}_${statType}`;
            const pointMultiplier = pointValues[key];

            if (pointMultiplier) {
                const points = statValue * pointMultiplier;
                totalPoints += points;

                // Check if this is a desired stat
                const isAccessory = item.itemType === 'accessory';
                const isArmor = ['helmet', 'chestArmor', 'gloves', 'boots'].includes(item.itemType);

                const isDesired = desiredStats.includes(statName) &&
                    (statName === 'critDmg' || statName === 'speed' || statName === 'critChance' ||
                        (isAccessory && statName === item.primary1 && statType === 'pct') ||
                        (isAccessory && ['attack', 'defence', 'health'].includes(statName) && statName !== item.primary1 && statType === 'pct') ||
                    (isArmor && ['attack', 'defence', 'health', 'dmgIncrease', 'dmgReduction', 'resilience', 'effectiveness'].includes(statName) && statType === 'pct'));

                if (isDesired) {
                    desiredPoints += points;
                }
            }
        }
    }

    item.totalPoints = Math.round(totalPoints * 100) / 100; // Round to 2 decimal places
    item.desiredPoints = Math.round(desiredPoints * 100) / 100; // Round to 2 decimal places
    item.desiredStats = desiredStats;

    // Check if totalPoints or desiredPoints are fractions (not whole numbers)
    if (item.totalPoints % 1 !== 0) {
        throw {
            message: `Invalid total points: ${item.totalPoints}. Points must be whole numbers, not fractions. This likely indicates OCR errors in stat values.`,
            item
        };
    }
    if (item.desiredPoints % 1 !== 0) {
        throw {
            message: `Invalid desired points: ${item.desiredPoints}. Points must be whole numbers, not fractions. This likely indicates OCR errors in stat values.`,
            item
        };
    }

    // Sanity check: max possible points is 24 (6 rolls × 4 points max per roll for Crit Dmg)
    // If total points exceeds this, there's likely an OCR error in the stat values
    if (item.totalPoints > 24) {
        throw {
            message: `Invalid total points: ${item.totalPoints}. Maximum possible is 24 points. This likely indicates OCR errors in stat values.`,
            item
        };
    }
}

// Helper function to find and add highest main stat
function addHighestMainStat(item, desiredStats) {
    const mainStats = ['attack', 'defence', 'health', 'dmgIncrease'];
    let highestMainStat = null;
    let highestMainStatValue = 0;

    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statValue = parseFloat(item[`secondary${i}Value`]);
        const statType = item[`secondary${i}ValueType`];

        if (mainStats.includes(statName) && statType === 'pct' && statValue > highestMainStatValue) {
            highestMainStat = statName;
            highestMainStatValue = statValue;
        }
    }

    if (highestMainStat && !desiredStats.includes(highestMainStat)) {
        desiredStats.push(highestMainStat);
    }
}
