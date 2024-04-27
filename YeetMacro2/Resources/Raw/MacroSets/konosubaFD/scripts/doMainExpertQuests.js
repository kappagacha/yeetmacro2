// @position=2
// Target farm materials to farm all expert main quests
const loopPatterns = [patterns.titles.home, patterns.titles.smithy, patterns.titles.craft, patterns.skipAll.title];
const offset = macroService.GetTopLeft();
const minStamina = settings.doMainExpertQuests.minStamina.Value;
const daily = dailyManager.GetCurrentDaily();
if (daily.doMainExpertQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('doMainExpertQuests: click smithy tab');
			macroService.ClickPattern(patterns.tabs.smithy);
			break;
		case 'titles.smithy':
			logger.info('doMainExpertQuests: click craft');
			macroService.ClickPattern(patterns.smithy.craft);
			break;
		case 'titles.craft':
			macroService.PollPattern(patterns.smithy.craft.jewelry, { DoClick: true, PredicatePattern: patterns.smithy.craft.jewelry.list });
			sleep(500);
			macroService.PollPattern(patterns.smithy.craft.materials.archAngelFeather, { DoClick: true, ClickPattern: [patterns.smithy.craft.ascendingRarity, patterns.smithy.craft.jewelry.chaosBrooch], PredicatePattern: patterns.smithy.prompt.howToAcquire });
			sleep(1_000);
			macroService.PollPattern(patterns.smithy.prompt.skipAll, { DoClick: true, PredicatePattern: patterns.skipAll.title });
			sleep(500);
			break;
		case 'skipAll.title':
			logger.info('doMainExpertQuests: farm extreme levels');
			farmMat([patterns.skipAll.search.select.mithrilOre, patterns.skipAll.search.select.yggdrasilBranch, patterns.skipAll.search.select.platinumOre], minStamina, 1);
			// sky dragon scale is obsolete when compared to the brooches (hard to come by though)
			//sleep(1_000);
			//logger.info('farmMats: farm skyDragonScale');
			//farmMat([patterns.skipAll.search.select.skyDragonScale], 500, 3);
			if (macroService.IsRunning) {
				daily.doMainExpertQuests.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}

function farmMat(targetMats, staminaCost, numSkips) {
	macroService.PollPattern(patterns.skipAll.material, { DoClick: true, PredicatePattern: patterns.skipAll.search });
	sleep(500);
	const filterOffResult = macroService.FindPattern(patterns.skipAll.search.filter.off);
	if (filterOffResult.IsSuccess) {
		macroService.PollPattern(patterns.skipAll.search.filter.off, { DoClick: true, PredicatePattern: patterns.skipAll.search.filter });
		sleep(500);
		const checkResult = macroService.FindPattern(patterns.skipAll.search.filter.check, { Limit: 4 });
		for (let point of checkResult.Points) {
			if (point.X < offset.X + 400.0) continue;		// skip 4 stars
			macroService.DoClick(point);
			sleep(250);
		}
		macroService.PollPattern(patterns.skipAll.search.filter.close, { DoClick: true, PredicatePattern: patterns.skipAll.search });
		sleep(500);
	}

	macroService.PollPattern(patterns.skipAll.search.select.check, { DoClick: true, InversePredicatePattern: patterns.skipAll.search.select.check });
	for (const mat of targetMats) {
		const matResult = macroService.FindPattern(mat);
		if (matResult.IsSuccess) {
			const matCheckPattern = macroService.ClonePattern(patterns.skipAll.search.select.check, {
				Path: `skipAll.search.select.check_${mat.Path}`,
				CenterX: matResult.Point.X - 60,
				CenterY: matResult.Point.Y - 60,
				Width: 110,
				Height: 85,
				OffsetCalcType: "None"
			});
			macroService.PollPattern(mat, { DoClick: true, PredicatePattern: matCheckPattern, IntervalDelayMs: 1_000 });
		}
	}
	sleep(500);
	macroService.PollPattern(patterns.skipAll.search.button, { DoClick: true, PredicatePattern: patterns.skipAll.title });
	sleep(2000);

	const currentStaminaCost = macroService.GetText(patterns.skipAll.totalCost);
	if (currentStaminaCost < staminaCost) {
		macroService.PollPattern(patterns.skipAll.addStamina, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina });
		macroService.PollPattern(patterns.stamina.meat, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina2 });
		let targetStamina = macroService.GetText(patterns.stamina.target);
		while (macroService.IsRunning && targetStamina < staminaCost) {
			macroService.ClickPattern(patterns.stamina.plusOne);
			sleep(500);
			targetStamina = macroService.GetText(patterns.stamina.target);
		}
		macroService.PollPattern(patterns.stamina.prompt.recover, { DoClick: true, ClickPattern: patterns.stamina.prompt.ok, PredicatePattern: patterns.skipAll.addMaxSkips, IntervalDelayMs: 1_000 });
	}

	let maxNumSkips = macroService.GetText(patterns.skipAll.maxNumSkips);
	while (macroService.IsRunning && maxNumSkips < numSkips) {
		macroService.ClickPattern(patterns.skipAll.addMaxSkips);
		sleep(500);
		maxNumSkips = macroService.GetText(patterns.skipAll.maxNumSkips);
	}

	macroService.PollPattern(patterns.skipAll.button, { DoClick: true, PredicatePattern: patterns.skipAll.prompt.ok });
	sleep(1_000);
	macroService.PollPattern(patterns.skipAll.prompt.ok, { DoClick: true, PredicatePattern: patterns.skipAll.skipComplete });
	macroService.PollPattern(patterns.skipAll.skipComplete, { DoClick: true, ClickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.skipAll.title });
};
