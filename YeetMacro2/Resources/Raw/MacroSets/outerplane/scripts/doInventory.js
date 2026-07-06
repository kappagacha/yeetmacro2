// @raw-script
// @tags=inventory

function doInventory(type = '') {
	if (!type) type = settings.doInventory.type.Value;

	switch (type) {
		case 'lock':
			return lockInventory();
		case 'sell':
			return sellInventory();
		case 'getItemStats':
			return getItemStats();
	}
}

function lockInventory() {
	const padding = 5;
	let lastSelectedResult = { Point: { X: 0, Y: 0 } };
	let selectedResult = macroService.PollPattern(patterns.inventory.selected);
	let isFirstIteration = true;
	let firstItemGrade = null;
	let lockedCount = 0;
	let unlockedCount = 0;
	let processedCount = 0;

	outerLoop: while ((Math.abs(lastSelectedResult.Point.X - selectedResult.Point.X) > 15 ||
			Math.abs(lastSelectedResult.Point.Y - selectedResult.Point.Y) > 15)) {
		if (!macroService.IsRunning) break;

		const itemResultLegendary = macroService.FindPattern(patterns.inventory.item.corner.legendary, { Limit: 100 });
		const itemResultEpic = macroService.FindPattern(patterns.inventory.item.corner.epic, { Limit: 100 });
		const itemResultPoints = [...(itemResultLegendary.Points ?? []), ...(itemResultEpic.Points ?? [])].map(p => ({ X: p.X - 70, Y: p.Y }));
		const minY = selectedResult.Point.Y;
		const selectedRowMinX = selectedResult.Point.X;

		const isSelectedItem = (p) => {
			return Math.abs(p.X - selectedRowMinX) <= 15 && Math.abs(p.Y - minY) <= 15;
		};

		const isAfterSelected = (p) => {
			const isOnSelectedRow = Math.abs(p.Y - minY) <= padding;
			if (isOnSelectedRow) {
				return p.X > selectedRowMinX + padding;
			} else {
				return p.Y > minY + padding;
			}
		};

		const itemsAfterSelected = itemResultPoints
			.filter(p => !isSelectedItem(p) && isAfterSelected(p))
			.sort((a, b) => {
				if (Math.abs(a.Y - b.Y) > padding) {
					return a.Y - b.Y;
				}
				return a.X - b.X;
			});

		const filteredItems = isFirstIteration
			? [selectedResult.Point, ...itemsAfterSelected]
			: itemsAfterSelected;

		for (let i = 0; i < filteredItems.length; i++) {
			if (!macroService.IsRunning) break;

			const item = filteredItems[i];
			const isLastItem = i === filteredItems.length - 1;

			const yellowStarPattern = macroService.ClonePattern(patterns.inventory.item.yellowStar, { OffsetCalcType: 'None', X: item.X, Y: item.Y - 30, Width: 145, Height: 25, PathSuffix: `_${item.X}x${item.Y}y` });
			const yellowStarResult = macroService.FindPattern(yellowStarPattern, { Limit: 6 });
			const numYellowStars = yellowStarResult.Points?.map(p => p).length || 0;

			const redStarPattern = macroService.ClonePattern(patterns.inventory.item.redStar, { OffsetCalcType: 'None', X: item.X, Y: item.Y - 30, Width: 145, Height: 25, PathSuffix: `_${item.X}x${item.Y}y` });
			const redStarResult = macroService.FindPattern(redStarPattern, { Limit: 6 });
			const numRedStars = redStarResult.Points?.map(p => p).length || 0;

			const numStars = numYellowStars + numRedStars;

			if (numStars !== 6) {
				macroService.IsRunning = false;
				break;
			}

			const selectedPattern = macroService.ClonePattern(patterns.inventory.selected, { OffsetCalcType: 'None', CenterX: item.X, CenterY: item.Y, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
			macroService.PollPoint({ X: item.X + 60, Y: item.Y - 60 }, { PredicatePattern: selectedPattern });
			sleep(500);

			if (isLastItem) continue;

			let itemStats = getItemStats();

			if (firstItemGrade === null) {
				firstItemGrade = itemStats.itemGrade;
			}

			if (itemStats.itemGrade !== firstItemGrade) {
				macroService.IsRunning = false;
				break;
			}

			if (!['legendary', 'epic'].includes(itemStats.itemGrade)) continue;

			processedCount++;

			if (itemStats.itemGrade === 'legendary') {
				if (numYellowStars !== 6) continue;

				const numDesiredStats = itemStats.desiredStats.filter(stat =>
					[itemStats.secondary1, itemStats.secondary2, itemStats.secondary3, itemStats.secondary4].includes(stat)
				).length;

				const shouldLock = itemStats.desiredPoints >= 7 && numDesiredStats >= 3;
				const lockedPattern = macroService.ClonePattern(patterns.inventory.item.locked, { OffsetCalcType: 'None', CenterX: item.X + 10, CenterY: item.Y - 80, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });

				if (shouldLock) {
					macroService.PollPattern(patterns.inventory.item.stat.unlocked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: lockedPattern });
					lockedCount++;
				} else {
					const lockedResult = macroService.FindPattern(lockedPattern);
					if (lockedResult.IsSuccess) {
						macroService.PollPattern(patterns.inventory.item.stat.locked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, InversePredicatePattern: lockedPattern });
					}
					unlockedCount++;
				}
			} else {
				const lockedPattern = macroService.ClonePattern(patterns.inventory.item.locked, { OffsetCalcType: 'None', CenterX: item.X + 10, CenterY: item.Y - 80, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
				const lockedResult = macroService.FindPattern(lockedPattern);
				if (lockedResult.IsSuccess) continue;

				if (numRedStars === 0 && itemStats.desiredPoints >= 6) {
					macroService.PollPattern(patterns.inventory.enhance, { DoClick: true, PredicatePattern: patterns.titles.improveGear });
					macroService.PollPattern(patterns.inventory.improveGear.reforge, { DoClick: true, PredicatePattern: patterns.inventory.improveGear.reforge.reforge });
					macroService.PollPattern(patterns.inventory.improveGear.reforge.reforge, { DoClick: true, IntervalDelayMs: 4_000, PrimaryClickInversePredicatePattern: patterns.inventory.improveGear.reforge.redStar, PredicatePattern: patterns.inventory.improveGear.reforge.redStar });
					macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickPredicatePattern: patterns.titles.improveGear, PredicatePattern: patterns.titles.inventory });

					lastSelectedResult = { Point: { X: 0, Y: 0 } };
					selectedResult = macroService.PollPattern(patterns.inventory.selected);
					isFirstIteration = true;
					continue outerLoop;
				}

				const numDesiredStats = itemStats.desiredStats.filter(stat =>
					[itemStats.secondary1, itemStats.secondary2, itemStats.secondary3, itemStats.secondary4].includes(stat)
				).length;

				if (itemStats.desiredPoints >= 8 && numDesiredStats >= 3) {
					macroService.PollPattern(patterns.inventory.item.stat.unlocked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: lockedPattern });
					lockedCount++;
				}
			}

			sleep(1_000);
		}

		if (!macroService.IsRunning) break;
		isFirstIteration = false;
		lastSelectedResult = selectedResult;
		macroService.SwipePattern(patterns.inventory.swipeDown);
		sleep(2_000);
		selectedResult = macroService.PollPattern(patterns.inventory.selected);
	}

	const searchType = firstItemGrade === 'legendary' ? 'Legendary' : 'Epic';
	const summary = `Inventory processing complete.\nSearch type: ${searchType}\nProcessed: ${processedCount}\nLocked: ${lockedCount}\nUnlocked: ${unlockedCount}`;
	return summary;
}

