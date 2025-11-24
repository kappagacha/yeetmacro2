// @tags=inventory
// @position=-20

const itemStats = {};
const itemGradePatterns = ['legendary', 'epic','superior'].map(ig => patterns.inventory.item.stat2.grade[ig]);
const itemTypePatterns = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'].map(it => patterns.inventory.item.stat2.type[it]);
const itemStatTypePatterns = ['health', 'speed', 'attack', 'defence', 'critChance', 'critDmg', 'accuracy', 'evasion', 'effectiveness', 'resilience', 'penetration', 'healsWhenHit'].map(ist => patterns.inventory.item.stat2.statType[ist]);
itemStats.itemGrade = macroService.PollPattern(itemGradePatterns).Path?.split('.').pop();
itemStats.itemType = macroService.PollPattern(itemTypePatterns).Path?.split('.').pop();

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
itemStats.primary1 = macroService.PollPattern(primary1Patterns).Path?.split('.').pop()?.split('_')[0];

if (itemStats.itemType === 'weapon') {
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
	itemStats.primary2 = macroService.PollPattern(primary2Patterns).Path?.split('.').pop()?.split('_')[0];
} else {
	itemStats.primary2 = '';
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
		itemStats[`secondary${i}`] = '';
		itemStats[`secondary${i}ValueType`] = '';
		continue;
	}

	const percentPattern = macroService.ClonePattern(patterns.inventory.item.stat2.percent, secondaryCloneOpts);
	const percentResult = macroService.FindPattern(percentPattern);
	const isPct = percentResult.IsSuccess;
	itemStats[`secondary${i}ValueType`] = isPct ? 'Pct' : 'Flat';

	const plusWidth = patterns.inventory.item.stat2.plus?.Pattern?.RawBounds?.Width || patterns.inventory.item.stat2.plus.$patterns[0].rawBounds.width;
	const percentWidth = patterns.inventory.item.stat2.percent?.Pattern?.RawBounds?.Width || patterns.inventory.item.stat2.percent.$patterns[0].rawBounds.width;
	const plusHeight = patterns.inventory.item.stat2.plus?.Pattern?.RawBounds?.Height || patterns.inventory.item.stat2.plus.$patterns[0].rawBounds.height;
	const valueBounds = {
		X: plusResult.Point.X + (plusWidth / 2.0) + 3,
		Y: plusResult.Point.Y - (plusHeight / 2.0) - 2,
		Height: plusHeight + 4,
		Width: isPct ?
			(percentResult.Point.X - plusResult.Point.X - (plusWidth / 2.0) - (percentWidth / 2.0) - 5) :
			(secondaryCloneOpts.RawBounds.X + secondaryCloneOpts.RawBounds.Width - plusResult.Point.X - (plusWidth / 2.0) - 5)
	}
	itemStats[`secondary${i}Value`] = macroService.FindTextWithBounds(valueBounds, '.0123456789');

	const secondaryPatterns = itemStatTypePatterns.map(p => macroService.ClonePattern(p, secondaryCloneOpts));
	itemStats[`secondary${i}`] = macroService.PollPattern(secondaryPatterns).Path?.split('.').pop()?.split('_')[0];
}

// TODO: itemEffect

return itemStats;