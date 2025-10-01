// Helper function to clean stat names (for both primary and secondary stats)
function cleanStatName(stat) {
    if (!stat) return stat;

    // Attack OCR errors (Alleek, Attaek, etc.)
    if (stat.match(/All?e+[ck]/i) || stat.match(/Att?a[ce]k/i)) {
        return 'Attack';
    }
    // Defense OCR errors (Defene', Defence, Defens, etc.)
    if (stat.match(/Defen/i)) {
        return 'Defense';
    }
    // Health OCR errors (Heellh, Heaith, etc.)
    if (stat.match(/He[ae]l[lt]h/i)) {
        return 'Health';
    }
    // Crit Dmg OCR errors
    if (stat.includes('Cr') && stat.includes('Dmg')) {
        return 'Crit Dmg';
    }
    // Crit Chance OCR errors (Cm Chan, Cri Chan, Cm Chen, etc.)
    if ((stat.includes('Cm') || stat.includes('Cri') || stat.includes('Cr')) &&
        (stat.includes('Chan') || stat.includes('Chen') || stat.includes('Chance'))) {
        return 'Crit Chance';
    }
    // Effectiveness OCR errors
    if (stat.includes('Ef') && (stat.includes('v') || stat.includes('ﬁ'))) {
        return 'Effectiveness';
    }
    // Heals when hit OCR errors (Heels when hit, etc.)
    if (stat.match(/He[ae]ls?\s+when\s+hit/i)) {
        return 'Heals when hit';
    }

    // Check for exact matches (case insensitive)
    const lowerStat = stat.toLowerCase();
    if (lowerStat === 'speed') return 'Speed';
    if (lowerStat === 'attack') return 'Attack';
    if (lowerStat === 'defense' || lowerStat === 'defence') return 'Defense';
    if (lowerStat === 'health') return 'Health';
    if (lowerStat === 'evasion') return 'Evasion';
    if (lowerStat === 'accuracy') return 'Accuracy';
    if (lowerStat === 'effectiveness') return 'Effectiveness';
    if (lowerStat === 'resilience') return 'Resilience';
    if (lowerStat === 'penetration') return 'Penetration';
    if (lowerStat === 'heals when hit') return 'Heals when hit';

    // Capitalize first letter if no specific correction
    return stat.charAt(0).toUpperCase() + stat.slice(1);
}

// Validation function
function validateItem(item) {
    const validGrades = ['Legendary', 'Epic'];
    const validTypes = ['Weapon', 'Accessory', 'Helmet', 'Chest Armor', 'Gloves', 'Boots'];
    const validStats = ['Health', 'Speed', 'Attack', 'Defense', 'Crit Chance', 'Crit Dmg',
        'Accuracy', 'Evasion', 'Effectiveness', 'Resilience', 'Penetration', 'Heals when hit'];

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

    // Check secondary stats and their point values
    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statValue = item[`secondary${i}Value`];
        const statType = item[`secondary${i}ValueType`];

        if (statName && statName !== '') {
            // Check if stat name is valid
            if (!validStats.includes(statName)) {
                throw createError(`Unexpected secondary${i} stat: "${statName}". Valid stats: ${validStats.join(', ')}`);
            }

            // Check if we have point values for this stat/type combination
            if (statValue !== undefined && statType) {
                const key = `${statName}_${statType}`;
                const pointMultiplier = pointValues[key];

                if (!pointMultiplier) {
                    throw createError(`No point value defined for stat combination: ${key} (stat: "${statName}", type: ${statType}, value: ${statValue})`);
                }

                // Sanity check for percentage values
                if (statType === 'Pct' && statValue > 100) {
                    throw createError(`Unrealistic percentage value for ${statName}: ${statValue}%. This is likely an OCR error.`);
                }
            }
        }

        // Check value type is valid
        if (statType && statType !== 'Flat' && statType !== 'Pct') {
            throw createError(`Unexpected value type for secondary${i}: "${statType}". Expected: Flat or Pct`);
        }
    }

    // Validate item effect for armor pieces
    const armorTypes = ['Helmet', 'Chest Armor', 'Gloves', 'Boots'];
    const validEffects = ['Attack Set', 'Defense Set', 'Life Set', 'Critical Hit Set', 'Effectiveness Set', 'Resilience Set', 'Counterattack Set'];
    if (armorTypes.includes(item.itemType) && item.itemEffect) {
        if (!validEffects.includes(item.itemEffect)) {
            throw createError(`Unexpected item effect: "${item.itemEffect}". Expected: ${validEffects.join(', ')}`);
        }
    }
}

