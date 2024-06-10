// @position=13
// Auto or sweep doppelganger
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge, patterns.titles.doppelganger];
const daily = dailyManager.GetCurrentDaily();
const elementTypeTarget1 = settings.doDoppelganger.elementTypeTarget1.Value;
const teamSlot1 = settings.doDoppelganger.teamSlot1.Value;
const sweepBattle1 = settings.doDoppelganger.sweepBattle1.Value;
const elementTypeTarget2 = settings.doDoppelganger.elementTypeTarget2.Value;
const teamSlot2 = settings.doDoppelganger.teamSlot2.Value;
const sweepBattle2 = settings.doDoppelganger.sweepBattle2.Value;
const checkPiecesLimit1 = settings.doDoppelganger.checkPiecesLimit1.Value;
//const checkPiecesLimit2 = settings.doDoppelganger.checkPiecesLimit2.Value;
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
			logger.info('doDoppelganger elementTypeTarget1: auto or sweep');
			macroService.PollPattern(patterns.doppelganger[elementTypeTarget1], { DoClick: true, PredicatePattern: patterns.doppelganger[elementTypeTarget1].selected });
			if (checkPiecesLimit1 && macroService.FindPattern(patterns.doppelganger.piecesLimit1).IsSuccess) {
				throw new Error('Failed checkPiecesLimit1');
			}
			macroService.PollPattern(patterns.doppelganger.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
			selectTeamAndBattle(teamSlot1, sweepBattle1);
			goBackToDoppelgangerScreen(sweepBattle1);

			if (elementTypeTarget1 !== elementTypeTarget2) {
				logger.info('doDoppelganger elementTypeTarget2: auto or sweep');
				macroService.PollPattern(patterns.doppelganger[elementTypeTarget2], { DoClick: true, PredicatePattern: patterns.doppelganger[elementTypeTarget2].selected });
				if (checkPiecesLimit1 && macroService.FindPattern(patterns.doppelganger.piecesLimit1).IsSuccess) {
					throw new Error('Failed checkPiecesLimit1');
				}
				macroService.PollPattern(patterns.doppelganger.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
				selectTeamAndBattle(teamSlot2, sweepBattle2);
				goBackToDoppelgangerScreen(sweepBattle2);
			}

			if (macroService.IsRunning) {
				daily.doDoppelganger.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}

function goBackToDoppelgangerScreen(sweepBattle) {
	if (sweepBattle) {
		macroService.PollPattern(patterns.general.ok, { DoClick: true, PredicatePattern: patterns.battle.exit });
		macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.titles.doppelganger });
	} else {
		macroService.PollPattern(patterns.battle.setup.enter.ok, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
		let doppelgangerTitleResult = macroService.FindPattern(patterns.titles.doppelganger);
		while (macroService.IsRunning && !doppelgangerTitleResult.IsSuccess) {
			macroService.ClickPattern(patterns.general.back);
			sleep(1_000);
			doppelgangerTitleResult = macroService.FindPattern(patterns.titles.doppelganger);
			sleep(1_000);
		}
	}
}