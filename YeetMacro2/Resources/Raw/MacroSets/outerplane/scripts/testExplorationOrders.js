// @position=9999
// Test exploration orders

const cornerResult = macroService.FindPattern(patterns.terminusIsle.explorationOrder.corner, { Limit: 5 });
const orderNames = cornerResult.Points.filter(p => p).map(p => {
	const orderNamePattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.orderName, { CenterY: p.Y + 25, Path: `patterns.terminusIsle.explorationOrder.orderName_x${p.X}_y${p.Y}` });

	return {
		point: { X: p.X, Y: p.Y + 25 },
		name: macroService.GetText(orderNamePattern)
	};
});
return orderNames;