let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.freeQuests];
while (state.isRunning && !done) {
	const result = await macroService.pollPattern(loopPatterns);
	switch (result.path) {
		case 'titles.home':
			logger.info('doFreeQuests: click tab quest');
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doFreeQuests: click quest free quests');
			await macroService.clickPattern(patterns.quest.freeQuests);
			break;
		case 'titles.freeQuests':
			logger.info('doFreeQuests: target upgrade stone');
			const upgradeStoneTargetLevel = settings.freeQuests.upgradeStone.targetLevel.props.value || 'intermediate';
			if (upgradeStoneTargetLevel !== 'extreme') {
				await macroService.pollPattern(patterns.freeQuests.upgradeStone, { doClick: true, predicatePattern: patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel] });
				await macroService.pollPattern(patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel], { doClick: true, predicatePattern: patterns.tickets.add });
				// sample text capture: "25 x1" (it catches some of the words)
				let numTickets = (await screenService.getText(patterns.tickets.numTickets)).split('x')[1];
				while (numTickets < 2) {
					await macroService.clickPattern(patterns.tickets.add);
					await sleep(500);
					numTickets = (await screenService.getText(patterns.tickets.numTickets)).split('x')[1];
				}
				await macroService.pollPattern(patterns.tickets.use, { doClick: true, predicatePattern: patterns.tickets.prompt.ok });
				await macroService.pollPattern(patterns.tickets.prompt.ok, { doClick: true, predicatePattern: patterns.titles.freeQuests });
			}
			
			logger.info('doFreeQuests: skip all');
			await macroService.pollPattern(patterns.freeQuests.eris, { doClick: true, predicatePattern: patterns.skipAll.skipQuest });
			await macroService.pollPattern(patterns.skipAll.skipQuest, { doClick: true, predicatePattern: patterns.skipAll.button });
			const filterOffResult = await macroService.findPattern(patterns.skipAll.search.filter.off);
			if (filterOffResult.isSuccess) {
				await macroService.pollPattern(patterns.skipAll.search.filter.off, { doClick: true, predicatePattern: patterns.skipAll.search.filter });
				await sleep(500);
				const checkResult = await macroService.findPattern(patterns.skipAll.search.filter.check, { limit: 5 });
				for (let point of checkResult.points) {
					logger.debug(JSON.stringify(point));
					if (point.x < 300.0) continue;		// skip 4 stars
					screenService.doClick(point);
					await sleep(250);
				}
				await macroService.pollPattern(patterns.skipAll.search.filter.close, { doClick: true, predicatePattern: patterns.skipAll.title });
				await sleep(500);
			}

			let maxNumSkips = await screenService.getText(patterns.skipAll.maxNumSkips);
			while (maxNumSkips < 2) {
				await macroService.clickPattern(patterns.skipAll.addMaxSkips);
				await sleep(500);
				maxNumSkips = await screenService.getText(patterns.skipAll.maxNumSkips);
			}
			await macroService.pollPattern(patterns.skipAll.button, { doClick: true, predicatePattern: patterns.skipAll.prompt.ok });
			await macroService.pollPattern(patterns.skipAll.prompt.ok, { doClick: true, clickPattern: [patterns.skipAll.prompt.ok, patterns.skipAll.skipComplete], predicatePattern: patterns.skipAll.title });
			done = true;
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');