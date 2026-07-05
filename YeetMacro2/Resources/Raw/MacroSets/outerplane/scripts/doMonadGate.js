// @raw-script
// @tags=weeklies

function doMonadGate(type = '') {
	if (!type) type = settings.doMonadGate.type.Value;

	switch (type) {
		case 'normal':
			doNormalObservation();
			break;
		case 'dimensionalSingularity':
			doDimensionalSingularity();
			break;
	}
}

function doNormalObservation() {
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.monadGate.singularityRepel,
		patterns.monadGate.selectEntryRoute,
		patterns.monadGate.heroDeployment, patterns.monadGate.currentLocation, patterns.monadGate.nodes.heroDeployment,
		patterns.monadGate.relics.greenCard, patterns.monadGate.relics.redCard, patterns.monadGate.relics.blueCard,
		patterns.monadGate.event.options, patterns.monadGate.event.heroGrowth, patterns.monadGate.completed,
		patterns.event.story.notice.doNotShowStoryForADay, patterns.battle.enter];
	const clickPatterns = [patterns.adventure.doNotSeeFor3days, patterns.monadGate.gateEntryDevice, patterns.event.story.skip,
		patterns.monadGate.relics, patterns.monadGate.exit, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace,
		patterns.monadGate.next2
	];
	// 1 - right, 2 - top, 3 - bottom, 4 - left
	const lightTeam = ['demiurgeDrakhan', 'mysticSageAme', 'demiurgeLuna', 'monadEva'];

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doMonadGate: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doMonadGate: click monad gate');
				macroService.ClickPattern(patterns.adventure.monadGate);
				sleep(500);
				break;
			case 'monadGate.singularityRepel':
				logger.info('doDimensionalSingularity: click switch to normal observation mode');
				macroService.ClickPattern(patterns.monadGate.switch);
				sleep(500);
				break;
			case 'monadGate.selectEntryRoute':
				logger.info('doMonadGate: click select entry route select');
				macroService.ClickPattern(patterns.monadGate.selectEntryRoute.select);
				break;
			case 'monadGate.heroDeployment':
				macroService.PollPattern([patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.ok });
				macroService.PollPattern(patterns.monadGate.heroDeployment.filter.element.light, { DoClick: true, PredicatePattern: patterns.monadGate.heroDeployment.filter.element.light.selected });
				macroService.PollPattern(patterns.battle.characterFilter.ok, { DoClick: true, PredicatePattern: [patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied] });

				for (let character of lightTeam) {
					logger.info(`doMonadGate: selecting ${character}`);
					const ownedHeroesCloneOpts = { X: 60, Y: 180, Width: 640, Height: 740, PathSuffix: '_ownedHeroes', OffsetCalcType: 'None', BoundsCalcType: 'FillWidth' };
					const ownedHeroesPattern = macroService.ClonePattern(patterns.battle.character[character], ownedHeroesCloneOpts);
					const explorationHeroesCloneOpts = { X: 715, Y: 725, Width: 630, Height: 165, PathSuffix: '_exploration', OffsetCalcType: 'DockLeft' };
					const explorationHeroesPattern = macroService.ClonePattern(patterns.battle.character[character], explorationHeroesCloneOpts);

					macroService.PollPattern(ownedHeroesPattern, { DoClick: true, PredicatePattern: explorationHeroesPattern });
				}

				macroService.PollPattern(patterns.monadGate.heroDeployment.relicSettings, { DoClick: true, PredicatePattern: patterns.monadGate.heroDeployment.relicSettings.startExploration });
				macroService.PollPattern(patterns.monadGate.heroDeployment.relicSettings.startExploration, { DoClick: true, PredicatePattern: patterns.monadGate.heroDeployment.relicSettings.startExploration.ok });
				macroService.PollPattern(patterns.monadGate.heroDeployment.relicSettings.startExploration.ok, { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
				break;
			case 'monadGate.currentLocation':
				macroService.ClickPattern(patterns.monadGate.currentLocation);
				sleep(500);

				logger.info('doMonadGate: find next');
				const nextResult = macroService.PollPattern(patterns.monadGate.next);

				logger.info('doMonadGate: click on a node');
				const colors = ['green', 'red', 'blue', 'yellow'];

				colorLoop: for (let color of colors) {
					const colorResult = macroService.FindPattern(patterns.monadGate.nodes[color], { Limit: 10 });
					if (colorResult.IsSuccess) {
						for (let p of colorResult.Points) {
							if (p.X > nextResult.Point.X - 250) {
								macroService.DoClick(p.Offset(0, -30));
								break colorLoop;
							}
						}
					}
				}
				break;
			case 'event.story.notice.doNotShowStoryForADay':
				logger.info('doEventStory: handle do not show story for a day');
				macroService.PollPattern(patterns.event.story.notice.doNotShowStoryForADay.unselected, { DoClick: true, PredicatePattern: patterns.event.story.notice.doNotShowStoryForADay.selected });
				macroService.PollPattern(patterns.event.story.notice.ok, { DoClick: true, PredicatePattern: patterns.monadGate.exit });
				macroService.PollPattern(patterns.monadGate.exit, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.monadGate.currentLocation });
				break;
			case 'monadGate.relics.greenCard':
			case 'monadGate.relics.blueCard':
			case 'monadGate.relics.redCard':
				logger.info('doMonadGate: resolve green cards');
				const cards = [];
				const greenCardResult = macroService.FindPattern(patterns.monadGate.relics.greenCard, { Limit: 3 });
				const greenRelicTypeToPoints = {
					might: 100,
					criticalStrike: 200,
					resilience: 0,
					weaknessDetection: 300,
					fortitude: 20,
					effectiveness: 10,
					hardening: 30,
					acceleration: 200,
					analysis: 300,
					rupture: 210,
					generalSturdy1: 15,
					generalAccuracy1: 10,
					generalWeakPoint1: 300,
					generalReinforce1: 150,
					generalCriticalStrike1: 200,
					generalResistance1: 0,
				};

				if (greenCardResult.IsSuccess) {
					for (const p of greenCardResult.Points.sort((a, b) => a.X - b.X)) {
						const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
						const cardPattern = Object.keys(greenRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.greenCard[c], {
							PathSuffix: `_x${Math.trunc(p.X)}`,
							RawBounds: rect,
							OffsetCalcType: 'None',
						}));
						const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

						cards.push({ point: { X: p.X, Y: p.Y }, relic, points: greenRelicTypeToPoints[relic], color: 'green' });
					}
				}

				const blueCardResult = macroService.FindPattern(patterns.monadGate.relics.blueCard, { Limit: 3 });
				const blueRelicTypeToPoints = {
					fortuneFavorsTheBrave: 200,
					unstoppableMomentum: 100,
					finalBattle: 1000,
					generalEnhancement: 400,
					generalDestruction: 100,
					generalSturdy2: 50,
					generalCollapse: 100,
					generalHeavyStrike: 300,
					generalAnalysis2: 350,
					generalQuickAttack: 100,
					generalStrongArm2: 340,
					generalAccuracy2: 30,
					generalCriticalStrike2: 350,
					generalResistance2: 50,
					generalComboTechnique: 100,
					generalSelfSacrifice: 0,
					generalReinforce2: 100,
					generalWeakPoint2: 300,
					generalAcceleration2: 300,
					esephExploitWeaknessPetrified: 50,
					esephExploitWeaknessShocked: 55,
					esephStartShocked: 0,
					esephStartFrozen: 10,
					esephStrikePetrified: 150,
					esephStrikeShocked: 155,
					esephStrikeFrozen: 160,
					esephUltimateFrozen: 160,
					esephUltimatePetrified: 150,
					esephUltimateShocked: 155,
					esephDecisiveBlowShocked: 105,
					esephDecisiveBlowPetrified: 100,
					esephGnosisVielaExclusive: 0,
				};
				if (blueCardResult.IsSuccess) {
					for (const p of blueCardResult.Points.sort((a, b) => a.X - b.X)) {
						const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
						const cardPattern = Object.keys(blueRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.blueCard[c], {
							PathSuffix: `_x${Math.trunc(p.X)}`,
							RawBounds: rect,
							OffsetCalcType: 'None',
						}));
						const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

						cards.push({ point: { X: p.X, Y: p.Y }, relic, points: blueRelicTypeToPoints[relic], color: 'blue' });
					}
				}

				const redCardResult = macroService.FindPattern(patterns.monadGate.relics.redCard, { Limit: 3 });
				const redRelicTypeToPoints = {
					advent: 1_000,
					strategist: 10,
					asteiExclusiveRelic: 0,
					evaExclusiveRelic: 0,
					chainSystemLightII: -1_000,
					chainSystemLightIV: -1_000,
					reverseThinking: -100_000,
					protection: 400,
					esephDecisiveBlowPetrified: 400,
					esephDecisiveBlowFrozen: 420,
					esephStrikePetrified: 450,
					esephStrikeShocked: 460,
					esephStrikeFrozen: 470,
					esephUltimatePetrified: 450,
					esephUltimateShocked: 460,
					esephUltimateFrozen: 470,
					esephStartFrozen: 10,
					esephStartShocked: 20,
					esephExploitWeaknessPetrified: 300,
					esephExploitWeaknessFrozen: 320,
					esephGnosisVielaExclusive: 0,
				};
				if (redCardResult.IsSuccess) {
					for (const p of redCardResult.Points.sort((a, b) => a.X - b.X)) {
						const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
						const cardPattern = Object.keys(redRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.redCard[c], {
							PathSuffix: `_x${Math.trunc(p.X)}`,
							RawBounds: rect,
							OffsetCalcType: 'None',
						}));
						const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

						cards.push({ point: { X: p.X, Y: p.Y }, relic, points: redRelicTypeToPoints[relic], color: 'red' });
					}
				}

				const bestCard = cards.reduce((prev, current) => current.points >= prev.points ? current : prev);

				macroService.PollPoint(bestCard.point, { PredicatePattern: patterns.monadGate.relics.ok });
				macroService.PollPattern(patterns.monadGate.relics.ok, { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
				break;
			case 'monadGate.nodes.heroDeployment':
			case 'battle.enter':
				logger.info('doMonadGate: node hero deployment');
				macroService.PollPattern(patterns.monadGate.nodes.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
				macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.monadGate.next2, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace] });
				macroService.PollPattern([patterns.monadGate.next2, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace], { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
				break;
			case 'monadGate.event.heroGrowth':
				logger.info('doMonadGate: handle hero Growth');
				for (let character of lightTeam) {
					logger.info(`doMonadGate: selecting ${character}`);
					const heroGrowthCloneOpts = { X: 620, Y: 280, Width: 690, Height: 1080, PathSuffix: '_heroGrowth', OffsetCalcType: 'None', BoundsCalcType: 'FillWidth' };
					const heroGrowthPattern = macroService.ClonePattern(patterns.battle.character[character], heroGrowthCloneOpts);
					macroService.PollPattern(heroGrowthPattern, { DoClick: true, InversePredicatePattern: heroGrowthPattern });
				}
				macroService.PollPattern(patterns.monadGate.event.heroGrowth, { DoClick: true, PredicatePattern: patterns.monadGate.tapEmptySpace });
				macroService.PollPattern(patterns.monadGate.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
				break;
			case 'monadGate.event.options':
				logger.info('doMonadGate: randomly select option');
				sleep(1_000);
				const unselectedOptionsResult = macroService.FindPattern(patterns.monadGate.event.options, { Limit: 5 });
				let randomNumber = 0;
				let currentLocationResult = macroService.FindPattern([patterns.monadGate.currentLocation, patterns.monadGate.event.heroGrowth, patterns.battle.enter]);

				while (macroService.IsRunning && !currentLocationResult.IsSuccess) {
					randomNumber = macroService.Random(0, (unselectedOptionsResult.Points?.length ?? 0) + 1);
					if (randomNumber === 0) {
						macroService.ClickPattern(patterns.monadGate.event.selectedOption);
					} else {
						macroService.DoClick(unselectedOptionsResult.Points[randomNumber - 1]);
					}
					macroService.ClickPattern([patterns.monadGate.next2, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace]);
					sleep(200);
					currentLocationResult = macroService.FindPattern([patterns.monadGate.currentLocation, patterns.monadGate.event.heroGrowth, patterns.battle.enter]);
				}
				break;
			case 'monadGate.completed':
				macroService.PollPattern(patterns.monadGate.completed, { DoClick: true, PredicatePattern: patterns.monadGate.completed.endAfterClaimingRewards });
				macroService.PollPattern(patterns.monadGate.completed.endAfterClaimingRewards, { DoClick: true, PredicatePattern: patterns.monadGate.selectEntryRoute });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickPredicatePattern: patterns.monadGate.selectEntryRoute, PredicatePattern: patterns.monadGate.gateEntryDevice });

				let moveNotification = macroService.PollPattern(patterns.monadGate.move.notification, { TimeoutMs: 2_000 });
				if (moveNotification.IsSuccess) {
					macroService.PollPattern(patterns.monadGate.move, { DoClick: true, PredicatePattern: patterns.monadGate.move.receiveAll });
					macroService.PollPattern(patterns.monadGate.move.receiveAll, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.move.close });
					macroService.PollPattern(patterns.monadGate.move.receive, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.move.close });
					macroService.PollPattern(patterns.monadGate.move.close, { DoClick: true, PredicatePattern: patterns.monadGate.gateEntryDevice });
				}
				return;

		}
		sleep(1_000);
	}
}

