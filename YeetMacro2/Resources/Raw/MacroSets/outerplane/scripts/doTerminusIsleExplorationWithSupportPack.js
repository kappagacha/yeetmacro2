// Do terminus isle exploration with support pack
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage]
const daily = dailyManager.GetCurrentDaily();

if (daily.doTerminusIsleExplorationWithSupportPack.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

//logger.isPersistingLogs = true;

const orderNameRegex = {
	baseArtilleryFire: /Base/i,
	changeWeather: /We.*C[0o]/i,
	enhancedDeadlyCreatureAppearanceRate: /Enh/i,
	completeAllExplorations: /C[0o]m/i,
	increaseExplorationRewards: /Inc/i
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doTerminusIsleExplorationWithSupportPack: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doTerminusIsleExplorationWithSupportPack: click terminus isle');
			macroService.ClickPattern(patterns.adventure.terminusIsle);
			sleep(500);
			break;
		case 'terminusIsle.stage':
			logger.info('doTerminusIsleExplorationWithSupportPack: executeBonusOrders');
			const maxExplorationChances2Result = macroService.PollPattern(patterns.terminusIsle.maxExplorationChances2, { TimeoutMs: 3_500 });
			if (!maxExplorationChances2Result.IsSuccess) {
				throw new Error('Did not detect max exploration chance of 2. Make sure you have Exploration support pack')
			}

			executeBonusOrders();

			let zeroExplorationChanceResult = macroService.FindPattern(patterns.terminusIsle.zeroExplorationChances);
			while (!zeroExplorationChanceResult.IsSuccess) {
				let weatherCondition = getCurrentWeatherCondition();
				logger.info(`doTerminusIsleExplorationWithSupportPack: detected weather is ${weatherCondition}`);
				if (weatherCondition != 'fire') {
					executeOrderChangeWeather();
				}
				//const targetWeatherConditions = ['earth', 'fire'];
				//while (!targetWeatherConditions.includes(weatherCondition)) {
				//	executeOrderChangeWeather();
				//	sleep(3_000);
				//	weatherCondition = getCurrentWeatherCondition();
				//	logger.info(`doTerminusIsleExplorationWithSupportPack: detected weather is ${weatherCondition}`);
				//}

				logger.info('doTerminusIsleExplorationWithSupportPack: startExploration');
				startExploration();
				sleep(1_000);
				logger.info('doTerminusIsleExplorationWithSupportPack: executeOrderCompleteAllExplorations');
				executeOrderCompleteAllExplorations();
				sleep(1_000);
				logger.info('doTerminusIsleExplorationWithSupportPack: doExplorations');
				doExplorations();
				sleep(1_000);
				logger.info('doTerminusIsleExplorationWithSupportPack: doEnhancedDeadlyCreature');
				doEnhancedDeadlyCreature();
				sleep(1_000);
				logger.info('doTerminusIsleExplorationWithSupportPack: doMoonlitFangBoss');
				doMoonlitFangBoss();
				sleep(1_000);
				logger.info('doTerminusIsleExplorationWithSupportPack: claimFinalStageReward');
				claimFinalStageReward();
				sleep(1_000);

				zeroExplorationChanceResult = macroService.FindPattern(patterns.terminusIsle.zeroExplorationChances);
			}
			
			if (macroService.IsRunning) {
				daily.doTerminusIsleExplorationWithSupportPack.done.IsChecked = true;
			}
			//logger.isPersistingLogs = false;
			return;
	}
	sleep(1_000);
}

function getCurrentWeatherCondition() {
	const weatherConditionPatterns = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.terminusIsle.weatherCondition[el]);
	const weatherConditionResult = macroService.PollPattern(weatherConditionPatterns);
	const weatherCondition = weatherConditionResult.Path?.split('.').pop();
	return weatherCondition;
}

