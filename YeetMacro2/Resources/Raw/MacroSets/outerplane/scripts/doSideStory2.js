// Auto or sweep side story (2)
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.sideStory];
const daily = dailyManager.GetDaily();
const sideStoryNumber = 2;
const targetCharacter = settings[`doSideStory${sideStoryNumber}`].targetCharacter.Value;
const sweepBattle = settings[`doSideStory${sideStoryNumber}`].sweepBattle.Value;
const teamSlot = settings[`doSideStory${sideStoryNumber}`].teamSlot.Value;
if (daily[`doSideStory${sideStoryNumber}`].done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info(`doSideStory${sideStoryNumber}: click adventure tab`);
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info(`doSideStory${sideStoryNumber}: click sideStory`);
			macroService.ClickPattern(patterns.adventure.sideStory);
			sleep(500);
			break;
		case 'titles.sideStory':
			logger.info(`doSideStory${sideStoryNumber}: ${targetCharacter}`);
			switch (targetCharacter) {
				case 'noa':
				case 'veronica':
					macroService.PollPattern(patterns.sideStory.theOtherSideOfTheWorld, { DoClick: true, PredicatePattern: patterns.sideStory.theOtherSideOfTheWorld.selected });
					const swipeResult = macroService.SwipePollPattern(patterns.sideStory.theOtherSideOfTheWorld[targetCharacter], { Start: { X: 1000, Y: 850 }, End: { X: 500, Y: 850 } });
					if (!swipeResult.IsSuccess) {
						throw new Error(`Could not find side story for ${targetCharacter}`);
					}
					const targetCharacterResult = macroService.PollPattern(patterns.sideStory.theOtherSideOfTheWorld[targetCharacter], { DoClick: true, PredicatePattern: [patterns.battle.selectTeam, patterns.sideStory.enter] });
					if (targetCharacterResult.PredicatePath === 'sideStory.enter') {
						const threeStarsResult = macroService.FindPattern(patterns.sideStory.threeStars, { Limit: 10 });
						const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = maxY > p.Y ? maxY : p.Y), 0);
						if (!maxY) {
							throw new Error('Could not find bottom three stars');
						}
						const bottomThreeStars = macroService.ClonePattern(patterns.sideStory.threeStars, { CenterY: maxY, Height: 60, Path: `sideStory.threeStars_${maxY}` });
						macroService.PollPattern(bottomThreeStars, { DoClick: true, ClickOffset: { X: -100 }, PredicatePattern: patterns.battle.selectTeam });
					}
					macroService.PollPattern(patterns.battle.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
					selectTeamAndBattle(teamSlot, sweepBattle);
					if (macroService.IsRunning) {
						daily[`doSideStory${sideStoryNumber}`].done.IsChecked = true;
					}
					break;
			}
			return;
	}
	sleep(1_000);
}