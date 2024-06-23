// @position=11
// Skip decoy operation
const loopPatterns = [patterns.lobby.level, patterns.general.back];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const lobbySwipeXStart = resolution.Width - 50;
const lobbySwipeXEnd = lobbySwipeXStart - 300;

if (daily.sweepEvent.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const eventRaidPattern = macroService.ClonePattern(settings.sweepEvent.eventRaidPattern.Value, {
	X: 10,
	Y: 130,
	Width: 240,
	Height: 930,
	Path: 'settings.eventRaid.eventRaidPattern',
	OffsetCalcType: 'DockLeft'
});

const eventStagePattern = macroService.ClonePattern(settings.sweepEvent.eventStagePattern.Value, {
	X: 10,
	Y: 130,
	Width: 240,
	Height: 930,
	Path: 'settings.eventRaid.eventStagePattern',
	OffsetCalcType: 'DockLeft'
});


while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepEvent: click attendance check');
			const attendanceCheckSwipeResult = macroService.SwipePollPattern(patterns.lobby.attendanceCheck, { MaxSwipes: 10, Start: { X: lobbySwipeXStart, Y: 300 }, End: { X: lobbySwipeXEnd, Y: 300 }, PollTimeoutMs: 4_000 });
			if (!attendanceCheckSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: patterns.lobby.attendanceCheck');
			}

			macroService.PollPattern(patterns.lobby.attendanceCheck, { DoClick: true, PredicatePattern: patterns.general.back });
			break;
		case 'general.back':
			if (!daily.sweepEvent.eventRaid.IsChecked) {
				const eventRaidSwipeResult = macroService.SwipePollPattern(eventRaidPattern, { Start: { X: 50, Y: 800 }, End: { X: 50, Y: 500 } });
				if (!eventRaidSwipeResult.IsSuccess) {
					throw Error('Unable to find pattern: settings.eventRaid.eventRaidPattern');
				}
				macroService.PollPattern(eventRaidPattern, { DoClick: true, PredicatePattern: patterns.event.eventRaid });
				macroService.PollPattern(patterns.event.eventRaid, { DoClick: true, PredicatePattern: patterns.event.eventRaid.sweep, ClickOffset: { Y: 60 } });
				macroService.PollPattern(patterns.event.eventRaid.sweep, { DoClick: true, PredicatePattern: patterns.event.eventRaid.sweep.sweep });
				macroService.PollPattern(patterns.event.max, { DoClick: true, InversePredicatePattern: patterns.event.max });
				macroService.PollPattern(patterns.event.eventRaid.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
				macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.general.back });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.event.eventRaid });

				if (macroService.IsRunning) {
					daily.sweepEvent.eventRaid.IsChecked = true;
				}

				const eventStageSwipeResult = macroService.SwipePollPattern(eventStagePattern, { Start: { X: 50, Y: 800 }, End: { X: 50, Y: 500 } });
				if (!eventStageSwipeResult.IsSuccess) {
					throw Error('Unable to find pattern: settings.eventRaid.eventStagePattern');
				}
			}

			if (!daily.sweepEvent.eventStage.IsChecked) {
				macroService.PollPattern(eventStagePattern, { DoClick: true, PredicatePattern: patterns.event.eventStage });

				// currency amount 1 is to the left in event banner page
				const currencyAmount1 = macroService.GetText(patterns.event.eventStage.currency1.amount);
				// currency amount 2 is to the right in event banner page
				const currencyAmount2 = macroService.GetText(patterns.event.eventStage.currency2.amount);

				logger.info(`sweepEvent: currencyAmount1 ${currencyAmount1} VS currencyAmount2 ${currencyAmount2}`);

				macroService.PollPattern(patterns.event.eventStage, { DoClick: true, PredicatePattern: patterns.event.eventStage.sweep, ClickOffset: { Y: 60 } });

				// currency amount 1 is the right stage
				// currency amount 2 is the left stage
				if (currencyAmount1 < currencyAmount2) {
					macroService.PollPattern(patterns.event.eventStage.currency1.play, { DoClick: true, PredicatePattern: patterns.event.eventStage.currency1.enemyLvl });
				}
				macroService.PollPattern(patterns.event.eventStage.sweep, { DoClick: true, PredicatePattern: patterns.event.eventStage.sweep.sweep });
				macroService.PollPattern(patterns.event.max, { DoClick: true, InversePredicatePattern: patterns.event.max });
				macroService.PollPattern(patterns.event.eventStage.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
				macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.general.back });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.event.eventStage });

				if (macroService.IsRunning) {
					daily.sweepEvent.eventStage.IsChecked = true;
				}
			}

			if (macroService.IsRunning) {
				daily.sweepEvent.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}