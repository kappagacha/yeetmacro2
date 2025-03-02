// @position=14
// Skip all special requests once
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doSpecialRequests.teamSlot.Value;
const doRefillStamina = settings.doSpecialRequests.doRefillStamina.Value;

if (daily.doSpecialRequests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

if (doRefillStamina) {
	refillStamina(140);
	goToLobby();
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doSpecialRequests: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doSpecialRequests: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			if (!daily.doSpecialRequests.ecologyStudy.done.IsChecked) {
				doEcologyStudy();
				macroService.PollPattern(patterns.general.back, { DoClick: true, ClickPattern: patterns.challenge.specialRequest.sweepAll.cancel, PredicatePattern: patterns.titles.challenge });
			}
			
			if (!daily.doSpecialRequests.identification.done.IsChecked) {
				doIdentification();
			}

			if (macroService.IsRunning) {
				daily.doSpecialRequests.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function doEcologyStudy() {
	logger.info('doSpecialRequests: doEcologyStudy');
	macroService.PollPattern(patterns.challenge.ecologyStudy, { DoClick: true, PredicatePattern: patterns.challenge.enter });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.ok });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.ok, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });

	//const ecologyStudyTypes = ['masterlessGuardian', 'tyrantToddler', 'unidentifiedChimera', 'sacreedGuardian', 'grandCalamari'];
	//for (const ecologyStudy of ecologyStudyTypes) {
	//	logger.info(`doSpecialRequests: ${ecologyStudy}`);
	//	if (!daily.doSpecialRequests.ecologyStudy[ecologyStudy].IsChecked) {
	//		macroService.PollPattern(patterns.challenge.ecologyStudy[ecologyStudy].stars, { DoClick: true, PredicatePattern: patterns.challenge.ecologyStudy[ecologyStudy] });
	//		macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
	//		clickBottomThreeStars();
	//		macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: patterns.battle.enter });
	//		selectTeam(teamSlot, { applyPreset: true });

	//		macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.minSlider, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle.value1 });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle.sweep.ok });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.sweep.ok, { DoClick: true, ClickPattern: patterns.general.back, PredicatePattern: patterns.challenge.ecologyStudy[ecologyStudy].stars });

	//		if (macroService.IsRunning) {
	//			daily.doSpecialRequests.ecologyStudy[ecologyStudy].IsChecked = true;
	//		}
	//	}
	//}
	if (macroService.IsRunning) {
		daily.doSpecialRequests.ecologyStudy.done.IsChecked = true;
	}
}

function doIdentification() {
	logger.info('doSpecialRequests: doIdentification');
	macroService.PollPattern(patterns.challenge.identification, { DoClick: true, PredicatePattern: patterns.challenge.enter });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.ok });
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.ok, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });

	//const identificationTypes = ['dekRilAndMekRil', 'glicys', 'blazingKnightMeteos', 'arsNova', 'amadeus'];
	//for (const identification of identificationTypes) {
	//	logger.info(`doSpecialRequests: ${identification}`);
	//	if (!daily.doSpecialRequests.identification[identification].IsChecked) {
	//		macroService.PollPattern(patterns.challenge.identification[identification].stars, { DoClick: true, PredicatePattern: patterns.challenge.identification[identification] });
	//		macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
	//		clickBottomThreeStars();
	//		macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: patterns.battle.enter });
	//		selectTeam(teamSlot, { applyPreset: true });

	//		macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.minSlider, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle.value1 });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle.sweep.ok });
	//		macroService.PollPattern(patterns.battle.setup.repeatBattle.sweep.ok, { DoClick: true, ClickPattern: patterns.general.back, PredicatePattern: patterns.challenge.identification[identification].stars });

	//		if (macroService.IsRunning) {
	//			daily.doSpecialRequests.identification[identification].IsChecked = true;
	//		}
	//	}
	//}
	if (macroService.IsRunning) {
		daily.doSpecialRequests.identification.done.IsChecked = true;
	}
}

function clickBottomThreeStars() {
	if (!macroService.IsRunning) return;

	const threeStarsResult = macroService.FindPattern(patterns.challenge.threeStars, { Limit: 10 });
	const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = maxY > p.Y ? maxY : p.Y), 0);
	const bottomThreeStars = macroService.ClonePattern(patterns.challenge.threeStars, { CenterY: maxY, Height: 60.0 });
	const threeStarsSelected = macroService.ClonePattern(patterns.challenge.threeStars.selected, { CenterY: maxY });
	macroService.PollPattern(bottomThreeStars, { DoClick: true, PredicatePattern: threeStarsSelected });
}