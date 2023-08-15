let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.smithy, patterns.titles.craft, patterns.skipAll.title];
const offset = macroService.calcOffset(patterns.titles.home);

const farmMat = async (targetMats, staminaCost, numSkips) => {
	await macroService.pollPattern(patterns.skipAll.material, { doClick: true, predicatePattern: patterns.skipAll.search });
	await sleep(500);
	const filterOffResult = await macroService.findPattern(patterns.skipAll.search.filter.off);
	if (filterOffResult.isSuccess) {
		await macroService.pollPattern(patterns.skipAll.search.filter.off, { doClick: true, predicatePattern: patterns.skipAll.search.filter });
		await sleep(500);
		const checkResult = await macroService.findPattern(patterns.skipAll.search.filter.check, { limit: 4 });
		for (let point of checkResult.points) {
			if (point.x < offset.x + 400.0) continue;		// skip 4 stars
			screenService.doClick(point);
			await sleep(250);
		}
		await macroService.pollPattern(patterns.skipAll.search.filter.close, { doClick: true, predicatePattern: patterns.skipAll.search });
		await sleep(500);
	}

	await macroService.pollPattern(patterns.skipAll.search.select.check, { doClick: true, inversePredicatePattern: patterns.skipAll.search.select.check });
	for (const mat of targetMats) {
		const matResult = await macroService.findPattern(mat);
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
			await macroService.pollPattern(mat, { doClick: true, predicatePattern: matCheckPattern });
		}
	}
	await sleep(500);
	await macroService.pollPattern(patterns.skipAll.search.button, { doClick: true, predicatePattern: patterns.skipAll.title });
	await sleep(2000);

	const currentStaminaCost = await screenService.getText(patterns.skipAll.totalCost);
	if (currentStaminaCost < staminaCost) {
		await macroService.pollPattern(patterns.skipAll.addStamina, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina });
		await macroService.pollPattern(patterns.stamina.meat, { doClick: true, predicatePattern: patterns.stamina.prompt.recoverStamina2 });
		let targetStamina = await screenService.getText(patterns.stamina.target);
		while (state.isRunning && targetStamina < staminaCost) {
			await macroService.clickPattern(patterns.stamina.plusOne);
			await sleep(500);
			targetStamina = await screenService.getText(patterns.stamina.target);
		}
		await macroService.pollPattern(patterns.stamina.prompt.recover, { doClick: true, clickPattern: patterns.stamina.prompt.ok, predicatePattern: patterns.skipAll.addMaxSkips, intervalDelayMs: 1_000 });
	}

	let maxNumSkips = await screenService.getText(patterns.skipAll.maxNumSkips);
	while (state.isRunning && maxNumSkips < numSkips) {
		await macroService.clickPattern(patterns.skipAll.addMaxSkips);
		await sleep(500);
		maxNumSkips = await screenService.getText(patterns.skipAll.maxNumSkips);
	}

	await macroService.pollPattern(patterns.skipAll.button, { doClick: true, predicatePattern: patterns.skipAll.prompt.ok });
	await sleep(1_000);
	await macroService.pollPattern(patterns.skipAll.prompt.ok, { doClick: true, predicatePattern: patterns.skipAll.skipComplete });
	await macroService.pollPattern(patterns.skipAll.skipComplete, { doClick: true, clickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], predicatePattern: patterns.skipAll.title });
};

while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('farmMats: click smithy tab');
			await macroService.clickPattern(patterns.tabs.smithy);
			break;
		case 'titles.smithy':
			logger.info('farmMats: click craft');
			await macroService.clickPattern(patterns.smithy.craft);
			break;
		case 'titles.craft':
			await macroService.pollPattern(patterns.smithy.craft.jewelry, { doClick: true, predicatePattern: patterns.smithy.craft.jewelry.list });
			await sleep(500);
			await macroService.pollPattern(patterns.smithy.craft.materials.archAngelFeather, { doClick: true, predicatePattern: patterns.smithy.prompt.howToAcquire });
			await sleep(1_000);
			await macroService.pollPattern(patterns.smithy.prompt.skipAll, { doClick: true, predicatePattern: patterns.skipAll.title });
			await sleep(500);
			break;
		case 'skipAll.title':
			logger.info('farmMats: farm extreme levels');
			await farmMat([patterns.skipAll.search.select.mithrilOre, patterns.skipAll.search.select.yggdrasilBranch, patterns.skipAll.search.select.platinumOre], 500, 1);
			await sleep(1_000);
			logger.info('farmMats: farm skyDragonScale');
			await farmMat([patterns.skipAll.search.select.skyDragonScale], 500, 3);
			done = true;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');