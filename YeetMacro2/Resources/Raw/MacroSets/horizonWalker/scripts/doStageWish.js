// do stage wish
const loopPatterns = [patterns.phone.battery, patterns.stage.title, patterns.stage.wish.title];
const daily = dailyManager.GetCurrentDaily();
const priority1 = settings.doStageWish.priority1.Value;
const priority2 = settings.doStageWish.priority2.Value;
const priority3 = ['credit', 'vangaurdExp', 'weaponExp'].find(wish => ![priority1, priority2].includes(wish));

if (daily.doStageWish.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'phone.battery':
			logger.info('doStageWish: click stage');
			macroService.ClickPattern(patterns.stage);
			break;
		case 'stage.title':
			logger.info('doStageWish: click wish');
			macroService.ClickPattern(patterns.stage.wish);
			break;
		case 'stage.wish.title':
			logger.info('doStageWish: do wish daily mission');

			const zeroWishesResult = macroService.FindPattern(patterns.stage.wish.zeroWishes);
			if (zeroWishesResult.IsSuccess) {
				macroService.IsRunning && (daily.doStageWish.done.IsChecked = true);
				return;
			}

			const wishTargets = [patterns.stage.wish[priority1], patterns.stage.wish[priority2], patterns.stage.wish[priority3]]
			const wishResult = macroService.PollPattern(wishTargets);
			const wishTarget = wishResult.Path.split('.').pop();
			macroService.PollPattern(patterns.stage.wish[wishTarget], { DoClick: true, ClickOffset: { X: -30 }, PredicatePattern: patterns.battle.deploy })
			macroService.PollPattern(patterns.battle.deploy, { DoClick: true, ClickPattern: patterns.battle.skip, PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: patterns.stage.wish.title });
			break;
	}
	sleep(1_000);
}