// @tags=inventory
// @position=-20

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
    // the points will be in the middles instead of the expected lower left corner so subtracting X by 70
    const itemResultPoints = [...(itemResultLegendary.Points ?? []), ...(itemResultEpic.Points ?? [])].map(p => ({ X: p.X - 70, Y: p.Y }));
    const minY = selectedResult.Point.Y;
    const selectedRowMinX = selectedResult.Point.X;

    const isSelectedItem = (p) => {
        // Check if point is close to the selected item (to exclude duplicates)
        // Using 15 pixel threshold to catch items that are slightly offset
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

    // All items matching patterns.inventory.item are unselected items
    // Filter out the selected item duplicate and items before it
    const itemsAfterSelected = itemResultPoints
        .filter(p => !isSelectedItem(p) && isAfterSelected(p))
        .sort((a, b) => {
            // First sort by Y (row), then by X (column)
            if (Math.abs(a.Y - b.Y) > padding) {
                return a.Y - b.Y; // Different rows
            }
            return a.X - b.X; // Same row, sort by column
        });

    // Only include selected item in first iteration
    const filteredItems = isFirstIteration
        ? [selectedResult.Point, ...itemsAfterSelected]
        : itemsAfterSelected;

    // Loop through each filtered item
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

        // If number of stars is not 6, stop processing
        if (numStars !== 6) {
            macroService.IsRunning = false;
            break;
        }

        const selectedPattern = macroService.ClonePattern(patterns.inventory.selected, { OffsetCalcType: 'None', CenterX: item.X, CenterY: item.Y, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
        macroService.PollPoint({ X: item.X + 60, Y: item.Y - 60 }, { PredicatePattern: selectedPattern });
        sleep(500);

        if (isLastItem) continue;

        let itemStats = getItemStats2();

        // Track the very first item's grade
        if (firstItemGrade === null) {
            firstItemGrade = itemStats.itemGrade;
        }

        // If item grade changes from the first item's grade, stop processing
        if (itemStats.itemGrade !== firstItemGrade) {
            macroService.IsRunning = false;
            break;
        }

        if (!['legendary', 'epic'].includes(itemStats.itemGrade)) continue;

        processedCount++;

        // Legendary processing (no locked check, requires 6 yellow stars, requires 6+ points, requires 3+ desired stats)
        if (itemStats.itemGrade === 'legendary') {
            if (numYellowStars !== 6) continue;

            // Require at least 3 desired stats (out of 4 secondary stats) for all Legendary items
            const numDesiredStats = itemStats.desiredStats.filter(stat =>
                [itemStats.secondary1, itemStats.secondary2, itemStats.secondary3, itemStats.secondary4].includes(stat)
            ).length;

            const shouldLock = itemStats.desiredPoints >= 6 && numDesiredStats >= 3;
            const lockedPattern = macroService.ClonePattern(patterns.inventory.item.locked, { OffsetCalcType: 'None', CenterX: item.X + 10, CenterY: item.Y - 80, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });

            if (shouldLock) {
                //macroService.PollPattern(patterns.inventory.item.stat.unlocked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: patterns.inventory.item.stat.locked });
                macroService.PollPattern(patterns.inventory.item.stat.unlocked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: lockedPattern });
                lockedCount++;
            } else {
                // Unlock if it doesn't meet requirements
                //macroService.PollPattern(patterns.inventory.item.stat.locked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: patterns.inventory.item.stat.unlocked });
                const lockedResult = macroService.FindPattern(lockedPattern);
                if (lockedResult.IsSuccess) {
                    macroService.PollPattern(patterns.inventory.item.stat.locked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, InversePredicatePattern: lockedPattern });
                }
                unlockedCount++;
            }
        }
        // Epic and other grades processing (check locked, requires 5+ points)
        else {
            const lockedPattern = macroService.ClonePattern(patterns.inventory.item.locked, { OffsetCalcType: 'None', CenterX: item.X + 10, CenterY: item.Y - 80, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
            const lockedResult = macroService.FindPattern(lockedPattern);
            if (lockedResult.IsSuccess) continue;

            if (numRedStars === 0 && itemStats.desiredPoints >= 5) {
                // unlock the forth stat
                macroService.PollPattern(patterns.inventory.enhance, { DoClick: true, PredicatePattern: patterns.titles.improveGear });
                macroService.PollPattern(patterns.inventory.improveGear.reforge, { DoClick: true, PredicatePattern: patterns.inventory.improveGear.reforge.reforge });
                macroService.PollPattern(patterns.inventory.improveGear.reforge.reforge, { DoClick: true, IntervalDelayMs: 4_000, PrimaryClickInversePredicatePattern: patterns.inventory.improveGear.reforge.redStar, PredicatePattern: patterns.inventory.improveGear.reforge.redStar });
                macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickPredicatePattern: patterns.titles.improveGear, PredicatePattern: patterns.titles.inventory });

                // start the outer while loop over
                lastSelectedResult = { Point: { X: 0, Y: 0 } };
                selectedResult = macroService.PollPattern(patterns.inventory.selected);
                isFirstIteration = true;
                continue outerLoop;
            }

            // Require at least 3 desired stats (out of 4 secondary stats)
            const numDesiredStats = itemStats.desiredStats.filter(stat =>
                [itemStats.secondary1, itemStats.secondary2, itemStats.secondary3, itemStats.secondary4].includes(stat)
            ).length;

            if (itemStats.desiredPoints >= 7 && numDesiredStats >= 3) {
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

// Display summary
const searchType = firstItemGrade === 'legendary' ? 'Legendary' : 'Epic';
const summary = `Inventory processing complete.\nSearch type: ${searchType}\nProcessed: ${processedCount}\nLocked: ${lockedCount}\nUnlocked: ${unlockedCount}`;
return summary;
