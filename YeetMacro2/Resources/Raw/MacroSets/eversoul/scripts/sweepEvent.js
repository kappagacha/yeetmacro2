﻿// @position=11
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
			const attendanceCheckSwipeResult = macroService.PollPattern(patterns.lobby.attendanceCheck, { SwipePattern: patterns.lobby.attendanceCheck.swipe, TimoutMs: 10_000 });
			if (!attendanceCheckSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: patterns.lobby.attendanceCheck');
			}

			macroService.PollPattern(patterns.lobby.attendanceCheck, { DoClick: true, PredicatePattern: patterns.general.back });
			break;
		case 'general.back':
			if (settings.sweepEvent.doEventRaid.Value && !daily.sweepEvent.eventRaid.IsChecked) {
				const eventRaidSwipeResult = macroService.PollPattern(eventRaidPattern, { SwipePattern: patterns.event.leftPanelSwipe, TimoutMs: 10_000 });
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
			}

			if (settings.sweepEvent.doEventStage.Value && !daily.sweepEvent.eventStage.IsChecked) {
				const eventStageSwipeResult = macroService.PollPattern(eventStagePattern, { SwipePattern: patterns.event.leftPanelSwipe, TimoutMs: 10_000 });
				if (!eventStageSwipeResult.IsSuccess) {
					throw Error('Unable to find pattern: settings.eventRaid.eventStagePattern');
				}

				macroService.PollPattern(eventStagePattern, { DoClick: true, PredicatePattern: patterns.event.eventStage });
				let eventStageResult = macroService.PollPattern(patterns.event.eventStage, { DoClick: true, PredicatePattern: [patterns.event.eventStage.sweep, patterns.event.eventStage.sweep.disabled], ClickOffset: { Y: 60 } });

				macroService.PollPattern(patterns.event.eventStage.firstStage.play, { DoClick: true, PredicatePattern: patterns.event.eventStage.firstStage.selected });

				macroService.PollPattern(patterns.event.eventStage.currency, { DoClick: true, PredicatePattern: patterns.event.eventStage.currency.current });
				const firstStageCurrencyAmount = macroService.FindText(patterns.event.eventStage.currency.amount).replace(/[,.]/g, '');
				macroService.PollPattern(patterns.event.eventStage.currency.current, { DoClick: true, PredicatePattern: patterns.general.back, ClickOffset: { X: -200 } });

				macroService.PollPattern(patterns.event.eventStage.secondStage.play, { DoClick: true, PredicatePattern: patterns.event.eventStage.secondStage.selected });

				macroService.PollPattern(patterns.event.eventStage.currency, { DoClick: true, PredicatePattern: patterns.event.eventStage.currency.current });
				const secondStageCurrencyAmount = macroService.FindText(patterns.event.eventStage.currency.amount).replace(/[,.]/g, '');
				macroService.PollPattern(patterns.event.eventStage.currency.current, { DoClick: true, PredicatePattern: patterns.general.back, ClickOffset: { X: -200 } });

				logger.info(`sweepEvent: firstStage ${firstStageCurrencyAmount} VS secondStage ${secondStageCurrencyAmount}`);

				if (Number(firstStageCurrencyAmount) < Number(secondStageCurrencyAmount)) {
					macroService.PollPattern(patterns.event.eventStage.firstStage.play, { DoClick: true, PredicatePattern: patterns.event.eventStage.firstStage.selected });
				}
				
				eventStageResult = macroService.PollPattern(patterns.event.eventStage, { PredicatePattern: [patterns.event.eventStage.sweep, patterns.event.eventStage.sweep.disabled]});

				if (eventStageResult.PredicatePath === 'event.eventStage.sweep.disabled') {
					while (macroService.IsRunning) {
						macroService.PollPattern(patterns.event.eventStage.challenge, { DoClick: true, PredicatePattern: patterns.battle.start });
						macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.continue });
						const continueResult = macroService.PollPattern(patterns.battle.continue, { DoClick: true, PredicatePattern: [patterns.event.eventStage.challenge, patterns.event.eventStage.purchaseExtraEntries] });
						if (continueResult.PredicatePath === 'event.eventStage.purchaseExtraEntries') {
							break;
						}
					}
				} else {
					macroService.PollPattern(patterns.event.eventStage.sweep, { DoClick: true, PredicatePattern: patterns.event.eventStage.sweep.sweep });
					macroService.PollPattern(patterns.event.max, { DoClick: true, InversePredicatePattern: patterns.event.max });
					macroService.PollPattern(patterns.event.eventStage.sweep.sweep, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
					macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.general.back });
					macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.event.eventStage });
				}
				
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