// Auto or skip target identification
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();
const teamSlot = settings.doIdentification.teamSlot.Value;
const sweepBattle = settings.doIdentification.sweepBattle.Value;
const identification = settings.doIdentification.targetIdentification.Value;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doIdentification: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doIdentification: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			logger.info(`doIdentification: ${identification}`);
			macroService.PollPattern(patterns.challenge.identification, { DoClick: true, PredicatePattern: patterns.challenge.enter });
			macroService.PollPattern(patterns.challenge.identification[identification].stars, { DoClick: true, PredicatePattern: patterns.challenge.identification[identification] });
			macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
			clickBottomThreeStars();
			macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot, sweepBattle);

			if (macroService.IsRunning) {
				daily.doIdentification.count.Count++;
			}
			return;
	}
	sleep(1_000);
}

function clickBottomThreeStars() {
	if (!macroService.IsRunning) return;

	const threeStarsResult = macroService.FindPattern(patterns.challenge.threeStars, { Limit: 10 });
	const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = maxY > p.Y ? maxY : p.Y), 0);
	const bottomThreeStars = macroService.ClonePattern(patterns.challenge.threeStars, { CenterY: maxY, Height: 60.0 });
	const threeStarsSelected = macroService.ClonePattern(patterns.challenge.threeStars.selected, { CenterY: maxY });
	macroService.PollPattern(bottomThreeStars, { DoClick: true, PredicatePattern: threeStarsSelected });
}