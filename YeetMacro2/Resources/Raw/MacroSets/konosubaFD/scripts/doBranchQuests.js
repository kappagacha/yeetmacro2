let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.branchEvent, patterns.titles.branch, patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party];
//let event; // use this is more events have the prepare button

while (state.isRunning && !done) {
	const loopResult = await macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('doBranchQuests: check others notification');
			const othersNotificationResult = await macroService.findPattern(patterns.others.notification);
			if (othersNotificationResult.isSuccess) {
				await macroService.pollPattern(patterns.others.notification, { doClick: true, predicatePattern: patterns.others.branch.notification });
				await macroService.pollPattern(patterns.others.branch.notification, { doClick: true, predicatePattern: patterns.titles.branch });
			}
			else {
				done = true;
			}
			break;
		case 'titles.branch':
			logger.info('doBranchQuests: pick quest');
			const eventResult = await macroService.pollPattern([patterns.branchEvent.cabbageHunting, patterns.branchEvent.explosionWalk], { doClick: true, predicatePattern: patterns.titles.branchEvent });
			//event = eventResult.path;
			break;
		case 'titles.branchEvent':
			logger.info('doBranchQuests: start');
			await macroService.pollPattern([patterns.branchEvent.prepare, patterns.branchEvent.start], { doClick: true, predicatePattern: [patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party] });
			break;
		case 'titles.branchEvent.explosionWalk.chant.disabled':
			logger.info('doBranchQuests: explosion walk');
			const optionNumber = 1 + Math.floor(Math.random() * 4);
			logger.debug('option ' + optionNumber);
			await macroService.pollPattern(patterns.branchEvent.explosionWalk['option' + optionNumber], { doClick: true, predicatePattern: patterns.branchEvent.explosionWalk.chant.enabled });
			await macroService.pollPattern(patterns.branchEvent.explosionWalk.chant.enabled, { doClick: true, clickPattern: [patterns.battle.next, patterns.branchEvent.skip], predicatePattern: patterns.titles.home });
			break;
		case 'titles.party':
			logger.info('doBranchQuests: select party');
			const targetPartyName = settings.party.cabbageHunting.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				await selectPartyByRecommendedElement(isBossMulti ? -425 : 0);	// Recommended Element icons are shifted by 425 to the left of expected location
			}
			else {
				if (!(await selectParty(targetPartyName))) {
					result = `targetPartyName not found: ${targetPartyName}`;
					done = true;
					break;
				}
			}
			await sleep(500);
			await macroService.pollPattern(patterns.battle.begin, { doClick: true, predicatePattern: patterns.battle.report });
			await macroService.pollPattern(patterns.battle.next, { doClick: true, clickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], predicatePattern: patterns.titles.home });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');