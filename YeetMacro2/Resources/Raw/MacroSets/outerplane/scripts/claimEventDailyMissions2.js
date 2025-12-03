// @position=17
// @tags=dailies
// Claim daily event missions
//const loopPatterns = [patterns.lobby.level, patterns.event.close];
const loopPatterns = [patterns.lobby.level, patterns.festival.title];
const daily = dailyManager.GetCurrentDaily();
//const dailyMissionPattern = macroService.ClonePattern(settings.claimEventDailyMissions2.dailyMissionPattern.Value, {
//	X: 230,
//	Y: 200,
//	Width: 400,
//	Height: 800,
//	Path: 'settings.claimEventDailyMissions2.dailyMissionPattern2',
//	OffsetCalcType: 'DockLeft'
//});

if (daily.claimEventDailyMissions2.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

goToLobby();
refillStamina(50);

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.general.tapEmptySpace });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimEventDailyMissions2: click event');
			macroService.ClickPattern(patterns.festival);
			sleep(500);
			break;
		case 'festival.title':
			

			let demiurgeContractNotificationResult = macroService.PollPattern(patterns.festival.demiurgeContract.notification, { TimeoutMs: 3_000 });
			while (demiurgeContractNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.festival.demiurgeContract.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.festival.title });
				demiurgeContractNotificationResult = macroService.PollPattern(patterns.festival.demiurgeContract.notification, { TimeoutMs: 3_000 });
			}

			macroService.PollPattern(patterns.festival.festivalBingo, { DoClick: true, PredicatePattern: patterns.festival.festivalBingo.selected });
			let festivalBingoNotificationResult = macroService.PollPattern(patterns.festival.festivalBingo.notification, { TimeoutMs: 3_000 });
			while (festivalBingoNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.festival.festivalBingo.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, ClickOffset: { X: -40, Y: 40 } });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.festival.title });
				festivalBingoNotificationResult = macroService.PollPattern(patterns.festival.festivalBingo.notification, { TimeoutMs: 3_000 });
			}

			macroService.PollPattern(patterns.festival.showdown, { DoClick: true, PredicatePattern: patterns.festival.showdown.selected });
			macroService.PollPattern(patterns.festival.showdown.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });

			selectTeamAndBattle("Current", { targetNumBattles: 10 });

			if (macroService.IsRunning) {
				daily.claimEventDailyMissions2.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}
