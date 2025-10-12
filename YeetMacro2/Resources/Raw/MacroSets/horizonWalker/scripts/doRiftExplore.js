// do rift explore
// @isFavorite
// @position=0

const loopPatterns = [patterns.phone.battery, patterns.stage.title, patterns.stage.riftExplore.title, patterns.stage.riftExplore.everchange.title];

const weekly = weeklyManager.GetCurrentWeekly();
const rifts = ['bloodPit', 'fire', 'frost', 'nightmare', 'everchange'];

if (weekly.doRiftExplore.done.IsChecked) {
	return "Script already completed. Uncheck done to override weekly flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, {});
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('doRiftExplore: click stage');
			macroService.ClickPattern(patterns.stage);
			break;
		case 'stage.title':
			logger.info('doRiftExplore: click rift explore');
			macroService.ClickPattern(patterns.stage.riftExplore);
			break;
		case 'stage.riftExplore.title':
			logger.info('doRiftExplore: do the rifts');

			for (let rift of rifts) {
				logger.info(`doRiftExplore: do ${rift} rift`);
				if (weekly.doRiftExplore[rift].IsChecked) continue;

				if (rift === 'everchange') {
					macroService.PollPattern(patterns.stage.riftExplore[rift], { DoClick: true, PredicatePattern: patterns.stage.riftExplore.floorList });
					break;
				}

				macroService.PollPattern(patterns.stage.riftExplore[rift], { DoClick: true, PredicatePattern: patterns.stage.riftExplore.floorList });
				const regionResult = macroService.FindPattern(patterns.stage.riftExplore.region, { Limit: 3 });
				const maxXPoint = regionResult.Points.reduce((maxXPoint, p) => (maxXPoint = maxXPoint.X >= p.X ? maxXPoint : p));
				macroService.PollPoint(maxXPoint, { PredicatePattern: patterns.battle.deploy });
				macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
				macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.concedeX });
				macroService.PollPattern(patterns.battle.concedeX, { DoClick: true, PredicatePattern: patterns.battle.next });
				macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.phone.battery });

				if (macroService.IsRunning) weekly.doRiftExplore[rift].IsChecked = true;

				break;
			}
			break;
		case 'stage.riftExplore.everchange.title':
			logger.info('doRiftExplore: do everchange rift');
			if (weekly.doRiftExplore.everchange.IsChecked) {
				return;
			}

			macroService.PollPattern(patterns.stage.riftExplore.region, { DoClick: true, PredicatePattern: patterns.battle.deploy });
			macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
			let nextResult = macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.nextFloor });
			while (macroService.IsRunning && nextResult.PredicatePattern === 'battle.nextFloor') {
				macroService.PollPattern(patterns.battle.nextFloor, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
				nextResult = macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: [patterns.battle.nextFloor, patterns.battle.complete] });
			}

			macroService.PollPattern(patterns.battle.complete, { DoClick: true, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.phone.battery });

			if (macroService.IsRunning) {
				weekly.doRiftExplore.everchange.IsChecked = true;
				weekly.doRiftExplore.done.IsChecked = true;
				return;
			}
			break;
	}
	sleep(1_000);
}
