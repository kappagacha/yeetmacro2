// @raw-script
// @position=16
// Spend remaining stamina on survey hub

function doSurveyHub(targetNumBattles = 0) {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.surveyHub.rewardInfo, patterns.titles.shop];
	const daily = dailyManager.GetCurrentDaily();
	const teamSlot = settings.doSurveyHub.teamSlot.Value;
	const descendingPriority = settings.doSurveyHub.descendingPriority.Value;

	if (targetNumBattles && daily.doSurveyHub.count.Count >= targetNumBattles) {
		return;
	}

	let currentStamina;
	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns);
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doSurveyHub: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doSurveyHub: go to survey hub');
				macroService.PollPattern(patterns.adventure.adventure, { DoClick: true, PredicatePattern: patterns.adventure.surveyHub });
				sleep(500);
				currentStamina = macroService.FindText(patterns.general.staminaValue);
				if (currentStamina < 10) {
					return;
				}
				macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.shop });
				sleep(500);
				break;
			case 'titles.shop':
				logger.info('doSurveyHub: click placesToEarnPoints');
				macroService.ClickPattern(patterns.surveyHub.placesToEarnPoints);
				break;
			case 'surveyHub.rewardInfo':
				logger.info('doSurveyHub: find entry');
				const topLeft = macroService.GetTopLeft();
				const xLocation = topLeft.X + 1100;
				let selectTeamResult;

				if (descendingPriority) {
					sleep(1000);
					macroService.SwipePollPattern(patterns.surveyHub.rewardInfo.rightArrow, { MaxSwipes: 7, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
					sleep(1000);
					const rightArrowResult = macroService.PollPattern(patterns.surveyHub.rewardInfo.rightArrow, { Limit: 6 });
					const minY = rightArrowResult.Points.reduce((minY, p) => (minY = minY < p.Y ? minY : p.Y), 10_000);
					const topRightArrow = macroService.ClonePattern(patterns.surveyHub.rewardInfo.rightArrow, { CenterY: minY, Height: 60.0 });
					macroService.PollPattern(topRightArrow, { DoClick: true, PredicatePattern: patterns.surveyHub.selectTeam });
					selectTeamResult = macroService.PollPattern(patterns.surveyHub.selectTeam, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
				} else {
					macroService.SwipePollPattern([patterns.surveyHub.rewardInfo.lastLevel, patterns.surveyHub.rewardInfo.zeroOutOfFive, patterns.surveyHub.rewardInfo.lock], { MaxSwipes: 7, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
					sleep(1000);
					macroService.SwipePollPattern(patterns.surveyHub.rewardInfo.rightArrow, { MaxSwipes: 7, Start: { X: xLocation, Y: 280 }, End: { X: xLocation, Y: 800 } });
					sleep(1000);
					const rightArrowResult = macroService.PollPattern(patterns.surveyHub.rewardInfo.rightArrow, { Limit: 6 });
					const maxY = rightArrowResult.Points.reduce((maxY, p) => (maxY = maxY > p.Y ? maxY : p.Y), 0);
					const bottomRightArrow = macroService.ClonePattern(patterns.surveyHub.rewardInfo.rightArrow, { CenterY: maxY, Height: 60.0 });
					macroService.PollPattern(bottomRightArrow, { DoClick: true, PredicatePattern: patterns.surveyHub.selectTeam });
					selectTeamResult = macroService.PollPattern(patterns.surveyHub.selectTeam, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
				}

				if (selectTeamResult.PredicatePath === 'battle.restore') {
					return;
				}

				const numBattles = selectTeamAndBattle(teamSlot);
				if (macroService.IsRunning) {
					daily.doSurveyHub.count.Count += Number(numBattles);
				}

				//if (!sweepBattle) {
				//	macroService.PollPattern(patterns.battle.setup.enter.ok, { DoClick: true, PredicatePattern: patterns.battle.exit });
				//	macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.general.back });
				//	macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.adventure.surveyHub });
				//	//macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.shop });
				//}

				//logger.info(`targetNumBattles: ${targetNumBattles}, numBattles: ${numBattles}, doSurveyHub count: ${daily.doSurveyHub.count.Count}, `)
				if (targetNumBattles && daily.doSurveyHub.count.Count >= targetNumBattles) {
					return;
				}

				sleep(500);
				currentStamina = macroService.FindText(patterns.general.staminaValue);
				if (currentStamina < 10) {
					return;
				}

				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.adventure.surveyHub });
				macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.shop });
				break;
		}
		sleep(1_000);
	}
}
