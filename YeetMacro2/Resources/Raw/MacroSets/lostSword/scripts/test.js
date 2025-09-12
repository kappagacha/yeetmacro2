const coinResult = macroService.FindPattern(patterns.shop.coin, { Limit: 5 });
let cointTypeResults = coinResult.Points.map(p => {
	const cointTypePattern = macroService.ClonePattern(patterns.shop.coin.type, { CenterY: p.Y, PathSuffix: `_${p.X}x_${p.Y}y` });
	return {
		point: { X: p.X, Y: p.Y },
		text: macroService.FindText(cointTypePattern)
	};
});
return cointTypeResults;