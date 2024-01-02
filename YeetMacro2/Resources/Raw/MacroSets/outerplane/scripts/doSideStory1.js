// Auto or sweep side story (1)
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.sideStory];
const daily = dailyManager.GetDaily();
const sideStoryNumber = 1;
const targetCharacter = settings[`doSideStory${sideStoryNumber}`].targetCharacter.Value;
const sweepBattle = settings[`doSideStory${sideStoryNumber}`].sweepBattle.Value;
const teamSlot = settings[`doSideStory${sideStoryNumber}`].teamSlot.Value;
if (daily[`doSideStory${sideStoryNumber}`].done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	let sideStorySection;
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
				case 'ame':
					sideStorySection = patterns.sideStory.theOtherSideOfTheWorld;
					break;
				case 'valentine':
					sideStorySection = patterns.sideStory.aBriefInterlude;
					break;
				default:
					throw new Error(`Unhandled targetCharacter: ${targetCharacter}`);
			}

			macroService.PollPattern(sideStorySection, { DoClick: true, PredicatePattern: sideStorySection.selected });
			const swipeResult = macroService.SwipePollPattern(sideStorySection[targetCharacter], { Start: { X: 1000, Y: 850 }, End: { X: 500, Y: 850 } });
			if (!swipeResult.IsSuccess) {
				throw new Error(`Could not find side story for ${targetCharacter}`);
			}
			macroService.PollPattern(sideStorySection[targetCharacter], { DoClick: true, PredicatePattern: [patterns.battle.selectTeam, patterns.sideStory.enter] });
			macroService.PollPattern(patterns.sideStory.shardStage, { DoClick: true, PredicatePattern: patterns.sideStory.rewardSetting });
			macroService.PollPattern(patterns.battle.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
			selectTeamAndBattle(teamSlot, sweepBattle);
			if (macroService.IsRunning) {
				daily[`doSideStory${sideStoryNumber}`].done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}