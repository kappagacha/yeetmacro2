// @position=19
// Claim daily boss missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();

if (daily.claimBossDailyMissions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimBossDailyMissions: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimBossDailyMissions: claim rewards');
			const topLeft = macroService.GetTopLeft();
			const xLocation = topLeft.X + 300 + (resolution.Width - 1920) / 2.0;
			macroService.SwipePollPattern(patterns.event.dailyBossMission, { MaxSwipes: 3, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
			macroService.PollPattern(patterns.event.dailyBossMission, { DoClick: true, PredicatePattern: patterns.event.dailyBossMission.utc });

			const claimedFinalRewardResult = macroService.FindPattern(patterns.event.dailyBossMission.claimedFinalReward);
			if (claimedFinalRewardResult.IsSuccess && macroService.IsRunning) {
				daily.claimBossDailyMissions.done.IsChecked = true;
				return;
			}

			let bossRewardButtonResult = macroService.FindPattern([patterns.event.dailyBossMission.getFinalReward, patterns.event.dailyBossMission.move, patterns.event.dailyBossMission.getReward]);
			if (!bossRewardButtonResult.IsSuccess) throw new Error('Failed to find bossRewardButtonResult pattern');

			if (bossRewardButtonResult.Path === 'event.dailyBossMission.move') {
				macroService.PollPattern(patterns.event.dailyBossMission.move, { DoClick: true, PredicatePattern: patterns.general.back });
				if (isInEcologyStudy()) {
					goToLobby();
					refillStamina(70);
					goToLobby();
					doEcologyStudy();
					goToLobby();
					continue;
				} else if (isInIdentification()) {
					goToLobby();
					refillStamina(70);
					goToLobby();
					doIdentification();
					goToLobby();
					continue;
				}
				
				const moveResult = macroService.PollPattern([patterns.titles.inventory, patterns.titles.base, patterns.titles.base, patterns.titles.arena]);
				if (moveResult.Path === 'titles.inventory') {
					let gearEnhancedResult = macroService.FindPattern(patterns.inventory.gearEnhanced);
					if (gearEnhancedResult.IsSuccess) {
						macroService.PollPattern(patterns.inventory.filter, { DoClick: true, PredicatePattern: patterns.inventory.filter.apply });
						macroService.PollPattern(patterns.inventory.filter.excludeEquippedGear.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.excludeEquippedGear.enabled });
						macroService.PollPattern(patterns.inventory.filter.grade.normal.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.normal.enabled });
						macroService.PollPattern(patterns.inventory.filter.grade.superior.disabled, { DoClick: true, PredicatePattern: patterns.inventory.filter.grade.superior.enabled });
						macroService.PollPattern(patterns.inventory.filter.apply, { DoClick: true, PredicatePattern: patterns.titles.inventory });

						const starsPattern = macroService.ClonePattern(patterns.inventory.stars, {
							Width: resolution.Width - 720
						});
						const inventoryStarsResult = macroService.FindPattern(starsPattern, { Limit: 40 });
						for (const p of inventoryStarsResult.Points) {
							macroService.DoClick(p);
							sleep(1_000);
							gearEnhancedResult = macroService.FindPattern(patterns.inventory.gearEnhanced);
							if (!gearEnhancedResult.IsSuccess) break;
						}
					}
					macroService.PollPattern(patterns.inventory.enhance, { DoClick: true, PredicatePattern: patterns.titles.improveGear });
					let tenMaterialsResult = macroService.FindPattern(patterns.inventory.improveGear.tenMaterials);
					while (macroService.IsRunning && !tenMaterialsResult.IsSuccess) {
						macroService.ClickPattern([patterns.inventory.improveGear.apprenticeHammer, patterns.inventory.improveGear.apprenticeHammer.checked]);
						sleep(500);
						tenMaterialsResult = macroService.FindPattern(patterns.inventory.improveGear.tenMaterials);
					}
					macroService.PollPattern(patterns.inventory.improveGear.enhance.enhance.enabled, { DoClick: true, PredicatePattern: patterns.inventory.improveGear.enhance.enhance.disabled });
					goToLobby();
					continue;
				} else if (moveResult.Path === 'titles.base') {
					const date = new Date();
					date.setDate(date.getDate() - 1);
					// set date to yesterday for claimAntiparticle to run
					settings.claimAntiparticle.lastRun.Value = date.toISOString();
					claimAntiparticle();
					goToLobby();
					continue;
				} else if (moveResult.Path === 'titles.arena') {
					doArena();
					goToLobby();
					continue;
				}
			}

			while (bossRewardButtonResult.Path === 'event.dailyBossMission.getReward') {
				macroService.PollPattern(patterns.event.dailyBossMission.getReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.dailyBossMission.utc });

				bossRewardButtonResult = bossRewardButtonResult = macroService.FindPattern([patterns.event.dailyBossMission.move, patterns.event.dailyBossMission.getReward]);
			}

			macroService.PollPattern(patterns.event.dailyBossMission.getFinalReward, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.dailyBossMission.utc });

			if (macroService.IsRunning) {
				daily.claimBossDailyMissions.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

function isInEcologyStudy() {
	const ecologyStudyTypes = ['masterlessGuardian', 'tyrantToddler', 'unidentifiedChimera', 'sacreedGuardian', 'grandCalamari'];
	for (const ecologyStudy of ecologyStudyTypes) {
		if (macroService.FindPattern(patterns.challenge.ecologyStudy[ecologyStudy]).IsSuccess) {
			return true;
		}
	}
	return false;
}

function isInIdentification() {
	const identificationTypes = ['dekRilAndMekRil', 'glicys', 'blazingKnightMeteos', 'arsNova', 'amadeus'];
	for (const identification of identificationTypes) {
		if (macroService.FindPattern(patterns.challenge.identification[identification]).IsSuccess) {
			return true;
		}
	}
	return false;
}
