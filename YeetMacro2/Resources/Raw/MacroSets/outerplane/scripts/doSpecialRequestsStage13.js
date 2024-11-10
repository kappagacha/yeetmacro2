// @position=14
// Skip all stage13s in special requests
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.challenge];
const daily = dailyManager.GetCurrentDaily();

if (daily.doSpecialRequestsStage13.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.arena.defendReport.close });
	switch (loopResult.Path) {
		case 'lobby.level':
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
				macroService.PollPattern(patterns.challenge.identification, { DoClick: true, PredicatePattern: patterns.challenge.enter });
				const done = sweepAllStage13();

				if (macroService.IsRunning && done) {
					daily.doSpecialRequestsStage13.identification.IsChecked = true;
				}

				goToLobby();
			}

			if (!daily.doSpecialRequestsStage13.ecologyStudy.IsChecked) {
				logger.info('doSpecialRequestsStage13: doEcologyStudy');
				macroService.PollPattern(patterns.challenge.ecologyStudy, { DoClick: true, PredicatePattern: patterns.challenge.enter });
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
	macroService.PollPattern(patterns.challenge.specialRequest.sweepAll, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });

	const stage13AllPattern = macroService.ClonePattern(patterns.challenge.specialRequest.stage._13, { Height: 600, Path: 'patterns.challenge.specialRequest.stage._13_all' });
	let stage13AllResult = macroService.FindPattern(stage13AllPattern);
	while (stage13AllResult.IsSuccess) {
		const currentStamina = parseInt(macroService.GetText(patterns.challenge.specialRequest.currentStamina));
		const maxRuns = parseInt(currentStamina / 16);

		logger.info(`doSpecialRequestsStage13: currentStamina=${currentStamina}, maxRuns=${maxRuns}`);

		if (maxRuns === 0) return false;

		let numRuns = 0;
		let stageResult = macroService.FindPattern(patterns.challenge.specialRequest.stage, { Limit: 10 });

		for (let p of stageResult.Points) {
			const stage13Pattern = macroService.ClonePattern(patterns.challenge.specialRequest.stage._13, { CenterY: p.Y - 3, Padding: 10, Path: `patterns.challenge.specialRequest.stage._13_y${p.Y}` });
			const stagecheckPattern = macroService.ClonePattern(patterns.challenge.specialRequest.stage.check, { CenterY: p.Y, Padding: 10, Path: `patterns.challenge.specialRequest.stage.check_y${p.Y}` });
			const stage13Result = macroService.FindPattern(stage13Pattern);

			if (stage13Result.IsSuccess && numRuns < maxRuns) {
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: stagecheckPattern });
				numRuns++;
			} else {
				macroService.PollPoint(p, { DoClick: true, InversePredicatePattern: stagecheckPattern });
			}
		}
		
		macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.sweep, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.ok });
		macroService.PollPattern(patterns.challenge.specialRequest.sweepAll.ok, { DoClick: true, PredicatePattern: patterns.challenge.specialRequest.sweepAll.sweep });
		sleep(1_000);
		stage13AllResult = macroService.FindPattern(stage13AllPattern);
	}

	return !macroService.FindPattern(stage13AllPattern).IsSuccess;
}
