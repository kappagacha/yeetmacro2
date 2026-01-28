// @tags=weeklies
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.monadGate.selectEntryRoute,
	patterns.monadGate.heroDeployment, patterns.monadGate.currentLocation, patterns.monadGate.nodes.heroDeployment,
	patterns.monadGate.relics.greenCard, patterns.monadGate.relics.redCard, patterns.monadGate.relics.blueCard,
	patterns.monadGate.event.selectedOption, patterns.monadGate.completed];
const clickPatterns = [patterns.adventure.doNotSeeFor3days, patterns.monadGate.gateEntryDevice, patterns.event.story.skip,
	patterns.monadGate.relics, patterns.monadGate.exit, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace];

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
		case 'monadGate.selectEntryRoute':
			logger.info('doMonadGate: click select entry route select');
			macroService.ClickPattern(patterns.monadGate.selectEntryRoute.select);
			break;
		case 'monadGate.heroDeployment':
			macroService.PollPattern([patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied], { DoClick: true, PredicatePattern: patterns.battle.characterFilter.ok });
			macroService.PollPattern(patterns.monadGate.heroDeployment.filter.element.light, { DoClick: true, PredicatePattern: patterns.monadGate.heroDeployment.filter.element.light.selected });
			macroService.PollPattern(patterns.battle.characterFilter.ok, { DoClick: true, PredicatePattern: [patterns.monadGate.heroDeployment.filter, patterns.monadGate.heroDeployment.filter.applied] });

			// 1 - right, 2 - top, 3 - bottom, 4 - left
			const lightTeam = ['demiurgeDrakhan', 'mysticSageAme', 'demiurgeLuna', 'monadEva'];
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
			logger.info('doMonadGate: click on a node');
			macroService.ClickPattern(patterns.monadGate.currentLocation);
			sleep(1_000);
			const colors = ['green', 'red', 'blue', 'yellow'];
			for (let color of colors) {
				const colorResult = macroService.FindPattern(patterns.monadGate.nodes[color]);
				if (colorResult.IsSuccess) {
					macroService.ClickPattern(patterns.monadGate.nodes[color], { ClickOffset: { Y: -20 } });
					break;
				}
			}
			break;
		case 'event.story.notice.doNotShowStoryForADay':
			logger.info('doEventStory: handle do not show story for a day');
			macroService.PollPattern(patterns.event.story.notice.doNotShowStoryForADay.unselected, { DoClick: true, PredicatePattern: patterns.event.story.notice.doNotShowStoryForADay.selected });
			macroService.PollPattern(patterns.event.story.notice.ok, { DoClick: true, PredicatePattern: patterns.monadGate.exit });
			macroService.PollPattern(patterns.monadGate.exit, { DoClick: true, ClickPattern: patterns.general.tapEmptySpace, PredicatePattern: patterns.monadGate.currentLocation });
			break;
		case 'monadGate.newBonds':
			logger.info('doMonadGate: new bonds');
			macroService.PollPattern(patterns.monadGate.next2, { DoClick: true, PredicatePattern: patterns.monadGate.tapEmptySpace });
			macroService.PollPattern(patterns.monadGate.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });


			break;
		case 'monadGate.relics.greenCard':
		case 'monadGate.relics.blueCard':
		case 'monadGate.relics.redCard':
			logger.info('doMonadGate: resolve green cards');
			const cards = [];
			const greenCardResult = macroService.FindPattern(patterns.monadGate.relics.greenCard, { Limit: 3 });
			const greenRelicTypeToPoints = {
				might: 100,				// Lv.1 Increase Attack by 15%
				criticalStrike: 200,	// Lv.1 Increase Critical damage by 30%
				resilience: 0,			// Lv.1 Increase Resilience by 50%
				weaknessDetection: 300,	// Lv.1 Increase Critical hit by 15/30%
				fortitude: 20,			// Lv.1 Increase Health by 30%
				effectiveness: 10,		// Lv.1 Increase Effectiveness by 50%
				hardening: 30,			// Lv.1 Increase Defence by 50%
				acceleration: 200,		// Lv.1 Increase Speed by 10%
				analysis: 0,			// Lv.1 Increase Accuracy by 50%
			};

			if (greenCardResult.IsSuccess) {
				for (const p of greenCardResult.Points.sort((a, b) => a.X - b.X)) {
					const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
					const cardPattern = Object.keys(greenRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.greenCard[c], {
						PathSuffix: `_x${Math.trunc(p.X)}`,
						RawBounds: rect,
					}));
					const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

					cards.push({ point: { X: p.X, Y: p.Y }, relic, points: greenRelicTypeToPoints[relic], color: 'green' });
				}
			}

			const blueCardResult = macroService.FindPattern(patterns.monadGate.relics.blueCard, { Limit: 3 });
			const blueRelicTypeToPoints = {
				fortuneFavorsTheBrave: 200,			// Lv.1 Slightly increase damage proportional to the number of granted buffs
				unstoppableMomentum: 100,			// Lv.1 Slightly increase damage proportional to the number of granted debuffs
				finalBattle: 1000,					// Lv.1 At the start of the turn, has a low chance to reset Caster's cooldown
			};
			if (blueCardResult.IsSuccess) {
				for (const p of blueCardResult.Points.sort((a, b) => a.X - b.X)) {
					const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
					const cardPattern = Object.keys(blueRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.blueCard[c], {
						PathSuffix: `_x${Math.trunc(p.X)}`,
						RawBounds: rect,
					}));
					const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

					cards.push({ point: { X: p.X, Y: p.Y }, relic, points: blueRelicTypeToPoints[relic], color: 'blue' });
				}
			}

			const redCardResult = macroService.FindPattern(patterns.monadGate.relics.redCard, { Limit: 3 });
			const redRelicTypeToPoints = {
				advent: 1000,				// Lv.2 On death has a 30% chance to revive
				strategist: 10,				// Lv.1 Always take Elemental Advantage when attacking, but reduces Damage by 50%
				asteiExclusiveRelic: 0,		// Lv.1 At the start of the turn, reecover the Caster's Health by 20% and Action Points by 20
				chainSystemLightIV: -1_000,	// Lv.1 Reduce Light Speed by 50%, increase Dark Damage by 100%
				reverseThinking: -100_000,		// Lv.1 Increase Damage to the target with Non-Advantageous Element by 100% reduces Damage to the target with Advantageous Element by 100%

			};
			if (redCardResult.IsSuccess) {
				for (const p of redCardResult.Points.sort((a, b) => a.X - b.X)) {
					const rect = { X: p.X - 180, Y: p.Y + 10, Width: 360, Height: 70 };
					const cardPattern = Object.keys(redRelicTypeToPoints).map(c => macroService.ClonePattern(patterns.monadGate.relics.redCard[c], {
						PathSuffix: `_x${Math.trunc(p.X)}`,
						RawBounds: rect,
					}));
					const relic = macroService.PollPattern(cardPattern).Path?.split('.').pop()?.split('_')[0];

					cards.push({ point: { X: p.X, Y: p.Y }, relic, points: redRelicTypeToPoints[relic], color: 'red' });
				}
			}

			// if equal, prefer cards to the right
			const bestCard = cards.reduce((prev, current) => current.points >= prev.points ? current : prev);

			macroService.PollPoint(bestCard.point, { PredicatePattern: patterns.monadGate.relics.ok });
			macroService.PollPattern(patterns.monadGate.relics.ok, { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
			break;
		case 'monadGate.nodes.heroDeployment':
			logger.info('doMonadGate: node hero deployment');
			macroService.PollPattern(patterns.monadGate.nodes.heroDeployment, { DoClick: true, PredicatePattern: patterns.battle.enter });
			macroService.PollPattern(patterns.battle.enter, { DoClick: true, PredicatePattern: patterns.battle.exit });
			macroService.PollPattern(patterns.battle.exit, { DoClick: true, PredicatePattern: [patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace] });
			macroService.PollPattern([patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace], { DoClick: true, PredicatePattern: patterns.monadGate.currentLocation });
			break;
		case 'monadGate.event.selectedOption':
			logger.info('doMonadGate: randomly select option');
			sleep(1_000);
			const unselectedOptionsResult = macroService.FindPattern(patterns.monadGate.event.options, { Limit: 5 });
			let randomNumber = 0;
			let currentLocationResult = macroService.FindPattern(patterns.monadGate.currentLocation);

			while (!currentLocationResult.IsSuccess) {
				randomNumber = macroService.Random(0, (unselectedOptionsResult.Points?.length ?? 0) + 1);
				if (randomNumber === 0) {
					macroService.ClickPattern(patterns.monadGate.event.selectedOption);
				} else {
					macroService.DoClick(unselectedOptionsResult.Points[randomNumber - 1]);
				}
				macroService.ClickPattern([patterns.monadGate.next2, patterns.monadGate.tapEmptySpace, patterns.general.tapEmptySpace]);
				sleep(200);
				currentLocationResult = macroService.FindPattern(patterns.monadGate.currentLocation);
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
				macroService.PollPattern(patterns.monadGate.move.closee, { DoClick: true, PredicatePattern: patterns.monadGate.gateEntryDevice });
			}
			return;

	}
	sleep(1_000);
}


