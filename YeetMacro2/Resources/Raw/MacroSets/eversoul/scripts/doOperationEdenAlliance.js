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
			const operationEdenAllianceResult = macroService.PollPattern(patterns.adventure.tabs.specialMissions, { DoClick: true, PredicatePattern: [patterns.operationEdenAlliance, patterns.operationEdenAlliance.disabled] });
			if (macroService.IsRunning && operationEdenAllianceResult.PredicatePath === 'operationEdenAlliance.disabled') {
				daily.doOperationEdenAlliance.done.IsChecked = true;
				return;
			}
			macroService.PollPattern(patterns.operationEdenAlliance, { DoClick: true, PredicatePattern: patterns.titles.operationEdenAlliance });
			break;
		case 'titles.operationEdenAlliance':
			macroService.PollPattern(patterns.operationEdenAlliance.qualifiers, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });
			macroService.ClickPattern(patterns.operationEdenAlliance.selectMax);
			sleep(500);
			macroService.ClickPattern(patterns.operationEdenAlliance.selectMax);
			//macroService.PollPattern(patterns.operationEdenAlliance.selectMax, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.dailyOperations.disadvantages.check });
			macroService.PollPattern(patterns.operationEdenAlliance.battle, { DoClick: true, PredicatePattern: patterns.battle.start });
			macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.continue });
			macroService.PollPattern(patterns.battle.continue, { DoClick: true, ClickPattern: patterns.general.tapTheScreen, PredicatePattern: patterns.titles.operationEdenAlliance });

			//macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });
			//const lv1Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv1, { Limit: 3 });
			//const lvl1SortedPoints = lv1Result.Points.sort((a, b) => a.Y - b.Y);
			//for (const p of lvl1SortedPoints) {
			//	const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
			//	macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			//}

			//const lv2Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv2, { Limit: 3 });
			//const lvl2SortedPoints = lv2Result.Points.sort((a, b) => a.Y - b.Y);
			//for (const p of lvl2SortedPoints.slice(0, 2)) {
			//	const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
			//	macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			//}

			//const lv3Result = macroService.FindPattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.lv3, { Limit: 3 });
			//const lvl3SortedPoints = lv3Result.Points.sort((a, b) => a.Y - b.Y);
			//for (const p of lvl3SortedPoints.slice(0, 1)) {
			//	const checkPattern = macroService.ClonePattern(patterns.operationEdenAlliance.dailyOperations.disadvantages.check, { CenterX: p.X + 48, CenterY: p.Y + 38, Padding: 15, OffsetCalcType: 'None' })
			//	macroService.PollPoint(p, { DoClick: true, PredicatePattern: checkPattern });
			//}

			//macroService.PollPattern(patterns.operationEdenAlliance.battle, { DoClick: true, PredicatePattern: patterns.battle.start });
			//macroService.PollPattern(patterns.battle.start, { DoClick: true, PredicatePattern: patterns.battle.exit });
			//macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });
			//macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.dailyOperations.mission.close });
			//macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission.receiveAll, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
			//macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.dailyOperations.mission.close });
			//macroService.PollPattern(patterns.operationEdenAlliance.dailyOperations.mission.close, { DoClick: true, PredicatePattern: patterns.operationEdenAlliance.battle });

			if (macroService.IsRunning) {
				daily.doOperationEdenAlliance.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}