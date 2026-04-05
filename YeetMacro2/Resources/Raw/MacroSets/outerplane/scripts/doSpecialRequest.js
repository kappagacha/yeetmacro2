// @raw-script
// Auto or skip target special request (ecologyStudy or identification)

function doSpecialRequest(type, targetNumBattles = 0) {
	if (!type) type = settings.doSpecialRequest.type.Value;

	if (type === 'stage13') {
		doSpecialRequestStage13();
		return;
	}

	const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
	const daily = dailyManager.GetCurrentDaily();

	if (type !== 'ecologyStudy' && type !== 'identification') {
		return `Invalid type: ${type}. Expected 'ecologyStudy' or 'identification'.`;
	}

	const teamSlot = settings.doSpecialRequest[type].teamSlot.Value;
	const target = settings.doSpecialRequest[type].target.Value;
	const logPrefix = type;
	let currentStaminaValue;

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				currentStaminaValue = getCurrentStaminaValue();
				if (currentStaminaValue < 12) {
					return;
				}

				logger.info(`doSpecialRequest: click adventure tab for ${logPrefix}`);
				macroService.ClickPattern(patterns.tabs.adventure);
				sleep(500);
				break;
			case 'titles.adventure':
				logger.info(`doSpecialRequest: click challenge for ${logPrefix}`);
				macroService.ClickPattern(patterns.adventure.challenge);
				sleep(500);
				break;
			case 'titles.challenge':
				currentStaminaValue = getCurrentStaminaValue();
				if (currentStaminaValue < 12) {
					return;
				}

				logger.info(`doSpecialRequest: ${logPrefix} - ${target}`);
				macroService.PollPattern(patterns.challenge[type], { DoClick: true, PredicatePattern: patterns.challenge.enter });
				macroService.PollPattern(patterns.challenge[type][target].stars, { DoClick: true, PredicatePattern: patterns.challenge[patternPrefix][target] });
				macroService.PollPattern(patterns.challenge.enter, { DoClick: true, PredicatePattern: patterns.challenge.threeStars });
				clickBottomThreeStars();
				const teamsSetupResult = macroService.PollPattern(patterns.challenge.teamsSetup, { DoClick: true, PredicatePattern: [patterns.battle.enter, patterns.battle.restore] });
				if (teamsSetupResult.PredicatePath === 'battle.restore') {
					return;
				}
				selectTeamAndBattle(teamSlot, { targetNumBattles });

				if (macroService.IsRunning) daily.doSpecialRequest[type].count.Count++;
				return;
		}
		sleep(1_000);
	}

	function clickBottomThreeStars() {
		if (!macroService.IsRunning) return;

		const stage13Result = macroService.PollPattern(patterns.challenge.stage13, { TimeoutMs: 3_000 });
		const stage13Y = stage13Result.IsSuccess ? stage13Result.Point.Y - 50 : 1080;
		const threeStarsResult = macroService.FindPattern(patterns.challenge.threeStars, { Limit: 10 });
		const maxY = threeStarsResult.Points.reduce((maxY, p) => (maxY = p.Y > stage13Y || maxY > p.Y ? maxY : p.Y), 0);
		const bottomThreeStars = macroService.ClonePattern(patterns.challenge.threeStars, { CenterY: maxY, Height: 60.0 });
		const threeStarsSelected = macroService.ClonePattern(patterns.challenge.threeStars.selected, { CenterY: maxY });
		macroService.PollPattern(bottomThreeStars, { DoClick: true, PredicatePattern: threeStarsSelected });
	}
}

function doEcologyStudy(targetNumBattles = 0) {
	doSpecialRequest('ecologyStudy', targetNumBattles);
}

function doIdentification(targetNumBattles = 0) {
	doSpecialRequest('identification', targetNumBattles);
}