function doDimensionalSingularity() {
	const daily = dailyManager.GetCurrentDaily();
	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.monadGate.gateEntryDevice, patterns.battle.enter, patterns.monadGate.singularityRepel.observation];
	const clickPatterns = [patterns.monadGate.singularityRepel, patterns.general.tapEmptySpace, patterns.monadGate.singularityRepel.teamsSetup];
	if (daily.doMonadGate.singularityRepel.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: clickPatterns });
		switch (loopResult.Path) {
			case 'lobby.level':
				logger.info('doDimensionalSingularity: click adventure tab');
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info('doDimensionalSingularity: click monad gate');
				macroService.ClickPattern(patterns.adventure.monadGate);
				sleep(500);
				break
			case 'monadGate.gateEntryDevice':
				logger.info('doDimensionalSingularity: click switch to dimensional singularity mode');
				macroService.ClickPattern(patterns.monadGate.switch);
				sleep(500);
				break;
			case 'battle.enter':
				logger.info('doDimensionalSingularity: do dimensional singularity');
				const bossTypes = ['harshna', 'ksai'].map(bt => patterns.monadGate.singularityRepel.bossTypes[bt]);
				const bossTypeResult = macroService.PollPattern(bossTypes);
				const bossType = bossTypeResult.Path?.split('.').pop();

				if (!FindPattern(patterns.monadGate.singularityRepel.currentRank.sssPlusPlus)) {
					throw Error("Need to try to beat the boss");
				}

				//const currentRanks = ['sssPlusPlus'].map(cr => patterns.monadGate.singularityRepel.currentRank[cr]);
				//const currentRankResult = macroService.PollPattern(currentRanks);
				//const currentRank = currentRankResult.Path?.split('.').pop();

				//if (currentRank !== 'sssPlusPlus') {
				//	throw Error("Need to try to beat the boss");
				//}

				//if (currentRank !== 'sssPlusPlus' && [].includes(bossType)) {
				//	throw Error("Need to try to beat the boss");
				//}

				if (macroService.FindPattern(patterns.monadGate.singularityRepel.zeroAttemptsLeft).IsSuccess)
					return;

				selectTeam(9);
				macroService.PollPattern(patterns.battle.lineupRecord, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord.restoreTeam });
				macroService.PollPattern(patterns.battle.lineupRecord.restoreTeam, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord.restoreTeam.ok });
				macroService.PollPattern(patterns.battle.lineupRecord.restoreTeam.ok, { DoClick: true, PredicatePattern: patterns.battle.lineupRecord });
				macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.saveAndExit });
				macroService.PollPattern(patterns.battle.saveAndExit, { DoClick: true, PredicatePattern: patterns.battle.saveAndExit.confirm });
				macroService.PollPattern(patterns.battle.saveAndExit.confirm, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.singularityRepel.teamsSetup });
				break;
			case 'monadGate.singularityRepel.observation':
				if (macroService.IsRunning) daily.doMonadGate.singularityRepel.IsChecked = true;
				return;
		}
		sleep(1_000);
	}
}
