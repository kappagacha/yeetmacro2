// sweep event and claim rewards
const loopPatterns = [patterns.phone.battery,  patterns.event.claimAll, patterns.event.schedule];
const daily = dailyManager.GetCurrentDaily();

if (daily.sweepEvent.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.event.claimAll });
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('sweepEvent: click event');
			macroService.ClickPattern(patterns.stage, { ClickOffset: { Y: -350 } });
			break;
		case 'event.claimAll':
			logger.info('sweepEvent: click claim all');
			macroService.PollPattern(patterns.event.claimAll, { DoClick: true, PredicatePattern: patterns.general.touchTheScreen });
			macroService.PollPattern(patterns.general.touchTheScreen, { DoClick: true, PredicatePattern: patterns.event.claimAll.close });
			macroService.PollPattern(patterns.event.claimAll.close, { DoClick: true, PredicatePattern: patterns.event.schedule });
			break;
		case 'event.schedule':
			logger.info('sweepEvent: sweep story');
			macroService.PollPattern(patterns.event.enterStory, { DoClick: true, PredicatePattern: patterns.event.title });
			macroService.PollPattern(patterns.event.hard, { DoClick: true, PredicatePattern: patterns.event.hard.selected, TimeoutMs: 2_500 });

			macroService.SwipePattern(patterns.event.swipeRight);
			sleep(1_000);
			macroService.SwipePattern(patterns.event.swipeRight);
			sleep(1_000);
			macroService.SwipePattern(patterns.event.swipeRight);
			sleep(1_000);

			macroService.PollPattern(patterns.event.eventStory, { SwipePattern: patterns.event.swipeLeft });
			sleep(1_000);

			const eventStoryResult = macroService.FindPattern(patterns.event.eventStory, { Limit: 10 });
			const eventStoryMaxXPoint = eventStoryResult.Points.reduce((maxXPoint, p) => (maxXPoint = maxXPoint.X > p.X ? maxXPoint : p), 0);
			macroService.PollPoint(eventStoryMaxXPoint, { DoClick: true, PredicatePattern: patterns.battle.deploy });

			let deployResult = macroService.PollPattern(patterns.battle.deploy, { TimeoutMs: 3_000 });
			while (macroService.IsRunning && deployResult.IsSuccess) {
				macroService.PollPattern(patterns.battle.sweeping, { DoClick: true, PredicatePattern: patterns.battle.sweeping.confirm });
				macroService.PollPattern(patterns.battle.sweeping.confirm, { DoClick: true, PredicatePattern: patterns.general.confirm2 });
				sleep(1_000);
				macroService.PollPattern(patterns.general.confirm2, { DoClick: true, PredicatePattern: patterns.event.title });

				deployResult = macroService.PollPattern(patterns.battle.deploy, { TimeoutMs: 3_000 });
			}

			logger.info('sweepEvent: go back to event');
			let enterStoryResult = macroService.FindPattern(patterns.event.enterStory);
			while (macroService.IsRunning && !enterStoryResult.IsSuccess) {
				macroService.ClickPattern(patterns.event.back);
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
