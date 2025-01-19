// do stage bounty
const loopPatterns = [patterns.lobby.stage, patterns.titles.stage];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.doStageBounty.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetBountyPattern = macroService.ClonePattern(settings.doStageBounty.targetBountyPattern.Value, {
	X: 300,
	Y: 230,
	Width: resolution.Width - 320,
	Height: 790,
	Path: 'settings.doStageBounty.targetBountyPattern',
	OffsetCalcType: 'DockLeft'
});

const targetFloorPattern = macroService.ClonePattern(settings.doStageBounty.targetFloorPattern.Value, {
	X: 200,
	Y: 230,
	Width: resolution.Width - 220,
	Height: 790,
	Path: 'settings.doStageBounty.targetFloorPattern',
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('doStageBounty: click stage');
			macroService.ClickPattern(patterns.lobby.stage);
			break;
		case 'titles.stage':
			logger.info('doStageBounty: do bounty missions');
			macroService.PollPattern(patterns.stage.bounty, { DoClick: true, PredicatePattern: patterns.stage.bounty.selected });
			macroService.PollPattern(targetBountyPattern, { DoClick: true, PredicatePattern: targetFloorPattern });
			macroService.PollPattern(targetFloorPattern, { DoClick: true, PredicatePattern: patterns.battle.deploy });
			macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.titles.stage });

			logger.info('doStageBounty: sweep bounty missions');
			macroService.PollPattern(targetFloorPattern, { DoClick: true, PredicatePattern: patterns.battle.deploy });
			for (let i = 0; i < 2; i++) {
				macroService.PollPattern(patterns.stage.bounty.sweep, { DoClick: true, PredicatePattern: patterns.stage.bounty.sweep.confirm });
				macroService.PollPattern(patterns.stage.bounty.sweep.confirm, { DoClick: true, PredicatePattern: patterns.general.confirm2 });
				macroService.PollPattern(patterns.general.confirm2, { DoClick: true, PredicatePattern: patterns.battle.deploy });
			}
			
			macroService.IsRunning && (daily.doStageBounty.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}