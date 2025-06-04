// @isFavorite
// Auto or sweep doppelganger
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge, patterns.titles.doppelganger];
const daily = dailyManager.GetCurrentDaily();
const elementTypeTarget1 = settings.doDoppelganger.elementTypeTarget1.Value;
const teamSlot1 = settings.doDoppelganger.teamSlot1.Value;
const elementTypeTarget2 = settings.doDoppelganger.elementTypeTarget2.Value;
const teamSlot2 = settings.doDoppelganger.teamSlot2.Value;
const doRefillStamina = settings.doDoppelganger.doRefillStamina.Value;

if (daily.doDoppelganger.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

if (doRefillStamina) {
	refillStamina(60);
	goToLobby();
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doDoppelganger: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doDoppelganger: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			logger.info('doDoppelganger: click doppelganger');
			macroService.ClickPattern(patterns.challenge.doppelganger);
			sleep(500);
			break;
		case 'titles.doppelganger':
			logger.info(`doDoppelganger elementTypeTarget1 ${elementTypeTarget1}: auto or sweep`);
			macroService.PollPattern(patterns.doppelganger[elementTypeTarget1], { DoClick: true, PredicatePattern: patterns.doppelganger[elementTypeTarget1].selected });
			macroService.PollPattern(patterns.doppelganger.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });

			const recommendedElement = {
				earth: 'fire',
				water: 'earth',
				fire: 'water',
				light: 'dark',
				dark: 'light'
			};

			selectTeamAndBattle(teamSlot1 === 'RecommendedElement' ? recommendedElement[elementTypeTarget1] : teamSlot1);
			goBackToDoppelgangerScreen();

			if (elementTypeTarget1 !== elementTypeTarget2) {
				logger.info(`doDoppelganger elementTypeTarget2 ${elementTypeTarget2}: auto or sweep`);
				macroService.PollPattern(patterns.doppelganger[elementTypeTarget2], { DoClick: true, PredicatePattern: patterns.doppelganger[elementTypeTarget2].selected });
				macroService.PollPattern(patterns.doppelganger.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
				selectTeamAndBattle(teamSlot2 === 'RecommendedElement' ? recommendedElement[elementTypeTarget2] : teamSlot2);
				goBackToDoppelgangerScreen();
			}

			if (macroService.IsRunning) {
				daily.doDoppelganger.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}

function goBackToDoppelgangerScreen() {
	macroService.PollPattern(patterns.battle.setup.enter.ok, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
	let doppelgangerTitleResult = macroService.FindPattern(patterns.titles.doppelganger);
	while (macroService.IsRunning && !doppelgangerTitleResult.IsSuccess) {
		macroService.ClickPattern(patterns.general.back);
		sleep(1_000);
		doppelgangerTitleResult = macroService.FindPattern(patterns.titles.doppelganger);
		sleep(1_000);
	}
}