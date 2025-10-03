const selectedResult = macroService.PollPattern(patterns.inventory.selected);
const itemResult = macroService.FindPattern(patterns.inventory.item, { Limit: 100 });
const minY = selectedResult.Point.Y;
const selectedRowMinX = selectedResult.Point.X;
const padding = 10;

const isAfterSelected = (p) => {
    const isOnSelectedRow = Math.abs(p.Y - minY) <= padding;

    if (isOnSelectedRow) {
        return p.X > selectedRowMinX + padding;
    } else {
        return p.Y > minY + padding;
    }
};

// All items matching patterns.inventory.item are unselected items
// Just filter for position after cursor
const filteredItems = itemResult.Points
    .filter(p => isAfterSelected(p))
    .sort((a, b) => {
        // First sort by Y (row), then by X (column)
        if (Math.abs(a.Y - b.Y) > padding) {
            return a.Y - b.Y; // Different rows
        }
        return a.X - b.X; // Same row, sort by column
    });

// Loop through each filtered item
for (const item of filteredItems) {
    if (!macroService.IsRunning) break;

    const selectedPattern = macroService.ClonePattern(patterns.inventory.selected, { OffsetCalcType: 'None', CenterX: item.X, CenterY: item.Y, Width: 50, Height: 50, PathSuffix: `_${item.X}x${item.Y}y` });
    macroService.PollPoint({ X: item.X + 60, Y: item.Y - 60 }, { PredicatePattern: selectedPattern });
    sleep(1_000);
}