function sellInventory() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.inventory];
	const daily = dailyManager.GetCurrentDaily();
	const weekly = weeklyManager.GetCurrentWeekly();
	const doDismantle = !weekly.doInventory.sell.doDismantleOnce.IsChecked;

	if (daily.doInventory.sell.IsChecked && !settings.doInventory.sell.forceRun.Value) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('sellInventory: click inventory tab');
				macroService.ClickPattern(patterns.tabs.inventory);
				sleep(500);
				break;
			case 'titles.inventory':
				logger.info('sellInventory: go to survey hub');
				sellNormalSuperiorAndEpic(doDismantle);
				//sellLowStarEpic(doDismantle);

				if (macroService.IsRunning) {
					if (doDismantle) weekly.doInventory.sell.doDismantleOnce.IsChecked = true;
					daily.doInventory.sell.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}

	function sellNormalSuperiorAndEpic(doDismantle) {
		const action = doDismantle ? 'dismantle' : 'sell';

		macroService.PollPattern(patterns.inventory[action], { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
		macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
		macroService.PollPattern(patterns.inventory.sell.auto.grade.normal.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.normal.enabled });
		macroService.PollPattern(patterns.inventory.sell.auto.grade.superior.disabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.grade.superior.enabled });
		macroService.PollPattern(patterns.inventory.filter.grade.epic.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.epic.enabled });

		const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory[action].enabled, patterns.inventory[action].disabled] });
		if (sellResult.PredicatePath === `inventory.${action}.disabled`) {
			return;
		}
		macroService.PollPattern(patterns.inventory[action].enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
		macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
	}

	function sellLowStarEpic(doDismantle) {
		const action = doDismantle ? 'dismantle' : 'sell';
		macroService.PollPattern(patterns.inventory[action], { DoClick: true, PredicatePattern: patterns.inventory.sell.auto });
		macroService.PollPattern(patterns.inventory.sell.auto, { DoClick: true, PredicatePattern: patterns.inventory.sell.auto.apply });
		for (let i = 1; i < 6; i++) {
			macroService.PollPattern(patterns.inventory.filter.starLevel[i].disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.starLevel[i].enabled });
		}
		macroService.PollPattern(patterns.inventory.filter.grade.epic.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.epic.enabled });

		const sellResult = macroService.PollPattern(patterns.inventory.sell.auto.apply, { DoClick: true, PredicatePattern: [patterns.inventory[action].enabled, patterns.inventory[action].disabled] });
		if (sellResult.PredicatePath === `inventory.${action}.disabled`) {
			return;
		}
		macroService.PollPattern(patterns.inventory[action].enabled, { DoClick: true, PredicatePattern: patterns.inventory.sell.ok });
		macroService.PollPattern(patterns.inventory.sell.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.inventory });
	}
}

