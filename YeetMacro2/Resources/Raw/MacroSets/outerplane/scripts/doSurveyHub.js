// @position=15
// Spend remaining stamina on survey hub
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.surveyHub, patterns.surveyHub.rewardInfo];
const daily = dailyManager.GetDaily();
const teamSlot = settings.doSurveyHub.teamSlot.Value;
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
			currentStamina = macroService.GetText(patterns.general.staminaValue);
			if (currentStamina < 10) {
				return;
			}
			macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.surveyHub });
			sleep(500);
			break;
		case 'titles.surveyHub':
			logger.info('doSurveyHub: click placesToEarnPoints');
			macroService.ClickPattern(patterns.surveyHub.placesToEarnPoints);
			break;
		case 'surveyHub.rewardInfo':
			logger.info('doSurveyHub: find entry');
			const topLeft = macroService.GetTopLeft();
			const xLocation = topLeft.X + 1100;
			macroService.SwipePollPattern([patterns.surveyHub.rewardInfo.lastLevel, patterns.surveyHub.rewardInfo.zeroOutOfFive], { MaxSwipes: 7, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
			sleep(1000);
			macroService.SwipePollPattern(patterns.surveyHub.rewardInfo.rightArrow, { MaxSwipes: 7, Start: { X: xLocation, Y: 280 }, End: { X: xLocation, Y: 800 } });
			sleep(1000);
			const rightArrowResult = macroService.PollPattern(patterns.surveyHub.rewardInfo.rightArrow, { Limit: 6 });
			const maxY = rightArrowResult.Points.reduce((maxY, p) => (maxY = maxY > p.Y ? maxY : p.Y), 0);
			const bottomRightArrow = macroService.ClonePattern(patterns.surveyHub.rewardInfo.rightArrow, { CenterY: maxY, Height: 60.0 });
			macroService.PollPattern(bottomRightArrow, { DoClick: true, PredicatePattern: patterns.surveyHub.selectTeam });
			const selectTeamResult = macroService.PollPattern(patterns.surveyHub.selectTeam, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
			if (selectTeamResult.PredicatePath === 'battle.restore') {
				return;
			}
			selectTeamAndBattle(teamSlot, true);
			sleep(500);
			currentStamina = macroService.GetText(patterns.general.staminaValue);
			if (currentStamina < 10) {
				return;
			}
			macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.adventure.surveyHub });
			macroService.PollPattern(patterns.adventure.surveyHub, { DoClick: true, PredicatePattern: patterns.titles.surveyHub });
			break;
	}
	sleep(1_000);
}