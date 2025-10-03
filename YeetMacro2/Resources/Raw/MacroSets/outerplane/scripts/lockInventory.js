const padding = 5;
let lastSelectedResult = { Point: { X: 0, Y: 0 } };
let selectedResult = macroService.PollPattern(patterns.inventory.selected);
let isFirstIteration = true;

while ((Math.abs(lastSelectedResult.Point.X - selectedResult.Point.X) > 15 ||
        Math.abs(lastSelectedResult.Point.Y - selectedResult.Point.Y) > 15)) {

    const itemResult = macroService.FindPattern(patterns.inventory.item, { Limit: 100 });
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
    const itemsAfterSelected = itemResult.Points
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
        if (![0, 6].includes(numStars)) continue;

        const lockedPattern = macroService.ClonePattern(patterns.inventory.item.locked, { OffsetCalcType: 'None', CenterX: item.X + 10, CenterY: item.Y - 80, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
        const lockedResult = macroService.FindPattern(lockedPattern);

        if (lockedResult.IsSuccess && !isLastItem) continue;

        const selectedPattern = macroService.ClonePattern(patterns.inventory.selected, { OffsetCalcType: 'None', CenterX: item.X, CenterY: item.Y, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
        macroService.PollPoint({ X: item.X + 60, Y: item.Y - 60 }, { PredicatePattern: selectedPattern });
        sleep(500);

        if (lockedResult.IsSuccess) continue;

        const itemStats = getItemStats();
        if (!['Legendary', 'Epic'].includes(itemStats.itemGrade)) continue;
        if (itemStats.desiredPoints >= 5) {
            // lock the item
            macroService.PollPattern(patterns.inventory.item.stat.unlocked, { DoClick: true, ClickPattern: patterns.inventory.item.stat.lockToggledMessage, PredicatePattern: patterns.inventory.item.stat.locked });
        }

        sleep(1_000);
    }

    isFirstIteration = false;
    lastSelectedResult = selectedResult;
    macroService.SwipePattern(patterns.inventory.swipeDown);
    sleep(2_000);
    selectedResult = macroService.PollPattern(patterns.inventory.selected);
}
