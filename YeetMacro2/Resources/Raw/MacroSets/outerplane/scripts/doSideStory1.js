// @position=10
// Auto or sweep side story (1). Required options:
// section: selects the tab
// targetStory: pattern that selects story
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.sideStory];
const daily = dailyManager.GetDaily();
const sideStoryNumber = 1;
const sweepBattle = settings[`doSideStory${sideStoryNumber}`].sweepBattle.Value;
const teamSlot = settings[`doSideStory${sideStoryNumber}`].teamSlot.Value;
const sideStorySection = patterns.sideStory[settings[`doSideStory${sideStoryNumber}`].section.Value];
const resolution = macroService.GetCurrentResolution();
const targetStory = macroService.ClonePattern(settings[`doSideStory${sideStoryNumber}`].targetStory.Value, {
	X: 450,
	Y: 700,
	Width: resolution.Width - 500,
	Height: 300,
	Path: 'settings.outings.target',
	OffsetCalcType: 'DockLeft'
});

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
			logger.info(`doSideStory${sideStoryNumber}: auto or sweep`);
			
			macroService.PollPattern(sideStorySection, { DoClick: true, PredicatePattern: sideStorySection.selected });
			const swipeResult = macroService.SwipePollPattern(targetStory, { Start: { X: 1000, Y: 850 }, End: { X: 500, Y: 850 } });
			if (!swipeResult.IsSuccess) {
				throw new Error('Could not find target side story');
			}
			macroService.PollPattern(targetStory, { DoClick: true, PredicatePattern: [patterns.battle.selectTeam, patterns.sideStory.enter] });
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