// do stage wish
const loopPatterns = [patterns.lobby.stage, patterns.titles.stage];
const daily = dailyManager.GetCurrentDaily();

if (daily.doStageWish.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.stage':
			logger.info('doStageWish: click stage');
			macroService.ClickPattern(patterns.lobby.stage);
			break;
		case 'titles.stage':
			logger.info('doStageWish: do wish daily mission');
			macroService.PollPattern(patterns.stage.wish, { DoClick: true, PredicatePattern: patterns.stage.wish.selected });

			const zeroWishesResult = macroService.FindPattern(patterns.stage.wish.zeroWishes);
			if (zeroWishesResult.IsSuccess) {
				macroService.IsRunning && (daily.doStageWish.done.IsChecked = true);
				return;
			}

			const stageResult = macroService.FindPattern(patterns.stage.wish.stage, { Limit: 7 });
			const stageNames = stageResult.Points.filter(p => p).map(p => {
				const stageNamePattern = macroService.ClonePattern(patterns.stage.wish.stage.name, { CenterY: p.Y, OffsetCalcType: 'None', Path: `stage.wish.stage.name_x${p.X}_y${p.Y}` });
				return {
					point: { X: p.X, Y: p.Y },
					name: macroService.GetText(stageNamePattern)
				};
			});
			//stageNames.sort((a, b) => a.point.Y - b.point.Y);		// Y ascending
			stageNames.sort((a, b) => a.point.Y - b.point.Y);		// Y descending
			//const targetStage = stageNames.find(pn => pn.name.match(/corp|lab/gi));
			const targetStage = stageNames.find(pn => pn.name.match(/corp/gi)) || stageNames.find(pn => pn.name.match(/lab/gi));
			macroService.PollPoint(targetStage.point, { PredicatePattern: patterns.battle.deploy });
			macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.titles.stage });

	}
	sleep(1_000);
}