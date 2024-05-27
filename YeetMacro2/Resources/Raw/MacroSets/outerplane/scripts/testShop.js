// @position=9999

const resolution = macroService.GetCurrentResolution();
const itemCornerPattern = macroService.ClonePattern(patterns.shop.itemCorner, {
	X: 350,
	Y: 130,
	Width: 1370,	//resolution.Width - 550,
	Height: 900
});

let itemCornerResult = macroService.FindPattern(itemCornerPattern, { Limit: 12 });
let textResults = itemCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
	const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X, Y: p.Y, OffsetCalcType: 'None' });
	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.GetText(itemTextPattern)
	};
});

return textResults;