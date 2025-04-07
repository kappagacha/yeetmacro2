const itemCornerResult = macroService.FindPattern(patterns.shop.itemCorner, { Limit: 8 });

let textResults = itemCornerResult.Points.map(p => {
	var rect = { X: p.X + 5, Y: p.Y - 340, Width: 225, Height: 200 };
	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.FindTextWithBounds(rect)
	};
});

return textResults;