// @position=11
// do operation eden alliance
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.operationEdenAlliance];
const daily = dailyManager.GetCurrentDaily();

if (daily.doOperationEdenAlliance.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doOperationEdenAlliance: click adventure');
			macroService.ClickPattern(patterns.lobby.adventure);
			break;
		case 'titles.adventure':
			logger.info('doOperationEdenAlliance: click operation eden alliance');
			const operationEdenAllianceNotificationResult = macroService.PollPattern(patterns.adventure.operationEdenAlliance.notification, { TimeoutMs: 3_000 });
			if (!operationEdenAllianceNotificationResult.IsSuccess) {
				if (macroService.IsRunning) {
					daily.doOperationEdenAlliance.done.IsChecked = true;
				}
				return;
			}
			
			macroService.PollPattern(patterns.adventure.operationEdenAlliance, { DoClick: true, PredicatePattern: patterns.adventure.operationEdenAlliance.operationEdenAlliance });
			macroService.PollPattern(patterns.adventure.operationEdenAlliance.operationEdenAlliance, { DoClick: true, PredicatePattern: patterns.titles.operationEdenAlliance });
			break;
		case 'titles.operationEdenAlliance':
			macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });
			const lv1Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv1, { Limit: 3 });
			lv1Result.Points.sort((a, b) => a.Y - b.Y);
			for (const p of lv1Result.Points) {
				const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			}

			const lv2Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv2, { Limit: 3 });
			lv2Result.Points.sort((a, b) => a.Y - b.Y);
			for (const p of lv2Result.Points.slice(0, 2)) {
				const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			}

			const lv3Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv3, { Limit: 3 });
			lv3Result.Points.sort((a, b) => a.Y - b.Y);
			for (const p of lv3Result.Points.slice(0, 1)) {
				const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			}

			macroService.PollPattern(patterns.operationEdenAlliance.battle, { DoClick: true, PredicatePattern: patterns.battle.start });
			macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });
			macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.dailyOperations.mission.close });
			macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission.receiveAll, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.dailyOperations.mission.close });
			macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission.close, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });

			if (macroService.IsRunning) {
				daily.doOperationEdenAlliance.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}