//const resolution = macroService.GetCurrentResolution();
//const lowerRightCornerResult = macroService.FindPattern(patterns.shop.lowerRightCorner, { Limit: 10 });
//let textResults = lowerRightCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
//	const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X - 3, Y: p.Y - 352, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
//	return {
//		point: { X: p.X, Y: p.Y },
//		text: macroService.FindText(itemTextPattern)
//	};
//});

//return textResults;

const shopItemName = 'Roulette Ticket';
const resolution = macroService.GetCurrentResolution();

const regexString = shopItemName.replace(/ /g, "\\s*");
const itemRegex = new RegExp(regexString, 'is');

let findResult;
let tryCount = 0;

while (!findResult) {
	const lowerRightCornerResult = macroService.FindPattern(patterns.shop.lowerRightCorner, { Limit: 10 });
	let textResults = lowerRightCornerResult.Points.filter(p => p.X < resolution.Width - 350).map(p => {
		const itemTextPattern = macroService.ClonePattern(patterns.shop.itemText, { X: p.X - 3, Y: p.Y - 352, OffsetCalcType: 'None', PathSuffix: `_${p.X}x_${p.Y}y` });
		return {
			point: { X: p.X, Y: p.Y },
			text: macroService.FindText(itemTextPattern)
		};
	});

	findResult = textResults.find(tr => tr.text.match(itemRegex));

	return { shopItemName, itemRegex, findResult, textResults };

	tryCount++;
	if (!findResult && tryCount % 2 === 0) {	// scan twice before swiping
		macroService.SwipePattern(patterns.general.swipeRight);
		sleep(2000);
	}
}

if (!findResult) throw Error(`Could not find ${shopItemName}`);

return findResult;