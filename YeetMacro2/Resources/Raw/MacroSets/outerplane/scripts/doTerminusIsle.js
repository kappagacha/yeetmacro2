// @raw-script
// @tags=favorites
function doTerminusIsle(type) {
	if (!type) type = settings.doTerminusIsle.type.Value;

	if (type === 'start') {
		return doTerminusIsleStart();
	} else if (type === 'normal') {
		return doTerminusIsleNormal();
	} else if (type === 'withSupportPack') {
		return doTerminusIsleWithSupportPack();
	} else {
		throw new Error(`Invalid terminus isle type: ${type}. Expected 'start', 'normal', or 'withSupportPack'.`);
	}
}

function doTerminusIsleStart() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.doTerminusIsle.start.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	refillStamina(30);
	goToLobby();

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doTerminusIsle: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doTerminusIsle: click terminus isle');
				macroService.ClickPattern(patterns.adventure.terminusIsle);
				sleep(500);
				break;
			case 'terminusIsle.stage':
				logger.info('doTerminusIsle: start exploration');
				const formExplorationTeamResult = macroService.PollPattern(patterns.terminusIsle.formExplorationTeam, { DoClick: true, PredicatePattern: [patterns.terminusIsle.formExplorationTeam.autoFormation, patterns.terminusIsle.zeroExplorationChances] });
				if (formExplorationTeamResult.PredicatePath === 'terminusIsle.zeroExplorationChances') {
					return;
				}
				macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.autoFormation, { DoClick: true, PredicatePattern: patterns.terminusIsle.formExplorationTeam.startExploration });
				macroService.PollPattern(patterns.terminusIsle.formExplorationTeam.startExploration, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });

				if (macroService.IsRunning) {
					daily.doTerminusIsle.start.IsChecked = true;
					settings.doTerminusIsle.start.lastRun.Value = new Date().toISOString();
				}
				return;
		}
		sleep(1_000);
	}
}

function doTerminusIsleNormal() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.doTerminusIsle.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	// Exploration takes 4 hours
	const isTerminusIsleReady = (Date.now() - settings.doTerminusIsle.start.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 > 4;
	if (!isTerminusIsleReady && !settings.doTerminusIsle.forceRun.Value) {
		return 'startTerminusIsleExploration was ran less than 4 hours ago. Use forceRun setting to override check';
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doTerminusIsle: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doTerminusIsle: click terminus isle');
				macroService.ClickPattern(patterns.adventure.terminusIsle);
				sleep(500);
				break;
			case 'terminusIsle.stage':
				logger.info('doTerminusIsle: do explorations');
				const terminusIsleResult = macroService.PollPattern([patterns.terminusIsle.confirm, patterns.terminusIsle.inProgress], { TimeoutMs: 3_000 });
				if (terminusIsleResult.Path === 'terminusIsle.inProgress') {
					return;
				}

				doExplorations(false);

				sleep(1_000);
				doEnhancedDeadlyCreature(false);

				sleep(1_000);
				doMoonlitFangBoss();

				sleep(1_000);
				claimFinalStageReward();

				if (macroService.IsRunning) {
					daily.doTerminusIsle.done.IsChecked = true;
				}
				return;
		}
		sleep(1_000);
	}
}