// Create raw item object with all keys in the desired order and populate with OCR data
const rawItem = {
    totalPoints: 0,
    desiredPoints: 0,
    desiredStats: [],
    itemGrade: undefined,
    itemType: macroService.FindText(patterns.inventory.item.type),
    primary1: macroService.FindText(patterns.inventory.item.stat.primary1),
    primary2: macroService.FindText(patterns.inventory.item.stat.primary2),
    itemEffect: macroService.FindText(patterns.inventory.item.effect),
    secondary1: macroService.FindText(patterns.inventory.item.stat.secondary1),
    secondary1Value: macroService.FindText(patterns.inventory.item.stat.secondary1.value),
    secondary1ValueType: undefined,
    secondary2: macroService.FindText(patterns.inventory.item.stat.secondary2),
    secondary2Value: macroService.FindText(patterns.inventory.item.stat.secondary2.value),
    secondary2ValueType: undefined,
    secondary3: macroService.FindText(patterns.inventory.item.stat.secondary3),
    secondary3Value: macroService.FindText(patterns.inventory.item.stat.secondary3.value),
    secondary3ValueType: undefined,
    secondary4: macroService.FindText(patterns.inventory.item.stat.secondary4),
    secondary4Value: macroService.FindText(patterns.inventory.item.stat.secondary4.value),
    secondary4ValueType: undefined
};

// Create a copy for processing
const item = { ...rawItem };

