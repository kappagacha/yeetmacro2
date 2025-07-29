const resolution = macroService.GetCurrentResolution();
const lowerRightCornerResult = macroService.FindPattern(patterns.shop.lowerRightCorner, { Limit: 10 });
let textResults = lowerRightCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
	const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X - 3, Y: p.Y - 352, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.FindText(itemTextPattern)
	};
});

return textResults;