const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
let done = false;
while (state.isRunning && !done) {
	const loopResult = await macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('doBattleArena: click tab quest');
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doBattleArena: click battle arena');
			await macroService.clickPattern(patterns.quest.battleArena);
			break;
		case 'titles.battleArena':
			logger.info('doBattleArena: start arena');
			await macroService.pollPattern(patterns.battleArena.begin, { doClick: true, predicatePattern: patterns.battleArena.advanced });
			await sleep(500);
			await macroService.pollPattern(patterns.battleArena.advanced, { doClick: true, predicatePattern: patterns.battle.prepare });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			
			break;
		case 'titles.party':
			logger.info('doBattleArena: select party');
			const targetPartyName = settings.party.battleArena.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				await selectPartyByRecommendedElement();
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
			break;
		case 'battle.report':
			logger.info('doBattleArena: restart');
			await macroService.pollPattern(patterns.battle.next, { doClick: true, clickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank, patterns.battle.next3], predicatePattern: patterns.battleArena.prompt.ok });
			await sleep(500);
			const replayResult = await macroService.pollPattern(patterns.battleArena.prompt.ok, { doClick: true, clickPattern: patterns.battle.replay, predicatePattern: [patterns.battle.replay.prompt, patterns.battle.replay.disabled] });
			if (replayResult.predicatePath === 'battle.replay.disabled') {
				done = true;
				break;
			}
			await sleep(500);
			await macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');