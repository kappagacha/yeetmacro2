const topLeft = macroService.GetTopLeft();
const coinResult = macroService.FindPattern(patterns.shop.coin, { Limit: 5 });
let cointTypeResults = coinResult.Points.map(p => {
	const coinTypeBounds = {
		X: 80 + topLeft.X,
		Y: p.Y - 19,
		Height: 40.5,
		Width: p.X - 35 - 80 - topLeft.X
	};

	//while (macroService.IsRunning) {
	//	macroService.DebugRectangle(coinTypeBounds);
	//	sleep(1_000);
	//}

	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.FindTextWithBounds(coinTypeBounds)
	};
});

return cointTypeResults;