function getItemStats() {
	const topLeft = macroService.GetTopLeft();
	const item = { totalPoints: 0, desiredPoints: 0, desiredStats: [] };
	const itemGradePatterns = ['legendary', 'epic', 'superior'].map(ig => patterns.inventory.item.stat2.grade[ig]);
	const itemTypePatterns = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'].map(it => patterns.inventory.item.stat2.type[it]);
	const itemStatTypePatterns = ['health', 'speed', 'attack', 'defence', 'critChance', 'critDmg', 'dmgIncrease', 'dmgReduction', 'effectiveness', 'resilience', 'penetration', 'critDmgReduction'].map(ist => patterns.inventory.item.stat2.statType[ist]);
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
		const armorSets = ['attack', 'defense', 'life', 'lifesteal', 'speed', 'criticalHit', 'criticalStrike', 'mitigation',
			'fortification', 'effectiveness', 'resilience', 'counterattack', 'penetration', 'revenge', 'patience',
			'pulverization', 'immunity', 'swiftness', 'weakness', 'augmentation'];
		const armorSetPatterns = armorSets.map(as => patterns.inventory.item.stat2.sets[as]);
		item.itemEffect = macroService.PollPattern(armorSetPatterns).Path?.split('.').pop();
	}

	validateItem(item);
	calculatePoints(item);

	return item;

	function validateItem(item) {
		const validGrades = ['legendary', 'epic', 'superior'];
		const validTypes = ['weapon', 'accessory', 'helmet', 'chestArmor', 'gloves', 'boots'];
		const validStats = ['health', 'speed', 'attack', 'defence', 'critChance', 'critDmg',
			'dmgIncrease', 'dmgReduction', 'effectiveness', 'resilience', 'penetration', 'critDmgReduction'];

		function createError(message) {
			return { message, item };
		}

		if (item.itemGrade && !validGrades.includes(item.itemGrade)) {
			throw createError(`Unexpected item grade: "${item.itemGrade}". Expected: ${validGrades.join(' or ')}`);
		}

		if (item.itemType && !validTypes.includes(item.itemType)) {
			throw createError(`Unexpected item type: "${item.itemType}". Expected: ${validTypes.join(' or ')}`);
		}

		if (item.primary1 && !validStats.includes(item.primary1)) {
			throw createError(`Unexpected primary1 stat: "${item.primary1}". Valid stats: ${validStats.join(', ')}`);
		}
		if (item.primary2 && !validStats.includes(item.primary2)) {
			throw createError(`Unexpected primary2 stat: "${item.primary2}". Valid stats: ${validStats.join(', ')}`);
		}

		for (let i = 1; i <= 4; i++) {
			const statName = item[`secondary${i}`];
			const statType = item[`secondary${i}ValueType`];

			if (statName && statName !== '') {
				if (!validStats.includes(statName)) {
					throw createError(`Unexpected secondary${i} stat: "${statName}". Valid stats: ${validStats.join(', ')}`);
				}
			}

			if (statType && statType !== 'flat' && statType !== 'pct') {
				throw createError(`Unexpected value type for secondary${i}: "${statType}". Expected: flat or pct`);
			}
		}

		const armorTypes = ['helmet', 'chestArmor', 'gloves', 'boots'];
		const validEffects = ['attack', 'defense', 'life', 'lifesteal', 'speed', 'criticalHit', 'criticalStrike', 'mitigation', 'fortification', 'effectiveness', 'resilience', 'counterattack', 'penetration', 'revenge', 'patience', 'pulverization', 'immunity', 'swiftness', 'weakness', 'augmentation'];
		if (armorTypes.includes(item.itemType) && item.itemEffect) {
			if (!validEffects.includes(item.itemEffect)) {
				throw createError(`Unexpected item effect: "${item.itemEffect}". Expected: ${validEffects.join(', ')}`);
			}
		}
	}

	function calculatePoints(item) {
		const pointValues = {
			'health_flat': 1 / 73,
			'speed_flat': 1 / 3,
			'attack_flat': 1 / 40,
			'defence_flat': 1 / 40,
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

		let totalPoints = 0;
		let desiredPoints = 0;
		let desiredStats = ['critDmg', 'speed', 'critChance', 'dmgIncrease'];

		if (item.itemType === 'accessory' && item.primary1 && !desiredStats.includes(item.primary1)) {
			desiredStats.push(item.primary1);
		}

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

		const armorTypes = ['helmet', 'chestArmor', 'gloves', 'boots'];
		if (armorTypes.includes(item.itemType)) {
			if (item.itemEffect === 'attack') {
				if (!desiredStats.includes('attack')) {
					desiredStats.push('attack');
				}
			} else if (item.itemEffect === 'defense') {
				if (!desiredStats.includes('defence')) {
					desiredStats.push('defence');
				}
			} else if (item.itemEffect === 'life') {
				if (!desiredStats.includes('health')) {
					desiredStats.push('health');
				}
			} else if (item.itemEffect === 'lifesteal') {
				addHighestMainStat(item, desiredStats);
			} else if (item.itemEffect === 'effectiveness') {
				if (!desiredStats.includes('effectiveness')) {
					desiredStats.push('effectiveness');
				}
				addHighestMainStat(item, desiredStats);
			} else if (item.itemEffect === 'revenge') {
				if (!desiredStats.includes('attack')) {
					desiredStats.push('attack');
				}
			} else if (item.itemEffect === 'patience') {
				if (!desiredStats.includes('defence')) {
					desiredStats.push('defence');
				}
			} else if (item.itemEffect === 'resilience') {
				if (!desiredStats.includes('resilience')) {
					desiredStats.push('resilience');
				}
				addHighestMainStat(item, desiredStats);
			} else {
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

					const isAccessory = item.itemType === 'accessory';
					const isArmor = ['helmet', 'chestArmor', 'gloves', 'boots'].includes(item.itemType);

					const isDesired = desiredStats.includes(statName) &&
						(statName === 'critDmg' || statName === 'speed' || statName === 'critChance' || statName === 'dmgIncrease' ||
							(isAccessory && statName === item.primary1 && statType === 'pct') ||
							(isAccessory && ['attack', 'defence', 'health'].includes(statName) && statName !== item.primary1 && statType === 'pct') ||
							(isArmor && ['attack', 'defence', 'health', 'dmgIncrease', 'dmgReduction', 'resilience', 'effectiveness'].includes(statName) && statType === 'pct'));

					if (isDesired) {
						desiredPoints += points;
					}
				}
			}
		}

		item.totalPoints = Math.round(totalPoints * 100) / 100;
		item.desiredPoints = Math.round(desiredPoints * 100) / 100;
		item.desiredStats = desiredStats;

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

		if (item.totalPoints > 24) {
			throw {
				message: `Invalid total points: ${item.totalPoints}. Maximum possible is 24 points. This likely indicates OCR errors in stat values.`,
				item
			};
		}
	}

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
}
