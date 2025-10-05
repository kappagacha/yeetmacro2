// @isFavorite
// @position=-20

// Helper function to clean stat names (for both primary and secondary stats)
function cleanStatName(stat) {
    if (!stat) return stat;

    const lowerCaseStat = stat.toLowerCase();

    // Attack OCR errors (Alleek, Attaek, Affeck, Arreck, Aneck, Affgck, Assesk, etc.)
    if (stat.match(/^A[lftrns]{1,2}[aefgs]*[ck]/i)) {
        return 'Attack';
    }
    // Defense OCR errors (Defene', Defence, Defens, etc.)
    if (stat.match(/Defen/i)) {
        return 'Defense';
    }
    // Heals when hit OCR errors (Heels when hit, Heals when h", etc.) - check before Health
    if (stat.match(/He[ae]ls?\s+when\s+h/i)) {
        return 'Heals when hit';
    }
    // Health OCR errors (Heellh, Heaith, Healln, Healrh, Heann, Heanh, Hean'n, Heah'h, Heulrh, etc.)
    if (stat.match(/He[ae][alnhu]['tlrnh]?['hnr]?/i)) {
        return 'Health';
    }
    // Crit Dmg OCR errors (Cr Dmg, Cm Dmg, cm Dmg, C'n Dmg, Crn Drng, Cm Dme, etc.)
    if ((lowerCaseStat.includes('cr') || lowerCaseStat.includes('cm') || lowerCaseStat.includes("c'")) && (lowerCaseStat.includes('dmg') || lowerCaseStat.includes('dme') || lowerCaseStat.includes('drng') || lowerCaseStat.includes('drn') || lowerCaseStat.includes('d') && lowerCaseStat.includes('ng'))) {
        return 'Crit Dmg';
    }
    // Additional Crit Dmg fallback check
    if (stat.match(/C[rmn]{1,2}\s*D[mrn]{1,2}[egn]{1,2}/i)) {
        return 'Crit Dmg';
    }
    // Crit Chance OCR errors (Cm Chan, Cri Chan, Cm Chen, cm chan, C'n Chan, Cnt Chance, etc.)
    if ((lowerCaseStat.includes('cm') || lowerCaseStat.includes('cri') || lowerCaseStat.includes('cr') || lowerCaseStat.includes("c'") || lowerCaseStat.includes('cnt')) &&
        (lowerCaseStat.includes('chan') || lowerCaseStat.includes('chen') || lowerCaseStat.includes('chance'))) {
        return 'Crit Chance';
    }
    // Accuracy OCR errors (Am"racy, Aeeuracy, Accuracy, Acuracy, Am"lacy, Aeeureey, etc.)
    if (lowerCaseStat.includes('racy') || lowerCaseStat.includes('lacy') || lowerCaseStat.includes('ccuracy') || lowerCaseStat.includes('ureey') || lowerCaseStat.includes('uracy')) {
        return 'Accuracy';
    }
    // Effectiveness OCR errors (Errecuveness, Effecfiveness, Errecrlver'ess, Errecuvene", etc.)
    if (stat.match(/E[rf]+[ert]+[ceo]+[ut]*[vf][ei]*[vn]+[ea]+[sn"]+/i)) {
        return 'Effectiveness';
    }
    if (lowerCaseStat.includes('ef') && (lowerCaseStat.includes('v') || stat.includes('ﬁ'))) {
        return 'Effectiveness';
    }
    if ((lowerCaseStat.startsWith('err') || lowerCaseStat.startsWith('eff')) && (lowerCaseStat.includes('ess') || lowerCaseStat.includes('ene'))) {
        return 'Effectiveness';
    }

    // Speed OCR errors (Speea, etc.)
    if (stat.match(/Spe+[da]/i)) {
        return 'Speed';
    }

    // Evasion OCR errors (Eveslen, Evesion, Eveeien, etc.)
    if (stat.match(/Ev[ea][seil]+[eio]*[eno]+n?/i)) {
        return 'Evasion';
    }

    // Resilience OCR errors (Reelllenee, etc.)
    if (stat.match(/Re[el]+[il]+[en]+e+/i)) {
        return 'Resilience';
    }

    // Penetration OCR errors (Penel'aﬁen, Penetrafim, etc.)
    if (stat.match(/Pene[lt].+[eto]*n/i) || lowerCaseStat.includes('penel') || lowerCaseStat.includes('penet')) {
        return 'Penetration';
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
function validateItem(item, rawItem) {
    const validGrades = ['Legendary', 'Epic', 'Superior'];
    const validTypes = ['Weapon', 'Accessory', 'Helmet', 'Chest Armor', 'Gloves', 'Boots'];
    const validStats = ['Health', 'Speed', 'Attack', 'Defense', 'Crit Chance', 'Crit Dmg',
        'Accuracy', 'Evasion', 'Effectiveness', 'Resilience', 'Penetration', 'Heals when hit'];

    // Helper function to create error with item and rawItem attached
    function createError(message) {
        return { message, item, rawItem };
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
    const validEffects = ['Attack Set', 'Defense Set', 'Life Set', 'Lifesteal Set', 'Speed Set', 'Critical Hit Set', 'Critical Strike Set', 'Accuracy Set', 'Evasion Set', 'Effectiveness Set', 'Resilience Set', 'Counterattack Set', 'Penetration Set', 'Revenge Set', 'Patience Set', 'Pulverization Set', 'Immunity Set', 'Swiftness Set', 'Weakness Set', 'Augmentation Set'];
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
    // Remove extra spaces first
    const cleaned = item.itemType.trim().replace(/\s+/g, ' ');

    // Try to match known grades with various OCR errors
    let grade = '';
    let type = cleaned;

    if (cleaned.match(/^(Legend[ae]r[yi]|Legen[dt]|Legen[ea]+['yr]+y?|chcndmy)/i)) {
        grade = 'Legendary';
        type = cleaned.replace(/^(Legend[ae]r[yi]|Legen[dt]|Legen[ea]+['yr]+y?|chcndmy)\s*/i, '');
    } else if (cleaned.match(/^Epic/i)) {
        grade = 'Epic';
        type = cleaned.replace(/^Epic\s*/i, '');
    } else if (cleaned.match(/^[Ss]\s*u?\s*perior/i)) {
        grade = 'Superior';
        type = cleaned.replace(/^[Ss]\s*u?\s*perior\s*/i, '');
    } else {
        // Fallback to old logic
        const parts = cleaned.split(/\s+/);
        if (parts.length >= 2) {
            grade = parts[0];
            type = parts.slice(1).join(' ');
        } else {
            grade = '';
            type = parts[0] || '';
        }
    }

    item.itemGrade = grade;
    item.itemType = type;

    // Fix common OCR errors in item type
    // Check Accessory first before Exclusive, since accessories can also have "Exclusive" in the name
    if (item.itemType.match(/A[ce]+ss[eo]ry/i)) {
        // Handle Accessory OCR errors (Aeecessery, Acccssory, etc.)
        item.itemType = 'Accessory';
    } else if (item.itemType.match(/We[aeq]+p[eo]n/i) || item.itemType.match(/Exclusive/i)) {
        // Handle Weapon, Weepen, Weapen, and Exclusive (character-specific weapons)
        item.itemType = 'Weapon';
    } else if (item.itemType.match(/Hel\s*m/i)) {
        // Handle Helmet OCR errors (Helmet, Hel met, Helm, etc.)
        item.itemType = 'Helmet';
    } else if (item.itemType.match(/C[hn]e[st][st]?[rt]?\s*A[r']?m/i)) {
        // Handle Chest Armor OCR errors (Chesl A'me', Chest Armor, Cnesr Armor, etc.)
        item.itemType = 'Chest Armor';
    } else if (item.itemType.match(/Gl[eo]ve/i)) {
        // Handle Gloves OCR errors (Gleves, Gloves, etc.)
        item.itemType = 'Gloves';
    } else if (item.itemType.match(/Boot/i) || item.itemType.match(/[BPp]eel/i) || item.itemType.match(/Beet/i)) {
        // Handle Boots OCR errors (Boots, Beels, peels, Beets, etc.)
        item.itemType = 'Boots';
    }

    // Fix common OCR errors in item grade (Epic, Legendary, Superior)
    if (item.itemGrade) {
        if (item.itemGrade.match(/Epic/i)) {
            item.itemGrade = 'Epic';
        } else if (item.itemGrade.match(/Legend/i)) {
            item.itemGrade = 'Legendary';
        } else if (item.itemGrade.match(/Superior/i)) {
            item.itemGrade = 'Superior';
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
        .replace(/se[rl]/i, 'set')  // Replace sel/ser with set first
        .replace(/C.u[a-z]*[lf]?[ae][cf]k/i, 'Counterattack')  // Handle C°umemflack/C°unleraflack/C°umemfleck/etc variations
        .replace(/Counter?attack/i, 'Counterattack')  // Handle normal Counterattack
        .replace(/\bA[lf]?l?[ae]?[ce]?k\b/i, 'Attack')  // Handle Allack, Alleek, etc. (\b is word boundary)
        .replace(/Defen[cs]e*/i, 'Defense')  // Handle Defense/Defence/Defensee OCR errors
        .replace(/Li[frt]es[lrt]e[ae][lt]/i, 'Lifesteal')  // Handle Lifesleel, Lifesteet, Liresreal, Litesteal, etc.
        .replace(/Life\s*\d*/i, 'Life')  // Handle "Life 5" -> Life
        .replace(/[Ss]peed\s*\d*/i, 'Speed')  // Handle "sPeed 5" -> Speed
        .replace(/Critical\s*H["\s]*/i, 'Critical Hit')  // Handle truncated "Critical H"" OCR error
        .replace(/C[rlni'"][rlni]+[cait]+[al]+\s+[sS][lti'r]+[rlki]+[ke]+e?/i, 'Critical Strike')  // Handle Critical Strike/Crnical srrn'e/Clllcal Sl'lke/Crrrrcal Srrrke OCR errors
        .replace(/C[rn'"][ritfn][a-z]+\s+H[it"\s]*/i, 'Critical Hit')  // Handle Critical Hit variations like C'ifical H"
        .replace(/A[mc"]+[ua]*r[au]cy/i, 'Accuracy')  // Handle Accuracy OCR errors
        .replace(/Ev[ae]s[eisr][a-z°']+/i, 'Evasion')  // Handle Evasr°n, Evesim', Evasi°n, Evesi°n OCR errors
        .replace(/Effec[tf]i?veness/i, 'Effectiveness')  // Handle Effectiveness, Effecfiveness OCR errors
        .replace(/Pene[a-z'°]+/i, 'Penetration')  // Handle Penefrefim', Penerrerr°n, Penefrafim' OCR errors
        .replace(/P[aegfl][atfgnlr]?[ief]?[enrl]+[nc]e/i, 'Patience')  // Handle Pafience/Pefience/Pgfience/Panence/Pallence/Parrence OCR errors
        .replace(/Pulv[a-z°]+n/i, 'Pulverization')  // Handle Pulvetlzatl°n/Pulvenzatl°n/Pulveizati°n OCR errors
        .replace(/Sw[il][lf][lt]?ness/i, 'Swiftness')  // Handle Swiflness, Swlflness OCR errors
        .replace(/Aug[mn][ea]n[a-z'°]+/i, 'Augmentation')  // Handle Augmenlalmn, Augmenlafim', Augnentatl°n OCR errors
        .replace(/[Il]mmu[nm]+y/i, 'Immunity');  // Handle Immunny, lmmunny OCR errors

    // Normalize to proper capitalization
    cleanedEffect = cleanedEffect.trim();
    if (cleanedEffect.toLowerCase().includes('counterattack')) {
        item.itemEffect = 'Counterattack Set';
    } else if (cleanedEffect.toLowerCase().includes('revenge')) {
        item.itemEffect = 'Revenge Set';
    } else if (cleanedEffect.toLowerCase().includes('patience')) {
        item.itemEffect = 'Patience Set';
    } else if (cleanedEffect.toLowerCase().includes('attack')) {
        item.itemEffect = 'Attack Set';
    } else if (cleanedEffect.toLowerCase().includes('defense') || cleanedEffect.toLowerCase().includes('defence')) {
        item.itemEffect = 'Defense Set';
    } else if (cleanedEffect.toLowerCase().includes('lifesteal')) {
        item.itemEffect = 'Lifesteal Set';
    } else if (cleanedEffect.toLowerCase().includes('life') || cleanedEffect.toLowerCase() === 'life') {
        item.itemEffect = 'Life Set';
    } else if (cleanedEffect.toLowerCase().includes('speed')) {
        item.itemEffect = 'Speed Set';
    } else if (cleanedEffect.toLowerCase().includes('critical strike') || cleanedEffect.toLowerCase().includes('crit strike') || (cleanedEffect.toLowerCase().includes('crnical') || cleanedEffect.toLowerCase().includes('cnical') || cleanedEffect.toLowerCase().includes('critic')) && (cleanedEffect.toLowerCase().includes('str') || cleanedEffect.toLowerCase().includes('srr'))) {
        item.itemEffect = 'Critical Strike Set';
    } else if (cleanedEffect.toLowerCase().includes('critical hit') || cleanedEffect.toLowerCase().includes('crit hit') || (cleanedEffect.toLowerCase().includes('ifical') && cleanedEffect.toLowerCase().includes('h'))) {
        item.itemEffect = 'Critical Hit Set';
    } else if (cleanedEffect.toLowerCase().includes('accuracy')) {
        item.itemEffect = 'Accuracy Set';
    } else if (cleanedEffect.toLowerCase().includes('evasion')) {
        item.itemEffect = 'Evasion Set';
    } else if (cleanedEffect.toLowerCase().includes('effectiveness')) {
        item.itemEffect = 'Effectiveness Set';
    } else if (cleanedEffect.toLowerCase().includes('resilience')) {
        item.itemEffect = 'Resilience Set';
    } else if (cleanedEffect.toLowerCase().includes('penetration')) {
        item.itemEffect = 'Penetration Set';
    } else if (cleanedEffect.toLowerCase().includes('pulverization') || cleanedEffect.toLowerCase().includes('pulv')) {
        item.itemEffect = 'Pulverization Set';
    } else if (cleanedEffect.toLowerCase().includes('immunity')) {
        item.itemEffect = 'Immunity Set';
    } else if (cleanedEffect.toLowerCase().includes('swiftness')) {
        item.itemEffect = 'Swiftness Set';
    } else if (cleanedEffect.toLowerCase().includes('weakness')) {
        item.itemEffect = 'Weakness Set';
    } else if (cleanedEffect.toLowerCase().includes('augmentation')) {
        item.itemEffect = 'Augmentation Set';
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
            .replace(/\+(\d)(\d)(96|06)$/g, '+$1.$2%')  // Convert "+2596" or "+5006" to "+2.5%" or "+5.0%" (96 or 06 are % symbol OCR errors)
            .replace(/\+(\d)(\d)\d{2,}$/g, '+$1.$2%')  // Convert other 4+ digit patterns like "+2599" to "+2.5%"
            .replace(/\+\d*(\d)(\d)40%/g, '+$1$2%')  // Convert "+1240%" to "+20%" (12 and 40 are OCR noise around 20)
            .replace(/\+(\d+)\s+[04]0%/g, '+$1%')  // Convert "+12 40%" or "+2 0%" to "+12%" or "+2%" (40 is OCR error for 0)
            .replace(/\+/g, '') // Remove remaining plus signs
            .replace(/(\d+)\s+[04]0%/g, '$1%')  // Convert "12 40%" or "2 0%" to "12%" or "2%" (without +)
            .replace(/\s+0%/g, '%')  // Remove space and 0 before %
            .replace(/(\d)\s+(\d)%/g, '$1.$2%') // Convert "2 5%" to "2.5%"
            .replace(/\s+%/g, '%')    // Remove spaces before %
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

// Fix percentage-only stats that were incorrectly detected as Flat
// These stats can ONLY be percentages in the game
const percentageOnlyStats = ['Crit Chance', 'Crit Dmg', 'Accuracy', 'Evasion', 'Effectiveness', 'Resilience'];

// Max values for percentage stats (max rolls)
const maxPercentageValues = {
    'Crit Chance': 18,  // 6 rolls × 3%
    'Crit Dmg': 24,     // 6 rolls × 4%
    'Accuracy': 12,     // 6 rolls × 2%
    'Evasion': 12,      // 6 rolls × 2%
    'Effectiveness': 16, // 6 rolls × 2.5% (rounded)
    'Resilience': 16,   // 6 rolls × 2.5% (rounded)
    'Attack': 24,       // 6 rolls × 4%
    'Defense': 24,      // 6 rolls × 4%
    'Health': 18        // 6 rolls × 3%
};

// Max values for flat stats (reasonable upper bounds)
const maxFlatValues = {
    'Health': 438,      // 6 rolls × 73
    'Attack': 240,      // 6 rolls × 40
    'Defense': 240,     // 6 rolls × 40
    'Speed': 18         // 6 rolls × 3
};

['secondary1', 'secondary2', 'secondary3', 'secondary4'].forEach(key => {
    const statName = item[key];
    const valueKey = key + 'Value';
    const valueTypeKey = key + 'ValueType';

    if (percentageOnlyStats.includes(statName) && item[valueTypeKey] === 'Flat') {
        // This stat can only be a percentage, OCR failed to detect %
        // Try to extract likely percentage value from the number
        let value = item[valueKey];
        const maxValue = maxPercentageValues[statName] || 25;

        // If value is very large (like 8006, 12006), it's likely OCR garbage
        if (value > 100) {
            const valueStr = value.toString();

            // Try first 2 digits (handles 12006 → 12)
            let candidate = parseInt(valueStr.substring(0, 2));
            if (candidate <= maxValue) {
                value = candidate;
            } else {
                // Try first digit (handles 8006 → 8)
                candidate = parseInt(valueStr.substring(0, 1));
                if (candidate <= maxValue) {
                    value = candidate;
                } else {
                    // Give up, set to 0
                    value = 0;
                }
            }
        } else if (value > maxValue) {
            // Value is between 100 and maxValue, try first digit
            const valueStr = value.toString();
            const candidate = parseInt(valueStr.substring(0, 1));
            if (candidate <= maxValue) {
                value = candidate;
            } else {
                value = 0;
            }
        }

        item[valueKey] = value;
        item[valueTypeKey] = 'Pct';
    } else if (item[valueTypeKey] === 'Flat' && statName) {
        // Check if flat value is suspiciously large (likely OCR reading "80%" as "8006")
        let value = item[valueKey];
        const maxFlat = maxFlatValues[statName];

        if (maxFlat && value > maxFlat) {
            // Value is too large for flat, likely a percentage OCR error
            const maxPct = maxPercentageValues[statName];
            if (maxPct) {
                const valueStr = value.toString();

                // Try first 2 digits
                let candidate = parseInt(valueStr.substring(0, 2));
                if (candidate <= maxPct) {
                    item[valueKey] = candidate;
                    item[valueTypeKey] = 'Pct';
                } else {
                    // Try first digit
                    candidate = parseInt(valueStr.substring(0, 1));
                    if (candidate <= maxPct) {
                        item[valueKey] = candidate;
                        item[valueTypeKey] = 'Pct';
                    } else {
                        // Set to 0
                        item[valueKey] = 0;
                        item[valueTypeKey] = 'Pct';
                    }
                }
            }
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
    } else if (item.itemEffect === 'Lifesteal Set') {
        // If it's Lifesteal Set armor, add highest main stat
        addHighestMainStat(item, desiredStats);
    } else if (item.itemEffect === 'Accuracy Set') {
        // If it's Accuracy Set armor, add Accuracy as desired stat
        if (!desiredStats.includes('Accuracy')) {
            desiredStats.push('Accuracy');
        }
        // Also add highest main stat for Accuracy Set
        addHighestMainStat(item, desiredStats);
    } else if (item.itemEffect === 'Evasion Set') {
        // If it's Evasion Set armor, add Evasion as desired stat
        if (!desiredStats.includes('Evasion')) {
            desiredStats.push('Evasion');
        }
        // Also add highest main stat for Evasion Set
        addHighestMainStat(item, desiredStats);
    } else if (item.itemEffect === 'Effectiveness Set') {
        // If it's Effectiveness Set armor, add Effectiveness as desired stat
        if (!desiredStats.includes('Effectiveness')) {
            desiredStats.push('Effectiveness');
        }
        // Also add highest main stat for Effectiveness Set
        addHighestMainStat(item, desiredStats);
    } else if (item.itemEffect === 'Revenge Set') {
        // If it's Revenge Set armor, add Attack as desired stat
        if (!desiredStats.includes('Attack')) {
            desiredStats.push('Attack');
        }
    } else if (item.itemEffect === 'Patience Set') {
        // If it's Patience Set armor, add Defense as desired stat
        if (!desiredStats.includes('Defense')) {
            desiredStats.push('Defense');
        }
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
                    (isArmor && ['Attack', 'Defense', 'Health', 'Accuracy', 'Evasion', 'Resilience', 'Effectiveness'].includes(statName) && statType === 'Pct'));

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
        item,
        rawItem
    };
}
if (item.desiredPoints % 1 !== 0) {
    throw {
        message: `Invalid desired points: ${item.desiredPoints}. Points must be whole numbers, not fractions. This likely indicates OCR errors in stat values.`,
        item,
        rawItem
    };
}

// Sanity check: max possible points is 24 (6 rolls × 4 points max per roll for Crit Dmg)
// If total points exceeds this, there's likely an OCR error in the stat values
if (item.totalPoints > 24) {
    throw {
        message: `Invalid total points: ${item.totalPoints}. Maximum possible is 24 points. This likely indicates OCR errors in stat values.`,
        item,
        rawItem
    };
}

// Validate the item
validateItem(item, rawItem);

// Version: 22
//return { item, rawItem };
return item;