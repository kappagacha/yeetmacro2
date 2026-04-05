// @raw-script
// Auto or skip target special request (ecologyStudy or identification)

function doSpecialRequest(type, targetNumBattles = 0) {
	if (!type) type = settings.doSpecialRequest.type.Value;

	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
	const daily = dailyManager.GetCurrentDaily();

	if (type !== 'ecologyStudy' && type !== 'identification') {
		return `Invalid type: ${type}. Expected 'ecologyStudy' or 'identification'.`;
	}

	const teamSlot = settings.doSpecialRequest[type].teamSlot.Value;
	const target = settings.doSpecialRequest[type].target.Value;
	const logPrefix = type;
	let currentStaminaValue;

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				currentStaminaValue = getCurrentStaminaValue();
				if (currentStaminaValue < 12) {
					return;
				}

				logger.info(`doSpecialRequest: click adventure tab for ${logPrefix}`);
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info(`doSpecialRequest: click challenge for ${logPrefix}`);
				macroService.ClickPattern(patterns.adventure.challenge);
				sleep(500);
				break;
			case 'titles.challenge':
				currentStaminaValue = getCurrentStaminaValue();
				if (currentStaminaValue < 12) {
					return;
				}

				logger.info(`doSpecialRequest: ${logPrefix} - ${target}`);
				macroService.PollPattern(patterns.challenge[type], { DoClick: true, PredicatePattern: patterns.challenge.enter });
				macroService.PollPattern(patterns.challenge[type][target].stars, { DoClick: true, PredicatePattern: patterns.challenge[patternPrefix][target] });
				macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
				clickBottomThreeStars();
				const teamsSetupResult = macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
				if (teamsSetupResult.PredicatePath === 'battle.restore') {
					return;
				}
				selectTeamAndBattle(teamSlot, { targetNumBattles });

				if (macroService.IsRunning) daily[`do${capitalize(type)}`].count.Count++;
				return;
		}
		sleep(1_000);
	}

	function capitalize(str) {
		return str.charAt(0).toUpperCase() + str.slice(1);
	}

	function clickBottomThreeStars() {
		if (!macroService.IsRunning) return;

		const stage13Result = macroService.PollPattern(patterns.challenge.stage13, { TimeoutMs: 3_000 });
		const stage13Y = stage13Result.IsSuccess ? stage13Result.Point.Y - 50 : 1080;
		const threeStarsResult = macroService.FindPattern(patterns.challenge.threeStars, { Limit: 10 });
		const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = p.Y > stage13Y || maxY > p.Y ? maxY : p.Y), 0);
		const bottomThreeStars = macroService.ClonePattern(patterns.challenge.threeStars, { CenterY: maxY, Height: 60.0 });
		const threeStarsSelected = macroService.ClonePattern(patterns.challenge.threeStars.selected, { CenterY: maxY });
		macroService.PollPattern(bottomThreeStars, { DoClick: true, PredicatePattern: threeStarsSelected });
	}
}

function doEcologyStudy(targetNumBattles = 0) {
	doSpecialRequest('ecologyStudy', targetNumBattles);
}

function doIdentification(targetNumBattles = 0) {
	doSpecialRequest('identification', targetNumBattles);
}