// if available, activate enhancedDeadlyCreatureAppearanceRate or increaseExplorationRewards
function executeBonusOrders() {
	macroService.PollPattern(patterns.terminusIsle.explorationOrder, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate });
	sleep(1_000);
	macroService.SwipePattern(patterns.terminusIsle.explorationOrder.swipeDown);
	sleep(1_500);
	const orderNames = getOrderNames();
	let isAtLeastOneOrderSelected = false;
	for (let { point: p, name } of orderNames) {
		if (name.match(orderNameRegex.enhancedDeadlyCreatureAppearanceRate) || name.match(orderNameRegex.increaseExplorationRewards)) {
			const selectedPattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.selected, { CenterY: p.Y + 30, Path: `patterns.terminusIsle.explorationOrder.selected_y${p.Y}` });
			macroService.PollPoint(p, { DoClick: true, PredicatePattern: selectedPattern });
			isAtLeastOneOrderSelected = true;
		}
	}

	if (isAtLeastOneOrderSelected) {
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.ok });
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.ok, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	} else {
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.close, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	}
}

function executeOrderChangeWeather() {
	macroService.PollPattern(patterns.terminusIsle.explorationOrder, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate });

	const orderNames = getOrderNames();
	for (let { point: p, name } of orderNames) {
		if (name.match(orderNameRegex.changeWeather)) {
			const selectedPattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.selected, { CenterY: p.Y + 30, Path: `patterns.terminusIsle.explorationOrder.selected_y${p.Y}` });
			macroService.PollPoint(p, { DoClick: true, PredicatePattern: selectedPattern });
			break;
		}
	}

	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.apply });
	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.heatWave, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.heatWave.selected });
	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.apply, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.ok });
	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.ok, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
}

function startExploration() {
	macroService.PollPattern(patterns.terminusIsle.formExplorationTeam, { DoClick: true, PredicatePattern: patterns.terminusIsle.formExplorationTeam.autoFormation });
	macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.autoFormation, { DoClick: true, PredicatePattern: patterns.terminusIsle.formExplorationTeam.startExploration });
	macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.startExploration, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
}

function executeOrderCompleteAllExplorations() {
	macroService.PollPattern(patterns.terminusIsle.explorationOrder, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate });
	sleep(1_000);
	macroService.SwipePattern(patterns.terminusIsle.explorationOrder.swipeDown);
	sleep(1_500);

	const orderNames = getOrderNames();
	for (let { point: p, name } of orderNames) {
		if (name.match(orderNameRegex.completeAllExplorations)) {
			const selectedPattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.selected, { CenterY: p.Y + 30, Path: `patterns.terminusIsle.explorationOrder.selected_y${p.Y}` });
			macroService.PollPoint(p, { DoClick: true, PredicatePattern: selectedPattern });
			break;
		}
	}

	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.ok });
	macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.ok, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
}

function executeOrderBaseArtilleryFire() {
	macroService.PollPattern(patterns.terminusIsle.explorationOrder, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate });

	const orderNames = getOrderNames();
	let foundOrder = false;
	for (let { point: p, name } of orderNames) {
		if (name.match(orderNameRegex.baseArtilleryFire)) {
			const selectedPattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.selected, { CenterY: p.Y + 30, Path: `patterns.terminusIsle.explorationOrder.selected_y${p.Y}` });
			macroService.PollPoint(p, { DoClick: true, PredicatePattern: selectedPattern });
			foundOrder = true;
			break;
		}
	}

	if (foundOrder) {
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate, { DoClick: true, PredicatePattern: patterns.terminusIsle.explorationOrder.activate.ok });
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.activate.ok, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	} else {
		macroService.PollPattern(patterns.terminusIsle.explorationOrder.close, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	}
}

function getOrderNames() {
	// TODO: throw error if FREE is not found
	const cornerResult = macroService.FindPattern(patterns.terminusIsle.explorationOrder.corner, { Limit: 5 });
	const orderNames = cornerResult.Points.filter(p => p).map(p => {
		const orderNamePattern = macroService.ClonePattern(patterns.terminusIsle.explorationOrder.orderName, { CenterY: p.Y + 25, Path: `patterns.terminusIsle.explorationOrder.orderName_x${p.X}_y${p.Y}` });

		return {
			point: { X: p.X, Y: p.Y + 25 },
			name: macroService.FindText(orderNamePattern)
		};
	});
	return orderNames;
}

