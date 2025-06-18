const unitTitleAndName = macroService.FindText(patterns.battle.teamFormation.unitTitleAndName);
return unitTitleAndName;

//sleep(2000);
//const operations = ['irregularQueen', 'blockbuster', 'mutatedWyvre', 'ironStretcher'];
//let operationToPoints = {
//	irregularQueen: {
//		target: 6000,
//		current: 0
//	},
//	blockbuster: {
//		target: 6000,
//		current: 0
//	},
//	mutatedWyvre: {
//		target: 6000,
//		current: 0
//	},
//	ironStretcher: {
//		target: 6000,
//		current: 0
//	}
//};

//operations.forEach(op => operationToPoints[op].current = Number(macroService.FindText(patterns.irregularExtermination.pursuitOperation[op].currentPoints).replace(/[, ]/g, '')));
//targetOperation = Object.entries(operationToPoints).reduce((targetOperation, [op, { target, current }]) => {
//	if (targetOperation || current > target) return targetOperation;
//	return op;
//}, null);
//operationToPoints.targetOperation = targetOperation;
//return operationToPoints;
