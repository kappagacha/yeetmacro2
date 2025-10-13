const currentResult = macroService.PollPattern(patterns.labyrinth.current);
const currentPoint = currentResult.Point;

const bigNodeCloneOptions = { X: currentPoint.X + 235, Y: 250, Width: 200, Height: 600, PathSuffix: `_${currentPoint.X}x_${currentPoint.Y}y`, OffsetCalcType: 'None' };
const topCloneOptions = { X: currentPoint.X + 235, Y: currentPoint.Y - 135, Width: 130, Height: 130, PathSuffix: `_top_${currentPoint.X}x_${currentPoint.Y}y`, OffsetCalcType: 'None' };
const midCloneOptions = { X: currentPoint.X + 235, Y: currentPoint.Y + 70, Width: 130, Height: 130, PathSuffix: `_mid_${currentPoint.X}x_${currentPoint.Y}y`, OffsetCalcType: 'None' };
const botCloneOptions = { X: currentPoint.X + 235, Y: currentPoint.Y + 275, Width: 130, Height: 130, PathSuffix: `_bot_${currentPoint.X}x_${currentPoint.Y}y`, OffsetCalcType: 'None' };

const bigNodeLabelPattern = macroService.ClonePattern(patterns.labyrinth.bigNodeLabel, bigNodeCloneOptions);
const bigNodeLabelResult = macroService.FindPattern(bigNodeLabelPattern);

// TODO: handle last node

let nodeResult;
if (bigNodeLabelResult.IsSuccess) {
	logger.info('doLabyrinth: handle big node');
	const bigLabelPoint = { X: bigNodeLabelResult.Point.X, Y: bigNodeLabelResult.Point.Y - 150 };
	nodeResult = macroService.PollPoint(bigLabelPoint, { DoClick: true, PredicatePattern: [patterns.general.warning, patterns.labyrinth.sweep, patterns.labyrinth.select, patterns.character.chat.next] });
	if (nodeResult.PredicatePath === 'general.warning') {
		macroService.PollPattern(patterns.general.dontAskAgainCheckbox, { DoClick: true, PredicatePattern: patterns.general.dontAskAgainCheckbox.checked });
	}
	nodeResult = macroService.PollPattern(patterns.general.confirm, { DoClick: true, PredicatePattern: [patterns.labyrinth.sweep, patterns.labyrinth.select, patterns.character.chat.next] });
} else {
	const positions = ['top', 'mid', 'bot'];
	const positionToCloneOptions = { top: topCloneOptions, mid: midCloneOptions, bot: botCloneOptions };
	const smallNodeTypes = ['demon', 'heal', 'monster', 'treasureChest', 'question'];
	//const smallNodeTypes = ['treasureChest', 'monster', 'question', 'demon', 'heal'];
	const positionToNodeType = {};
	for (let position of positions) {
		const cloneOptions = positionToCloneOptions[position];
		const nodeTypePatterns = smallNodeTypes.map(nt => macroService.ClonePattern(patterns.labyrinth.smallNode[nt], cloneOptions));
		const nodeTypeResult = macroService.PollPattern(nodeTypePatterns);
		const nodeType = nodeTypeResult.Path.split('_')[0].split('.').pop();
		positionToNodeType[position] = { position, nodeType, nodeTypeIndex: smallNodeTypes.findIndex(nt => nt === nodeType) };
	}

	// TODO: treasure chest => DONE
	// TODO: monster => DONE
	// TODO: heal =>
	// TODO: demon =>
	// TODO: question => DONE

	const bestPostion = Object.values(positionToNodeType).reduce((bestP, currentP) => currentP.nodeTypeIndex < bestP.nodeTypeIndex ? currentP : bestP);

	logger.info(`doLabyrinth: handle node ${bestPostion.nodeType}`);
	const cloneOptions = positionToCloneOptions[bestPostion.position];
	const nodetypePattern = macroService.ClonePattern(patterns.labyrinth.smallNode[bestPostion.nodeType], cloneOptions);
	const nodeTypeResult = macroService.PollPattern(nodetypePattern, { DoClick: true, PredicatePattern: [patterns.general.warning, patterns.labyrinth.sweep, patterns.labyrinth.select, patterns.character.chat.next] });
	if (nodeTypeResult.PredicatePath === 'general.warning') {
		macroService.PollPattern(patterns.general.dontAskAgainCheckbox, { DoClick: true, PredicatePattern: patterns.general.dontAskAgainCheckbox.checked });
	}
	nodeResult = macroService.PollPattern(patterns.general.confirm, { DoClick: true, PredicatePattern: [patterns.labyrinth.sweep, patterns.labyrinth.select, patterns.character.chat.next, patterns.labyrinth.title] });
}

