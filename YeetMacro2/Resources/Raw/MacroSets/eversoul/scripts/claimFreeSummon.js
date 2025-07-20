// @position=4
// Claim free summon
const loopPatterns = [patterns.lobby.level, patterns.summon.info];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimFreeSummon.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}
while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimFreeSummon: click summon');
			const notificationResult = macroService.PollPattern(patterns.lobby.summon.notification, { TimeoutMs: 1_500 });
			if (notificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.lobby.summon);
			} else {	// already claimed
				return;
			}
			break;
		case 'summon.info':
			logger.info('claimFreeSummon: normal summon');
			const swipeResult = macroService.PollPattern(patterns.summon.normal, { SwipePattern: patterns.summon.leftPanelSwipe, TimeoutMs: 10_000 });
			
			if (!swipeResult.IsSuccess) {
				throw new Error('Unable to find normal summon');
			}
			const selectedSummonNormalPattern = macroService.ClonePattern(patterns.summon.normal.selected, { CenterY: swipeResult.Point.Y, Padding: 10 });
			macroService.PollPattern(patterns.summon.normal, { DoClick: true, PredicatePattern: selectedSummonNormalPattern });

			macroService.PollPattern(patterns.summon.normal.free, { DoClick: true, PredicatePattern: patterns.summon.normal.free.confirm });
			macroService.PollPattern(patterns.summon.normal.free.confirm, { DoClick: true, PredicatePattern: patterns.summon.skip });
			macroService.PollPattern(patterns.summon.skip, { DoClick: true, PredicatePattern: patterns.general.back, IntervalDelayMs: 2_500 });

			if (settings.claimFreeSummon.doOneArtifact.Value && !daily.claimFreeSummon.doOneArtifact.IsChecked) {
				let summonInfoResult = macroService.FindPattern(patterns.summon.info);
				while (!summonInfoResult.IsSuccess) {
					// only click back if you see home
					if (macroService.FindPattern(patterns.general.home).IsSuccess) {
						macroService.ClickPattern(patterns.general.back);
						sleep(1_000);
					}
					summonInfoResult = macroService.FindPattern(patterns.summon.info);
					sleep(200);
				}

				const artifactSwipeResult = macroService.PollPattern(patterns.summon.artifact, { SwipePattern: patterns.summon.leftPanelSwipe, TimeoutMs: 10_000 });

				if (!artifactSwipeResult.IsSuccess) {
					throw new Error('Unable to find artifact summon');
				}

				macroService.PollPattern(patterns.summon.artifact, { DoClick: true, PredicatePattern: patterns.summon.artifact.ticket });
				macroService.PollPattern(patterns.summon.artifact.ticket, { DoClick: true, PredicatePattern: patterns.summon.normal.free.confirm });
				macroService.PollPattern(patterns.summon.normal.free.confirm, { DoClick: true, PredicatePattern: patterns.summon.skip });
				macroService.PollPattern(patterns.summon.skip, { DoClick: true, PredicatePattern: patterns.general.back, IntervalDelayMs: 2_500 });

				if (macroService.IsRunning) {
					daily.claimFreeSummon.doOneArtifact.IsChecked = true;
				}
			}

			logger.info('claimFreeSummon: done');
			if (macroService.IsRunning) {
				daily.claimFreeSummon.done.IsChecked = true;
			}

			return;
	}

	sleep(1_000);
}