function doExplorations() {
	for (let i = 0; i < 5; i++) {
		const confirmResult = macroService.PollPattern(patterns.terminusIsle.confirm, {
			DoClick: true, ClickOffset: { X: -25, Y: -25 }, PredicatePattern: [
				patterns.terminusIsle.prompt.next,
				patterns.terminusIsle.prompt.treasureChestFound,
				patterns.terminusIsle.prompt.explorationFailed,
				patterns.terminusIsle.prompt.heroDeployment
			]
		});

		switch (confirmResult.PredicatePath) {
			case 'terminusIsle.prompt.next':
				const title = macroService.FindText(patterns.terminusIsle.prompt.title);
				sleep(3_000);
				logger.screenCapture(`Title: ${title}`);
				// TODO: pick 1 out of 3 options based on title
				const randomOption = macroService.Random(1, 3);
				const optionResult = macroService.PollPattern(patterns.terminusIsle.prompt.options[randomOption], { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: [patterns.general.tapEmptySpace, patterns.terminusIsle.prompt.heroDeployment] });

				if (optionResult.PredicatePath === 'terminusIsle.prompt.heroDeployment') {
					deployHeroes();
					sleep(1000);
					logger.screenCapture(`Title: ${title} => option ${randomOption}`);
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				} else {
					sleep(1000);
					logger.screenCapture(`Title: ${title} => option ${randomOption}`);
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				}
				break;
			case 'terminusIsle.prompt.treasureChestFound':
				const randomChest = macroService.Random(1, 3);
				macroService.PollPattern(patterns.terminusIsle.prompt.treasureChestFound[randomChest], { DoClick: true, PredicatePattern: patterns.terminusIsle.prompt.treasureChestFound.open });
				macroService.PollPattern(patterns.terminusIsle.prompt.treasureChestFound.open, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				break;
			case 'terminusIsle.prompt.explorationFailed':
				macroService.PollPattern(patterns.terminusIsle.prompt.explorationFailed.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				break;
			case 'terminusIsle.prompt.heroDeployment':
				deployHeroes();
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				break;
		}
	}
}

function deployHeroes() {
	macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment.skip, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });

	//macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
	//const recommendedElementPatterns = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.terminusIsle.prompt.heroDeployment.recommendedElement[el]);
	//const recommendedElementResult = macroService.PollPattern(recommendedElementPatterns);
	//const recommendedElement = recommendedElementResult.Path?.split('.').pop();
	//selectTeam(recommendedElement, { applyPreset: true });
	//macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
	//macroService.PollPattern(patterns.battle.exit, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });
}

function doEnhancedDeadlyCreature() {
	let warningResult = macroService.PollPattern(patterns.terminusIsle.warning, { TimeoutMs: 3_000 });
	if (warningResult.IsSuccess) {
		executeOrderBaseArtilleryFire();
		macroService.PollPattern(patterns.terminusIsle.warning, { DoClick: true, PredicatePattern: patterns.terminusIsle.prompt.heroDeployment, });
		macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
		selectTeam('RecommendedElement', { applyPreset: true });
		macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
		const exitResult = macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.general.tapEmptySpace, patterns.terminusIsle.prompt.heroDeployment] });
		// if another attempt is needed
		if (exitResult.PredicatePath === 'terminusIsle.prompt.heroDeployment') {
			macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeam('RecommendedElement', { applyPreset: true });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		}
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	}
}

function doMoonlitFangBoss() {
	let moonlitFangBossResult = macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { TimeoutMs: 3_000 });
	while (moonlitFangBossResult.IsSuccess) {
		macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { DoClick: true, PredicatePattern: patterns.terminusIsle.prompt.heroDeployment, });
		macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
		const bossType = detectBossType();
		selectTeam(bossType === 'light' ? 'dark' : 'light', { applyPreset: true });
		macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
		macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });

		moonlitFangBossResult = macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { TimeoutMs: 3_000 });
	}
}

function claimFinalStageReward() {
	let finalStageRewardResult = macroService.PollPattern(patterns.terminusIsle.finalStageReward, { TimeoutMs: 3_000 });
	if (finalStageRewardResult.IsSuccess) {
		macroService.PollPattern(patterns.terminusIsle.finalStageReward, { DoClick: true, PredicatePattern: patterns.terminusIsle.finalStageReward.ok, });
		macroService.PollPattern(patterns.terminusIsle.finalStageReward.ok, { DoClick: true, PredicatePattern: patterns.terminusIsle.finalStageReward.retry });
		macroService.PollPattern(patterns.terminusIsle.finalStageReward.retry, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
	}
}
