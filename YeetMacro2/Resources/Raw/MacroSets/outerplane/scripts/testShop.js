// @position=9999
// @tags=test

const resolution = macroService.GetCurrentResolution();
const itemCornerPattern = macroService.ClonePattern(patterns.shop.itemCorner, {
	X: 250,
	Y: 110,
	Width: 1470,	//resolution.Width - 550,
	Height: 920
});

let itemCornerResult = macroService.FindPattern(itemCornerPattern, { Limit: 12 });
let textResults = itemCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
	const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X, Y: p.Y, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.FindText(itemTextPattern)
	};
});

return textResults;