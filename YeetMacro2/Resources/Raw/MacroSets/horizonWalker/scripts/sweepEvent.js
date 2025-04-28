// sweep event and claim rewards
const loopPatterns = [patterns.lobby.stage, patterns.phone.battery, patterns.event.claimAll, patterns.event.schedule];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepEvent.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.event.claimAll });
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('sweepEvent: click phone');
			macroService.ClickPattern(patterns.lobby.stage, { ClickOffset: { X: 200, Y: 200 } });
			break;
		case 'phone.battery':
			logger.info('sweepEvent: click event');
			macroService.ClickPattern(patterns.phone.supply, { ClickOffset: { X: 200 } });
			break;
		case 'event.claimAll':
			logger.info('sweepEvent: click claim all');
			macroService.PollPattern(patterns.event.claimAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.event.claimAll.close });
			macroService.PollPattern(patterns.event.claimAll.close, { DoClick: true, PredicatePattern: patterns.event.schedule });
			break;
		case 'event.schedule':
			logger.info('sweepEvent: sweep story');
			macroService.PollPattern(patterns.event.enterStory, { DoClick: true, PredicatePattern: patterns.titles.stage });
			macroService.PollPattern(patterns.event.stage);
			const stageResult = macroService.FindPattern(patterns.event.stage, { Limit: 10 });
			const stageMaxYPoint = stageResult.Points.reduce((maxYPoint, p) => (maxYPoint = maxYPoint.Y > p.Y ? maxYPoint : p), 0);
			macroService.PollPoint(stageMaxYPoint, { DoClick: true, PredicatePattern: patterns.event.star });

			const starResult = macroService.FindPattern(patterns.event.star, { Limit: 30 });
			const starMaxYPoint = starResult.Points.reduce((maxYPoint, p) => (maxYPoint = maxYPoint.Y > p.Y ? maxYPoint : p), 0);
			macroService.PollPoint(starMaxYPoint, { DoClick: true, PredicatePattern: patterns.battle.deploy });

			let notificationResult = macroService.PollPattern(patterns.battle.deploy, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.battle.sweeping, { DoClick: true, PredicatePattern: patterns.battle.sweeping.confirm });
				macroService.PollPattern(patterns.battle.sweeping.confirm, { DoClick: true, PredicatePattern: patterns.general.confirm2 });
				macroService.PollPattern(patterns.general.confirm2, { DoClick: true, PredicatePattern: patterns.titles.stage });

				notificationResult = macroService.PollPattern(patterns.battle.deploy, { TimeoutMs: 3_000 });
			}

			logger.info('sweepEvent: go back to event');
			let enterStoryResult = macroService.FindPattern(patterns.event.enterStory);
			while (!enterStoryResult.IsSuccess) {
				macroService.ClickPattern(patterns.general.back);
				sleep(1000);
				enterStoryResult = macroService.FindPattern(patterns.event.enterStory);
			}

			logger.info('sweepEvent: claim rewards');
			macroService.PollPattern(patterns.event.schedule, { DoClick: true, PredicatePattern: patterns.event.schedule.claimAll });
			macroService.PollPattern(patterns.event.schedule.claimAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.event.schedule.close });
			macroService.PollPattern(patterns.event.schedule.close, { DoClick: true, PredicatePattern: patterns.event.enterStory });

			macroService.IsRunning && (daily.sweepEvent.done.IsChecked = true);
			return;
	}
	sleep(1_000);
}
