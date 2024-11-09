// @raw-script
// Auto or skip target ecology study

function doEcologyStudy(targetNumBattles = 0) {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
	const daily = dailyManager.GetCurrentDaily();
	const teamSlot = settings.doEcologyStudy.teamSlot.Value;
	const sweepBattle = settings.doEcologyStudy.sweepBattle.Value;
	const ecologyStudy = settings.doEcologyStudy.targetEcologyStudy.Value;

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doEcologyStudy: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doEcologyStudy: click challenge');
				macroService.ClickPattern(patterns.adventure.challenge);
				sleep(500);
				break;
			case 'titles.challenge':
				logger.info(`doEcologyStudy: ${ecologyStudy}`);
				macroService.PollPattern(patterns.challenge.ecologyStudy, { DoClick: true, PredicatePattern: patterns.challenge.enter });
				macroService.PollPattern(patterns.challenge.ecologyStudy[ecologyStudy].stars, { DoClick: true, PredicatePattern: patterns.challenge.ecologyStudy[ecologyStudy] });
				macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
				clickBottomThreeStars();
				const teamsSetupResult = macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
				if (teamsSetupResult.PredicatePath === 'battle.restore') {
					return;
				}
				selectTeamAndBattle(teamSlot, sweepBattle, targetNumBattles);

				if (macroService.IsRunning) {
					daily.doEcologyStudy.count.Count++;
				}
				return;
		}
		sleep(1_000);
	}
}

function clickBottomThreeStars() {
	if (!macroService.IsRunning) return;

	const stage13Result = macroService.PollPattern(patterns.challenge.stage13, { TimeoutMs: 1_500 });
	const stage13Y = stage13Result.IsSuccess ? stage13Result.Point.Y - 50 : 1080;
	const threeStarsResult = macroService.FindPattern(patterns.challenge.threeStars, { Limit: 10 });
	const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = p.Y > stage13Y || maxY > p.Y ? maxY : p.Y), 0);
	const bottomThreeStars = macroService.ClonePattern(patterns.challenge.threeStars, { CenterY: maxY, Height: 60.0 });
	const threeStarsSelected = macroService.ClonePattern(patterns.challenge.threeStars.selected, { CenterY: maxY });
	macroService.PollPattern(bottomThreeStars, { DoClick: true, PredicatePattern: threeStarsSelected });
}