// Do terminus isle exploration
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.terminusIsle.stage]
const daily = dailyManager.GetCurrentDaily();

if (daily.doTerminusIsleExploration.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

// Exploration takes 4 hours
const isTerminusIsleReady = (Date.now() - settings.startTerminusIsleExploration.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 > 4;
if (!isTerminusIsleReady && !settings.doTerminusIsleExploration.forceRun.Value) {
	return 'startTerminusIsleExploration was ran less than 4 hours ago. Use forceRun setting to override check';
}

logger.isPersistingLogs = true;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doTerminusIsleExploration: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doTerminusIsleExploration: click terminus isle');
			macroService.ClickPattern(patterns.adventure.terminusIsle);
			sleep(500);
			break;
		case 'terminusIsle.stage':
			logger.info('doTerminusIsleExploration: do explorations');
			const terminusIsleResult = macroService.PollPattern([patterns.terminusIsle.confirm, patterns.terminusIsle.inProgress], { TimeoutMs: 3_000 });
			if (terminusIsleResult.Path === 'terminusIsle.inProgress') {
				logger.isPersistingLogs = false;
				return;
			}

			let confirmResult = macroService.PollPattern(patterns.terminusIsle.confirm, { TimeoutMs: 3_000 });
			while (confirmResult.IsSuccess) {
				confirmResult = macroService.PollPattern(patterns.terminusIsle.confirm, {
					DoClick: true, ClickOffset: { X: -25, Y: -25 }, PredicatePattern: [
						patterns.terminusIsle.prompt.next,
						patterns.terminusIsle.prompt.treasureChestFound,
						patterns.terminusIsle.prompt.explorationFailed,
						patterns.terminusIsle.prompt.heroDeployment
					]
				});

				switch (confirmResult.PredicatePath) {
					case 'terminusIsle.prompt.next':
						const title = macroService.GetText(patterns.terminusIsle.prompt.title);
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

				confirmResult = macroService.PollPattern(patterns.terminusIsle.confirm, { TimeoutMs: 3_000 });
			}
			
			let moonlitFangBossResult = macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { TimeoutMs: 3_000 });
			while (moonlitFangBossResult.IsSuccess) {
				macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { DoClick: true, PredicatePattern: patterns.terminusIsle.prompt.heroDeployment, });
				macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
				const bossType = detectBossType();
				selectTeam(bossType === 'light' ? 'dark' : 'light');
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
				macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.terminusIsle.stage });

				moonlitFangBossResult = macroService.PollPattern(patterns.terminusIsle.moonlitFangBoss, { TimeoutMs: 3_000 });
			}

			if (macroService.IsRunning) {
				daily.doTerminusIsleExploration.done.IsChecked = true;
			}
			logger.isPersistingLogs = false;
			return;
	}
	sleep(1_000);
}

function deployHeroes() {
	macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment.skip, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });

	//macroService.PollPattern(patterns.terminusIsle.prompt.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
	//const recommendedElementPatterns = ['earth', 'water', 'fire', 'light', 'dark'].map(el => patterns.terminusIsle.prompt.heroDeployment.recommendedElement[el]);
	//const recommendedElementResult = macroService.PollPattern(recommendedElementPatterns);
	//const recommendedElement = recommendedElementResult.Path?.split('.').pop();
	//selectTeam(recommendedElement);
	//macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
	//macroService.PollPattern(patterns.battle.exit, { DoClick: true, ClickPattern: patterns.terminusIsle.prompt.next, PredicatePattern: patterns.general.tapEmptySpace });
}