const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.battleArena, patterns.titles.party, patterns.battle.report];
let done = false;
while (state.isRunning && !done) {
	const loopResult = await macroService.pollPattern(loopPatterns);
	switch (loopResult.path) {
		case 'titles.home':
			logger.info('doBattleArenaEx: click tab quest');
			await macroService.clickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doBattleArenaEx: click battle arena');
			await macroService.clickPattern(patterns.quest.battleArena);
			break;
		case 'titles.battleArena':
			logger.info('doBattleArena: start arena');
			await macroService.pollPattern(patterns.battleArena.tabs.arenaEX, { doClick: true, predicatePattern: patterns.titles.battleArenaEX });
			await sleep(500);
			await macroService.pollPattern(patterns.battleArena.begin, { doClick: true, predicatePattern: patterns.battleArena.exRank });
			await sleep(500);
			await macroService.pollPattern(patterns.battleArena.exRank, { doClick: true, predicatePattern: patterns.battle.prepare });
			await sleep(500);
			await macroService.pollPattern(patterns.battle.prepare, { doClick: true, predicatePattern: patterns.titles.party });
			break;
		case 'titles.party':
			logger.info('doBattleArenaEx: select party');
			const scoreBonusPatterns = ['physicalDamage', 'lowRarity'].map(sb => patterns.battleArena.exScoreBonus[sb]);
			const scoreBonusResult = await macroService.pollPattern(scoreBonusPatterns);
			const scoreBonus = scoreBonusResult.path?.split('.').pop();
			logger.debug(`scoreBonus: ${scoreBonus}`);
			if (!scoreBonusResult.isSuccess) {
				result = 'Could not detect score bonus...';
				done = true;
				break;
			}
			const scoreBonusPartyName = settings.party.arenaEX[scoreBonus]?.props.value;
			if (!scoreBonusPartyName) {
				result = `Could not find scoreBonusPartyName for ${scoreBonusPartyName} in settings...`;
				done = true;
				break;
			}

			logger.debug(`scoreBonusPartyName: ${scoreBonusPartyName}`);
			if (!(await selectParty(scoreBonusPartyName))) {
				result = `scoreBonusPartyName not found: ${scoreBonusPartyName}`;
				done = true;
				break;
			}
			await sleep(500);
			await macroService.pollPattern(patterns.battle.begin, { doClick: true, clickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], predicatePattern: patterns.battle.report });
			break;
		case 'battle.report':
			logger.info('doBattleArenaEx: restart');
			// 1st battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 2nd battle => battle.next (middle) => patterns.battle.replay => patterns.battle.replay.ok
			// 3rd battle => battle.next (middle) => patterns.battle.replay.disabled

			await macroService.pollPattern(patterns.battle.next, { doClick: true, clickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], predicatePattern: [patterns.battle.replay, patterns.battle.next3] });
			await sleep(500);
			const replayResult = await macroService.findPattern(patterns.battle.replay);
			if (replayResult.isSuccess) {
				logger.debug('doBattleArenaEx: found replay');
				await macroService.pollPattern(patterns.battle.replay, { doClick: true, predicatePattern: patterns.battle.replay.ok });
				await macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			} else {
				logger.debug('doBattleArenaEx: found next3');
				const next3Result = await macroService.pollPattern(patterns.battle.next3, { doClick: true, clickPattern: patterns.battleArena.prompt.ok, predicatePattern: [patterns.titles.battleArena, patterns.battle.replay] });
				if (next3Result.predicatePath === 'titles.battleArena') {
					done = true;
					break;
				}
				await macroService.pollPattern(patterns.battle.replay, { doClick: true, predicatePattern: patterns.battle.replay.ok });
				await macroService.pollPattern(patterns.battle.replay.ok, { doClick: true, predicatePattern: patterns.battle.report });
			}
			break;
	}

	await sleep(1_000);
}
logger.info('Done...');