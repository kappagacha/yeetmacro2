let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.smithy, patterns.titles.craft, patterns.skipAll.title];
const offset = macroService.calcOffset(patterns.titles.home);

const farmMat = async (targetMats, staminaCost, numSkips) => {
	macroService.pollPattern(patterns.skipAll.material, { doClick: true, predicatePattern: patterns.skipAll.search });
	sleep(500);
	const filterOffResult = macroService.findPattern(patterns.skipAll.search.filter.off);
	if (filterOffResult.isSuccess) {
		macroService.pollPattern(patterns.skipAll.search.filter.off, { doClick: true, predicatePattern: patterns.skipAll.search.filter });
		sleep(500);
		const checkResult = macroService.findPattern(patterns.skipAll.search.filter.check, { limit: 4 });
		for (let point of checkResult.points) {
			if (point.x < offset.x + 400.0) continue;		// skip 4 stars
			screenService.doClick(point);
			sleep(250);
		}
		macroService.pollPattern(patterns.skipAll.search.filter.close, { doClick: true, predicatePattern: patterns.skipAll.search });
		sleep(500);
	}

	macroService.pollPattern(patterns.skipAll.search.select.check, { doClick: true, inversePredicatePattern: patterns.skipAll.search.select.check });
	for (const mat of targetMats) {
		const matResult = macroService.findPattern(mat);
		if (matResult.isSuccess) {
			const matCheckPattern = {
				...patterns.skipAll.search.select.check,
				props: {
					...patterns.skipAll.search.select.check.props,
					path: patterns.skipAll.search.select.check.props.path + '_' + mat.props.path,
					patterns: patterns.skipAll.search.select.check.props.patterns.map(p => ({
						...p,
						rect: {
							x: matResult.point.x - 110.0,
							y: matResult.point.y - 100.0,
							width: 100.0,
							height: 75.0
						},
						offsetCalcType: "None"
					})),
				}
			};
			logger.debug(JSON.stringify(matCheckPattern, null, 2));
			macroService.pollPattern(mat, { doClick: true, predicatePattern: matCheckPattern });
		}
	}
	sleep(500);
	macroService.pollPattern(patterns.skipAll.search.button, { doClick: true, predicatePattern: patterns.skipAll.title });
	sleep(2000);

	const currentStaminaCost = screenService.getText(patterns.skipAll.totalCost);
	if (currentStaminaCost < staminaCost) {
		macroService.pollPattern(patterns.skipAll.addStamina, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina });
		macroService.pollPattern(patterns.stamina.meat, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina2 });
		let targetStamina = screenService.getText(patterns.stamina.target);
		while (state.isRunning() && targetStamina < staminaCost) {
			macroService.clickPattern(patterns.stamina.plusOne);
			sleep(500);
			targetStamina = screenService.getText(patterns.stamina.target);
		}
		macroService.pollPattern(patterns.stamina.prompt.recover, { doClick: true, clickPattern: patterns.stamina.prompt.ok, predicatePattern: patterns.skipAll.addMaxSkips, intervalDelayMs: 1_000 });
	}

	let maxNumSkips = screenService.getText(patterns.skipAll.maxNumSkips);
	while (state.isRunning() && maxNumSkips < numSkips) {
		macroService.clickPattern(patterns.skipAll.addMaxSkips);
		sleep(500);
		maxNumSkips = screenService.getText(patterns.skipAll.maxNumSkips);
	}

	macroService.pollPattern(patterns.skipAll.button, { doClick: true, predicatePattern: patterns.skipAll.prompt.ok });
	sleep(1_000);
	macroService.pollPattern(patterns.skipAll.prompt.ok, { doClick: true, predicatePattern: patterns.skipAll.skipComplete });
	macroService.pollPattern(patterns.skipAll.skipComplete, { doClick: true, clickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.skipAll.title });
};

while (state.isRunning() && !done) {
	const result = macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('farmMats: click smithy tab');
			macroService.clickPattern(patterns.tabs.smithy);
			break;
		case 'titles.smithy':
			logger.info('farmMats: click craft');
			macroService.clickPattern(patterns.smithy.craft);
			break;
		case 'titles.craft':
			macroService.pollPattern(patterns.smithy.craft.jewelry, { doClick: true, predicatePattern: patterns.smithy.craft.jewelry.list });
			sleep(500);
			macroService.pollPattern(patterns.smithy.craft.materials.archAngelFeather, { doClick: true, predicatePattern: patterns.smithy.prompt.howToAcquire });
			sleep(1_000);
			macroService.pollPattern(patterns.smithy.prompt.skipAll, { doClick: true, predicatePattern: patterns.skipAll.title });
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