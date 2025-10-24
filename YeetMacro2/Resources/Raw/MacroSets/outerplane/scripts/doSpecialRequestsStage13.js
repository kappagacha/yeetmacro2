// @position=14
// Skip all stage13s in special requests
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();

if (daily.doSpecialRequestsStage13.done.IsChecked) {
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

			logger.info('doSpecialRequestsStage13: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('doSpecialRequestsStage13: click challenge');
			macroService.ClickPattern(patterns.adventure.challenge);
			sleep(500);
			break;
		case 'titles.challenge':
			// Note: Each stage 13 costs 16 stamina
			// 2 runs because doSpecialRequest will do 1 run
			// 5 stages * 16 stamina * 2 runs = 160 * 2 (ecology study and identification) = 320 stamina

			if (!daily.doSpecialRequestsStage13.identification.IsChecked) {
				logger.info('doSpecialRequestsStage13: doIdentification');

				macroService.PollPattern(patterns.challenge.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
				macroService.PollPattern(patterns.challenge.sweepAll.identification, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.identification.selected });

				const done = sweepAllStage13();

				if (macroService.IsRunning && done) {
					daily.doSpecialRequestsStage13.identification.IsChecked = true;
				}

				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
			}

			if (!daily.doSpecialRequestsStage13.ecologyStudy.IsChecked) {
				logger.info('doSpecialRequestsStage13: doEcologyStudy');

				macroService.PollPattern(patterns.challenge.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.title });
				macroService.PollPattern(patterns.challenge.sweepAll.ecologyStudy, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.ecologyStudy.selected });

				const done = sweepAllStage13();

				if (macroService.IsRunning && done) {
					daily.doSpecialRequestsStage13.ecologyStudy.IsChecked = true;
				}
			}

			if (macroService.IsRunning) {
				daily.doSpecialRequestsStage13.done.IsChecked = daily.doSpecialRequestsStage13.identification.IsChecked && daily.doSpecialRequestsStage13.ecologyStudy.IsChecked;
			}
			return;
	}
	sleep(1_000);
}

function sweepAllStage13() {
	let stage13AllResult = macroService.FindPattern(patterns.challenge.sweepAll.specialRequest.stage._13);
	while (macroService.IsRunning && stage13AllResult.IsSuccess) {
		const staminaSlashResult = macroService.PollPattern(patterns.challenge.sweepAll.specialRequest.staminaSlash);
		const staminaPattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.currentStamina, { X: staminaSlashResult.Point.X - 71, OffsetCalcType: 'None', PathSuffix: `_${staminaSlashResult.Point.X}` });
		const staminaText = macroService.FindText(staminaPattern, '0123456789');
		const currentStamina = parseInt(staminaText);
		const maxRuns = parseInt(currentStamina / 16);

		logger.info(`doSpecialRequestsStage13: currentStamina=${currentStamina}, maxRuns=${maxRuns}, [${staminaText}]`);

		if (!maxRuns) return false;

		let numRuns = 0;
		let stageResult = macroService.FindPattern(patterns.challenge.sweepAll.specialRequest.stage, { Limit: 10 });

		for (let p of stageResult.Points.sort((a, b) => b.Y - a.Y)) {
			const stage13Pattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.stage._13, { CenterY: p.Y - 3, Height: 30, Padding: 10, PathSuffix: `_y${p.Y}` });
			const stagecheckPattern = macroService.ClonePattern(patterns.challenge.sweepAll.specialRequest.stage.check, { CenterY: p.Y - 25, Padding: 5, PathSuffix: `_y${p.Y}` });
			const stage13Result = macroService.FindPattern(stage13Pattern);

			if (stage13Result.IsSuccess && numRuns < maxRuns) {
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: stagecheckPattern });
				numRuns++;
			} else {
				macroService.PollPoint(p, { DoClick: true, InversePredicatePattern: stagecheckPattern });
			}
		}
		
		macroService.PollPattern(patterns.challenge.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep.ok });
		macroService.PollPattern(patterns.challenge.sweepAll.sweep.ok, { DoClick: true, PredicatePattern: patterns.challenge.sweepAll.sweep });
		sleep(1_000);
		stage13AllResult = macroService.FindPattern(patterns.challenge.sweepAll.specialRequest.stage._13);
	}

	return !macroService.FindPattern(patterns.challenge.sweepAll.specialRequest.stage._13).IsSuccess;
}