function doSpecialRequestStage13() {
	// @position=14
	// Skip all stage13s in special requests
	const loopPatterns = [patterns.lobby.level, patterns.challenge.sweepAll.title];
	const daily = dailyManager.GetCurrentDaily();
	const ecologyStudyKeyToBossType = {
		0: 'masterlessGuardian',
		1: 'tyrantToddler',
		2: 'unidentifiedChimera',
		3: 'sacreedGuardian',
		4: 'grandCalamari'
	};
	const identificationKeyToBossType = {
		0: 'dekRilAndMekRil',
		1: 'glicys',
		2: 'blazingKnightMeteos',
		3: 'arsNova',
		4: 'amadeus'
	};

	if (daily.doSpecialRequest.stage13.done.IsChecked) {
		return "Script already completed. Uncheck done to override daily flag.";
	}

	while (macroService.IsRunning) {
		const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
		switch (loopResult.Path) {
			case 'lobby.level':
				const currentStaminaValue = getCurrentStaminaValue();
				if (currentStaminaValue < 16) {
					return;
				}

				logger.info('sweepAll: go to sweepAll');
				macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.lobby.menu.sweepAll });
				macroService.PollPattern(patterns.lobby.menu.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
				break;
			case 'challenge.sweepAll.title':
				// Note: Each stage 13 costs 16 stamina
				// 2 runs because doSpecialRequest will do 1 run
				// 5 stages * 16 stamina * 6 runs = 480 * 2 (ecology study and identification) = 960 stamina

				if (!daily.doSpecialRequest.stage13.ecologyStudy.done.IsChecked) {
					logger.info('doSpecialRequestsStage13: doEcologyStudy');

					macroService.PollPattern(patterns.challenge.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
					macroService.PollPattern(patterns.challenge.sweepAll.ecologyStudy, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.ecologyStudy.selected });

					const done = sweepAllStage13('ecologyStudy', ecologyStudyKeyToBossType);

					if (macroService.IsRunning && done) {
						daily.doSpecialRequest.stage13.ecologyStudy.done.IsChecked = true;
					}
				}

				if (!daily.doSpecialRequest.stage13.identification.done.IsChecked) {
					logger.info('doSpecialRequestsStage13: doIdentification');

					macroService.PollPattern(patterns.challenge.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
					macroService.PollPattern(patterns.challenge.sweepAll.identification, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.identification.selected });

					const done = sweepAllStage13('identification', identificationKeyToBossType);

					if (macroService.IsRunning && done) {
						daily.doSpecialRequest.stage13.identification.done.IsChecked = true;
					}

					macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
				}

				if (macroService.IsRunning) {
					daily.doSpecialRequest.stage13.done.IsChecked = daily.doSpecialRequest.stage13.identification.done.IsChecked && daily.doSpecialRequest.stage13.ecologyStudy.done.IsChecked;
				}
				return;
		}
		sleep(1_000);
	}

	function sweepAllStage13(stageCategory, keyToBossType) {
		const doneTarget = daily.doSpecialRequest.stage13.doneTarget.IsChecked;
		macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage);
		let stage13AllResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage._13, { TimeoutMs: 1_000 });
		const maxStageRun = doneTarget ? 6 : 3;

		while (macroService.IsRunning && stage13AllResult.IsSuccess) {
			const staminaSlashResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.staminaSlash);
			const staminaPattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.currentStamina, { X: staminaSlashResult.Point.X - 69, OffsetCalcType: 'None', PathSuffix: `_${staminaSlashResult.Point.X}` });
			const staminaText = macroService.FindText(staminaPattern, '0123456789');
			const currentStamina = parseInt(staminaText);
			const maxRuns = parseInt(currentStamina / 16);

			logger.info(`doSpecialRequestsStage13: currentStamina=${currentStamina}, maxRuns=${maxRuns}, [${staminaText}]`);

			if (!maxRuns) return false;

			let numRuns = 0;
			let stageResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage, { Limit: 10 });
			const bossTypesChecked = [];

			for (let p of stageResult.Points.sort((a, b) => a.Y - b.Y)) {
				const stage13Pattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.stage._13, { CenterY: p.Y - 3, Height: 30, Padding: 10, PathSuffix: `_y${p.Y}` });
				const stagecheckPattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.stage.check, { CenterY: p.Y - 25, Padding: 5, PathSuffix: `_y${p.Y}` });
				const stage13Result = macroService.FindPattern(stage13Pattern);
				const index = pointToIndex(p);
				const bossType = keyToBossType[index];

				if (stage13Result.IsSuccess && numRuns < maxRuns && daily.doSpecialRequest.stage13[stageCategory][bossType].Count < maxStageRun) {
					macroService.PollPoint(p, { DoClick: true, PredicatePattern: stagecheckPattern });
					numRuns++;
					bossTypesChecked.push(bossType);
				} else {
					macroService.PollPoint(p, { DoClick: true, InversePredicatePattern: stagecheckPattern });
				}
			}

			// there are available runs but maxStageRun has been reached for all bossTypes
			if (!bossTypesChecked.length) {
				daily.doSpecialRequest.stage13[stageCategory].doneTarget.IsChecked = true;
				daily.doSpecialRequest.stage13.doneTarget.IsChecked = daily.doSpecialRequest.stage13.ecologyStudy.doneTarget.IsChecked && daily.doSpecialRequest.stage13.identification.doneTarget.IsChecked;
				return false;
			}

			macroService.PollPattern(patterns.challenge.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep.ok });
			macroService.PollPattern(patterns.challenge.sweepAll.sweep.ok, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep });

			for (const bossType of bossTypesChecked) {
				daily.doSpecialRequest.stage13[stageCategory][bossType].Count++;
			}

			sleep(1_000);
			macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage);
			stage13AllResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage._13, { TimeoutMs: 1_000 });
		}

		return !macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.stage._13, { TimeoutMs: 1_000 }).IsSuccess;
	}

	function pointToIndex(point) {
		// Midpoints between each row for boundary detection
		// Rows are at: ~285, ~381, ~477, ~573, ~669
		// Midpoints: 333, 429, 525, 621

		const y = point.Y;

		if (y < 333) return 0;      // Top row
		if (y < 429) return 1;      // Second row
		if (y < 525) return 2;      // Third row
		if (y < 621) return 3;      // Fourth row
		return 4;                   // Bottom row
	}
}