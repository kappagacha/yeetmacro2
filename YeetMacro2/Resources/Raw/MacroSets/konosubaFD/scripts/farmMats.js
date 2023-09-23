let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.smithy, patterns.titles.craft, patterns.skipAll.title];
const offset = macroService.CalcOffset(patterns.titles.home);

const farmMat = async (targetMats, staminaCost, numSkips) => {
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
			const matCheckPattern = {
				...patterns.skipAll.search.select.check,
				props: {
					...patterns.skipAll.search.select.check.props,
					path: patterns.skipAll.search.select.check.props.path + '_' + mat.props.path,
					patterns: patterns.skipAll.search.select.check.props.patterns.map(p => ({
						...p,
						rect: {
							x: matResult.Point.X - 115.0,
							y: matResult.Point.Y - 105.0,
							width: 110.0,
							height: 85.0
						},
						offsetCalcType: "None"
					})),
				}
			};
			//logger.debug(JSON.stringify(matCheckPattern, null, 2));
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

while (macroService.IsRunning && !done) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('farmMats: click smithy tab');
			macroService.ClickPattern(patterns.tabs.smithy);
			break;
		case 'titles.smithy':
			logger.info('farmMats: click craft');
			macroService.ClickPattern(patterns.smithy.craft);
			break;
		case 'titles.craft':
			macroService.PollPattern(patterns.smithy.craft.jewelry, { DoClick: true, PredicatePattern: patterns.smithy.craft.jewelry.list });
			sleep(500);
			macroService.PollPattern(patterns.smithy.craft.materials.archAngelFeather, { DoClick: true, PredicatePattern: patterns.smithy.prompt.howToAcquire });
			sleep(1_000);
			macroService.PollPattern(patterns.smithy.prompt.skipAll, { DoClick: true, PredicatePattern: patterns.skipAll.title });
			sleep(500);
			break;
		case 'skipAll.title':
			logger.info('farmMats: farm extreme levels');
			farmMat([patterns.skipAll.search.select.mithrilOre, patterns.skipAll.search.select.yggdrasilBranch, patterns.skipAll.search.select.platinumOre], 500, 1);
			sleep(1_000);
			logger.info('farmMats: farm skyDragonScale');
			farmMat([patterns.skipAll.search.select.skyDragonScale], 500, 3);
			done = true;
			break;
	}

	sleep(1_000);
}
logger.info('Done...');