// Split and fix item grade and type
if (item.itemType) {
    // Split grade and type (e.g., "Epic Weapon" -> grade: "Epic", type: "Weapon")
    const parts = item.itemType.trim().split(/\s+/);

    if (parts.length >= 2) {
        item.itemGrade = parts[0];
        item.itemType = parts.slice(1).join(' ');
    } else {
        item.itemGrade = '';
        item.itemType = parts[0] || '';
    }

    // Fix common OCR errors in item type
    if (item.itemType.match(/We[aq]p/i)) {
        item.itemType = 'Weapon';
    } else if (item.itemType.match(/A[ce]+ss/i)) {
        // Handle any string with "A" followed by c's or e's then "ss" as Accessory
        item.itemType = 'Accessory';
    } else if (item.itemType.match(/Helm/i)) {
        // Keep Helmet as is
        item.itemType = 'Helmet';
    } else if (item.itemType.match(/Che[st].*A[r']?m/i)) {
        // Handle Chest Armor OCR errors (Chesl A'me', Chest Armor, etc.)
        item.itemType = 'Chest Armor';
    } else if (item.itemType.match(/Gl[eo]ve/i)) {
        // Handle Gloves OCR errors (Gleves, Gloves, etc.)
        item.itemType = 'Gloves';
    } else if (item.itemType.match(/Boot/i) || item.itemType.match(/[BPp]eel/i)) {
        // Handle Boots OCR errors (Boots, Beels, peels, etc.)
        item.itemType = 'Boots';
    }

    // Fix common OCR errors in item grade (only Epic and Legendary)
    if (item.itemGrade) {
        if (item.itemGrade.match(/Epic/i)) {
            item.itemGrade = 'Epic';
        } else if (item.itemGrade.match(/Legend/i)) {
            item.itemGrade = 'Legendary';
        }
    }
}

// Fix primary stat names
if (item.primary1) item.primary1 = cleanStatName(item.primary1);
if (item.primary2) item.primary2 = cleanStatName(item.primary2);

// Clean item effect for armor pieces
if (item.itemEffect) {
    // Remove parentheses and everything inside them first
    let cleanedEffect = item.itemEffect.split('(')[0].trim();

    // Remove quotes and apostrophes
    cleanedEffect = cleanedEffect.replace(/["']+/g, '');

    // Replace ligatures with normal characters
    cleanedEffect = cleanedEffect.replace(/\uFB01/g, 'fi').replace(/\uFB02/g, 'fl');

    // Fix set name OCR errors
    cleanedEffect = cleanedEffect
        .replace(/sel/i, 'set')  // Replace sel with set first
        .replace(/C.umem[fﬂ]l[ae]ck/i, 'Counterattack')  // Handle C°umemflack/C°umemfleck variations
        .replace(/Counter?attack/i, 'Counterattack')  // Handle normal Counterattack
        .replace(/\bA[lf]?l?[ae]?[ce]?k\b/i, 'Attack')  // Handle Allack, Alleek, etc. (\b is word boundary)
        .replace(/Defen[cs]e*/i, 'Defense')  // Handle Defense/Defence/Defensee OCR errors
        .replace(/Life\s*\d*/i, 'Life')  // Handle "Life 5" -> Life
        .replace(/C[nr]iti?c?a?l?\s*H[it]?/i, 'Critical Hit')  // Handle Critical Hit OCR errors
        .replace(/Effec[tf]i?veness/i, 'Effectiveness');  // Handle Effectiveness, Effecfiveness OCR errors

    // Normalize to proper capitalization
    cleanedEffect = cleanedEffect.trim();
    if (cleanedEffect.toLowerCase().includes('counterattack')) {
        item.itemEffect = 'Counterattack Set';
    } else if (cleanedEffect.toLowerCase().includes('attack')) {
        item.itemEffect = 'Attack Set';
    } else if (cleanedEffect.toLowerCase().includes('defense') || cleanedEffect.toLowerCase().includes('defence')) {
        item.itemEffect = 'Defense Set';
    } else if (cleanedEffect.toLowerCase().includes('life') || cleanedEffect.toLowerCase() === 'life') {
        item.itemEffect = 'Life Set';
    } else if (cleanedEffect.toLowerCase().includes('critical hit') || cleanedEffect.toLowerCase().includes('crit hit')) {
        item.itemEffect = 'Critical Hit Set';
    } else if (cleanedEffect.toLowerCase().includes('effectiveness')) {
        item.itemEffect = 'Effectiveness Set';
    } else if (cleanedEffect.toLowerCase().includes('resilience')) {
        item.itemEffect = 'Resilience Set';
    } else {
        item.itemEffect = cleanedEffect;
    }
}

// Fix secondary stat names
['secondary1', 'secondary2', 'secondary3', 'secondary4'].forEach(key => {
    if (item[key]) {
        item[key] = cleanStatName(item[key]);
    }
});

// Process and separate value types (percentage vs flat)
['secondary1', 'secondary2', 'secondary3', 'secondary4'].forEach((key, index) => {
    const valueKey = key + 'Value';
    const valueTypeKey = key + 'ValueType';

    if (item[valueKey]) {
        // Clean the value string first
        let cleanedValue = item[valueKey]
            .replace(/\+(\d+)\+0%/g, '$1%')  // Convert "+12+0%" to "12%"
            .replace(/\+/g, '') // Remove remaining plus signs
            .replace(/(\d{2})\s+0%/g, '$1%')  // Convert "12 0%" to "12%"
            .replace(/\s+0%/g, '%')  // Remove space and 0 before %
            .replace(/\s+%/g, '%')    // Remove spaces before %
            .replace(/(\d)\s+(\d)%/g, '$1.$2%') // Convert "2 5%" to "2.5%"
            .replace(/(\d)\s+(\d)(?!%)/g, '$1$2'); // Remove spaces between digits (non-percentage)

        // Check if it's a percentage or flat value
        if (cleanedValue.includes('%')) {
            item[valueKey] = parseFloat(cleanedValue.replace('%', ''));
            item[valueTypeKey] = 'Pct';
        } else {
            item[valueKey] = parseFloat(cleanedValue);
            item[valueTypeKey] = 'Flat';
        }
    }
});

// Calculate points based on stat values
const pointValues = {
    // Flat values
    'Health_Flat': 1 / 73,
    'Speed_Flat': 1 / 3,
    'Attack_Flat': 1 / 40,
    'Defense_Flat': 1 / 40,  // Assuming similar to Attack flat

    // Percentage values
    'Health_Pct': 1 / 3,
    'Attack_Pct': 1 / 4,
    'Defense_Pct': 1 / 4,
    'Defence_Pct': 1 / 4,  // Handle both spellings
    'Crit Chance_Pct': 1 / 3,
    'Crit Dmg_Pct': 1 / 4,
    'Accuracy_Pct': 1 / 2,
    'Evasion_Pct': 1 / 2,
    'Effectiveness_Pct': 1 / 2.5,
    'Resilience_Pct': 1 / 2.5
};

// Calculate points for each secondary stat
let totalPoints = 0;
let desiredPoints = 0;
let desiredStats = ['Crit Dmg', 'Speed', 'Crit Chance'];

// Add primary1 stat as desired stat for Accessories only (if not already in list)
if (item.itemType === 'Accessory' && item.primary1 && !desiredStats.includes(item.primary1)) {
    desiredStats.push(item.primary1);
}

// For non-main stat accessories, find highest percent Attack/Defense/Health
if (item.itemType === 'Accessory' && !['Attack', 'Defense', 'Health'].includes(item.primary1)) {
    const mainStats = ['Attack', 'Defense', 'Health'];
    let highestMainStat = null;
    let highestMainStatValue = 0;

    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statValue = item[`secondary${i}Value`];
        const statType = item[`secondary${i}ValueType`];

        if (mainStats.includes(statName) && statType === 'Pct' && statValue > highestMainStatValue) {
            highestMainStat = statName;
            highestMainStatValue = statValue;
        }
    }

    if (highestMainStat) {
        desiredStats.push(highestMainStat);
    }
}

// Helper function to find and add highest main stat
function addHighestMainStat(item, desiredStats) {
    const mainStats = ['Attack', 'Defense', 'Health'];
    let highestMainStat = null;
    let highestMainStatValue = 0;

    for (let i = 1; i <= 4; i++) {
        const statName = item[`secondary${i}`];
        const statValue = item[`secondary${i}Value`];
        const statType = item[`secondary${i}ValueType`];

        if (mainStats.includes(statName) && statType === 'Pct' && statValue > highestMainStatValue) {
            highestMainStat = statName;
            highestMainStatValue = statValue;
        }
    }

    if (highestMainStat && !desiredStats.includes(highestMainStat)) {
        desiredStats.push(highestMainStat);
    }
}

// For armor pieces, handle set effects
const armorTypes = ['Helmet', 'Chest Armor', 'Gloves', 'Boots'];
if (armorTypes.includes(item.itemType)) {
    if (item.itemEffect === 'Attack Set') {
        // If it's Attack Set armor, add Attack as desired stat
        if (!desiredStats.includes('Attack')) {
            desiredStats.push('Attack');
        }
    } else if (item.itemEffect === 'Defense Set') {
        // If it's Defense Set armor, add Defense as desired stat
        if (!desiredStats.includes('Defense')) {
            desiredStats.push('Defense');
        }
    } else if (item.itemEffect === 'Life Set') {
        // If it's Life Set armor, add Health as desired stat
        if (!desiredStats.includes('Health')) {
            desiredStats.push('Health');
        }
    } else if (item.itemEffect === 'Effectiveness Set') {
        // If it's Effectiveness Set armor, add Effectiveness as desired stat
        if (!desiredStats.includes('Effectiveness')) {
            desiredStats.push('Effectiveness');
        }
        // Also add highest main stat for Effectiveness Set
        addHighestMainStat(item, desiredStats);
    } else if (item.itemEffect === 'Resilience Set') {
        // If it's Resilience Set armor, add Resilience as desired stat
        if (!desiredStats.includes('Resilience')) {
            desiredStats.push('Resilience');
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
            const isAccessory = item.itemType === 'Accessory';
            const isArmor = ['Helmet', 'Chest Armor', 'Gloves', 'Boots'].includes(item.itemType);

            const isDesired = desiredStats.includes(statName) &&
                (statName === 'Crit Dmg' || statName === 'Speed' || statName === 'Crit Chance' ||
                    (isAccessory && statName === item.primary1 && statType === 'Pct') ||
                    (isAccessory && ['Attack', 'Defense', 'Health'].includes(statName) && statName !== item.primary1 && statType === 'Pct') ||
                    (isArmor && ['Attack', 'Defense', 'Health', 'Resilience', 'Effectiveness'].includes(statName) && statType === 'Pct'));

            if (isDesired) {
                desiredPoints += points;
            }
        }
    }
}

item.totalPoints = Math.round(totalPoints * 100) / 100; // Round to 2 decimal places
item.desiredPoints = Math.round(desiredPoints * 100) / 100; // Round to 2 decimal places
item.desiredStats = desiredStats;

// Validate the item
validateItem(item);

//return { item, rawItem };
return item;