handleNode(nodeResult);

function handleNode(result) {
	if (result.PredicatePath === 'labyrinth.sweep') {
		logger.info('doLabyrinth: handle big node battle');
		doSweep();
	} else if (result.PredicatePath === 'labyrinth.select') {
		logger.info('doLabyrinth: handle big node select card');
		selectCard();
	} else if (result.PredicatePath === 'character.chat.next') {
		logger.info('doLabyrinth: handle options');
		selectOption();
	}
}

function doSweep() {
	const sweepResult = macroService.PollPattern(patterns.labyrinth.sweep, { DoClick: true, PredicatePattern: [patterns.general.warning, patterns.general.itemsAcquired] });
	if (sweepResult.PredicatePath === 'general.warning') {
		macroService.PollPattern(patterns.general.dontAskAgainCheckbox, { DoClick: true, PredicatePattern: patterns.general.dontAskAgainCheckbox.checked });
		macroService.PollPattern(patterns.general.confirm, { DoClick: true, PredicatePattern: patterns.general.itemsAcquired });
	}
	macroService.PollPattern(patterns.general.itemsAcquired, { DoClick: true, PredicatePattern: patterns.labyrinth.title });
}

function selectCard() {
	const goldResult = macroService.FindPattern(patterns.labyrinth.select.gold);
	if (goldResult.IsSuccess) {
		macroService.DoClick(goldResult.Point);
		sleep(350);
		macroService.DoClick(goldResult.Point);
	} else {
		const cardNumber = 1 + Math.floor(Math.random() * 3);
		macroService.ClickPattern(patterns.labyrinth.select[`card${cardNumber}`]);
		sleep(350);
		macroService.ClickPattern(patterns.labyrinth.select[`card${cardNumber}`]);
	}
	macroService.PollPattern(patterns.labyrinth.select, { DoClick: true, ClickPattern: patterns.general.itemsAcquired, PredicatePattern: [patterns.general.confirm, patterns.labyrinth.title] });
	macroService.PollPattern(patterns.general.confirm, { DoClick: true, PredicatePattern: patterns.labyrinth.title });
}

function selectOption() {
	let optionsResult = macroService.PollPattern(patterns.character.chat.next, { DoClick: true, ClickOffset: { Y: -60 }, PredicatePattern: patterns.character.chat.options });

	while (macroService.IsRunning && optionsResult.IsSuccess) {
		optionsResult = macroService.FindPattern(patterns.character.chat.options, { Limit: 3 });
		const optionNumber = Math.floor(Math.random() * optionsResult.Points.length);
		const optionPoint = optionsResult.Points[optionNumber];
		macroService.DoClick(optionPoint);
		sleep(500);
		optionsResult = macroService.PollPattern(patterns.character.chat.options, { TimeoutMs: 2_500 });
	}

	macroService.PollPattern(patterns.character.chat.next, { DoClick: true, ClickOffset: { Y: -60 }, PredicatePattern: [patterns.general.confirm, patterns.general.itemsAcquired, patterns.labyrinth.title] });
	macroService.PollPattern(patterns.general.confirm, { DoClick: true, ClickPattern: patterns.general.itemsAcquired, PredicatePattern: patterns.labyrinth.title });
}
