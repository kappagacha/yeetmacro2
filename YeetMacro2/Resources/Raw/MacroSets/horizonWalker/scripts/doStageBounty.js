// do stage bounty
const loopPatterns = [patterns.phone.battery, patterns.stage.title, patterns.stage.bounty.title];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.doStageBounty.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetBountyPattern = macroService.ClonePattern(settings.doStageBounty.targetBountyPattern.Value, {
	X: 50,
	Y: 230,
	//Width: 1600,
	Width: resolution.Width - 60,
	Height: 790,
	Path: 'settings.doStageBounty.targetBountyPattern',
	OffsetCalcType: 'DockLeft'
});

const targetFloorPattern = macroService.ClonePattern(settings.doStageBounty.targetFloorPattern.Value, {
	X: 50,
	Y: 230,
	//Width: 1700,
	Width: resolution.Width - 60,
	Height: 790,
	Path: 'settings.doStageBounty.targetFloorPattern',
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('doStageWish: click stage');
			macroService.ClickPattern(patterns.stage);
			break;
		case 'stage.title':
			logger.info('doStageWish: click bounty');
			macroService.ClickPattern(patterns.stage.bounty);
			break;
		case 'stage.bounty.title':
			logger.info('doStageBounty: do bounty missions');
			macroService.PollPattern(targetBountyPattern, { DoClick: true, PredicatePattern: patterns.stage.bounty.floor });
			const targetFloorPatternResult = macroService.PollPattern(targetFloorPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });
			if (!targetFloorPatternResult.IsSuccess) {
				throw Error('Could not find target floor pattern');
			}
			macroService.PollPattern(targetFloorPattern, { DoClick: true, ClickPattern: patterns.heroList.close, PredicatePattern: patterns.battle.deploy });
			macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.stage.bounty.title });

			logger.info('doStageBounty: sweep bounty missions');
			macroService.PollPattern(targetFloorPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });
			macroService.PollPattern(targetFloorPattern, { DoClick: true, ClickPattern: patterns.heroList.close, PredicatePattern: patterns.battle.deploy });
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