function doTerminusIsleWithSupportPack() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage];
	const daily = dailyManager.GetCurrentDaily();

	if (daily.doTerminusIsle.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	const orderNameRegex = {
		baseArtilleryFire: /Base/i,
		changeWeather: /We.*C[0o]/i,
		enhancedDeadlyCreatureAppearanceRate: /Enh/i,
		completeAllExplorations: /C[0o]m/i,
		increaseExplorationRewards: /Inc/i
	};

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doTerminusIsle: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doTerminusIsle: click terminus isle');
				macroService.ClickPattern(patterns.adventure.terminusIsle);
				sleep(500);
				break;
			case 'terminusIsle.stage':
				logger.info('doTerminusIsle: executeBonusOrders');
				const maxExplorationChances2Result = macroService.PollPattern(patterns.terminusIsle.maxExplorationChances2, { TimeoutMs: 3_500 });
				if (!maxExplorationChances2Result.IsSuccess) {
					throw new Error('Did not detect max exploration chance of 2. Make sure you have Exploration support pack');
				}

				executeBonusOrders(orderNameRegex);

				let zeroExplorationChanceResult = macroService.FindPattern(patterns.terminusIsle.zeroExplorationChances);
				while (!zeroExplorationChanceResult.IsSuccess) {
					let weatherCondition = getCurrentWeatherCondition();
					logger.info(`doTerminusIsle: detected weather is ${weatherCondition}`);
					if (weatherCondition != 'fire') {
						executeOrderChangeWeather(orderNameRegex);
					}

					logger.info('doTerminusIsle: startExploration');
					startExploration();
					sleep(1_000);
					logger.info('doTerminusIsle: executeOrderCompleteAllExplorations');
					executeOrderCompleteAllExplorations(orderNameRegex);
					sleep(1_000);
					logger.info('doTerminusIsle: doExplorations');
					doExplorations(true);
					sleep(1_000);
					logger.info('doTerminusIsle: doEnhancedDeadlyCreature');
					doEnhancedDeadlyCreature(true);
					sleep(1_000);
					logger.info('doTerminusIsle: doMoonlitFangBoss');
					doMoonlitFangBoss();
					sleep(1_000);
					logger.info('doTerminusIsle: claimFinalStageReward');
					claimFinalStageReward();
					sleep(1_000);

					zeroExplorationChanceResult = macroService.FindPattern(patterns.terminusIsle.zeroExplorationChances);
				}

				if (macroService.IsRunning) {
					daily.doTerminusIsle.done.IsChecked = true;
				}
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

	function executeBonusOrders(orderNameRegex) {
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

	function executeOrderChangeWeather(orderNameRegex) {
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

	function executeOrderCompleteAllExplorations(orderNameRegex) {
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
}

function getOrderNames() {
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

function executeOrderBaseArtilleryFire() {
	const orderNameRegex = {
		baseArtilleryFire: /Base/i
	};

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

function doExplorations(skipHeroDeployment) {
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
				const randomOption = macroService.Random(1, 3);
				const optionResult = macroService.PollPattern(patterns.terminusIsle.prompt.options[randomOption], { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: [patterns.general.tapEmptySpace, patterns.terminusIsle.prompt.heroDeployment] });

				if (optionResult.PredicatePath === 'terminusIsle.prompt.heroDeployment') {
					deployHeroes(skipHeroDeployment);
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
				deployHeroes(skipHeroDeployment);
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });
				break;
		}
	}
}

function deployHeroes(skipHeroDeployment) {
	if (!skipHeroDeployment) {
		const skipResult = macroService.FindPattern(patterns.terminusIsle.prompt.heroDeployment.skip);
		if (skipResult.IsSuccess) {
			macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment.skip, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });
			return;
		}

		macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
		const recommendedElementPatterns = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.terminusIsle.prompt.heroDeployment.recommendedElement[el]);
		const recommendedElementResult = macroService.PollPattern(recommendedElementPatterns);
		const recommendedElement = recommendedElementResult.Path?.split('.').pop();
		selectTeam(recommendedElement, { applyPreset: true });
		macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
		macroService.PollPattern(patterns.battle.exit, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });
	} else {
		macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment.skip, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });
	}
}

function doEnhancedDeadlyCreature(isWithSupportPack) {
	let warningResult = macroService.PollPattern(patterns.terminusIsle.warning, { TimeoutMs: 3_000 });
	if (warningResult.IsSuccess) {
		if (isWithSupportPack) {
			executeOrderBaseArtilleryFire();
		}
		macroService.PollPattern(patterns.terminusIsle.warning, { DoClick: true, PredicatePattern: patterns.terminusIsle.prompt.heroDeployment, });
		macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
		selectTeamGeneral();
		macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
		macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
		const exitResult = macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.general.tapEmptySpace, patterns.terminusIsle.prompt.heroDeployment] });
		if (exitResult.PredicatePath === 'terminusIsle.prompt.heroDeployment') {
			macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamGeneral();
			macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickInversePredicatePattern: patterns.battle.enter, PredicatePattern: patterns.battle.enter });
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
