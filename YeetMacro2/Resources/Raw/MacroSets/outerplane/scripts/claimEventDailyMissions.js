// @position=17
// Claim daily event missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
//const dailyMissionPattern = macroService.ClonePattern(settings.claimEventDailyMissions.dailyMissionPattern.Value, {
//	X: 90,
//	Y: 200,
//	Width: 400,
//	Height: 800,
//	Path: 'settings.claimEventDailyMissions.dailyMissionPattern',
//	OffsetCalcType: 'Default'
//});

if (daily.claimEventDailyMissions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimEventDailyMissions: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimEventDailyMissions: claim rewards');
			//macroService.PollPattern(dailyMissionPattern, { DoClick: true, PredicatePattern: patterns.event.daily.info, IntervalDelayMs: 3_000 });
			const topLeft = macroService.GetTopLeft();
			const xLocation = topLeft.X + 300 + (resolution.Width - 1920) / 2.0;
			macroService.SwipePollPattern(patterns.event.daily, { MaxSwipes: 3, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
			macroService.PollPattern(patterns.event.daily, { DoClick: true, PredicatePattern: patterns.event.daily.info, IntervalDelayMs: 3_000 });
			sleep(2_000);

			let notificationResult = macroService.PollPattern(patterns.event.daily.anniversary.notification, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.daily.anniversary.notification, { DoClick: true, PredicatePattern: [patterns.event.ok, patterns.event.confirm], ClickOffset: { X: -40, Y: 40 } });
				macroService.PollPattern([patterns.event.ok, patterns.event.confirm], { DoClick: true, PredicatePattern: patterns.event.daily.info });
				notificationResult = macroService.PollPattern(patterns.event.daily.anniversary.notification, { TimeoutMs: 3_000 });
			}
			
			const miniGameResult = macroService.PollPattern([patterns.event.rockPaperScissors, patterns.event.drawACapsule, patterns.event.spinTheWheel]);
			if (miniGameResult.Path === 'event.rockPaperScissors') {
				doRockPaperScissors();
			} else if (miniGameResult.Path === 'event.drawACapsule') {
				doDrawACapsule();
			} else if (miniGameResult.Path === 'event.spinTheWheel') {
				doSpinTheWheel();
			}

			if (macroService.IsRunning) {
				daily.claimEventDailyMissions.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}

function doRockPaperScissors() {
	const rockPaperScissors = ['rock', 'paper', 'scissor'];
	macroService.PollPattern(patterns.event.rockPaperScissors, { DoClick: true, PredicatePattern: patterns.event.coinsOwned });
	sleep(2_000);
	let numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	while (macroService.IsRunning && !numCoins) {
		sleep(200);
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}

	while (macroService.IsRunning && numCoins > 0) {
		const randomNumber = macroService.Random(0, 2);
		const rockPaperOrScissorPattern = patterns.event.rockPaperScissors[rockPaperScissors[randomNumber]];
		macroService.PollPattern(rockPaperOrScissorPattern, { DoClick: true, PredicatePattern: patterns.event.ok });
		macroService.PollPattern(patterns.event.ok, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}
}

function doDrawACapsule() {
	macroService.PollPattern(patterns.event.drawACapsule, { DoClick: true, PredicatePattern: patterns.event.coinsOwned });
	sleep(2_000);
	let numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	while (macroService.IsRunning && !numCoins) {
		sleep(200);
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}

	while (macroService.IsRunning && numCoins > 0) {
		macroService.PollPattern(patterns.event.drawACapsule.draw, { DoClick: true, PredicatePattern: patterns.event.confirm });
		macroService.PollPattern(patterns.event.confirm, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}
}

function doSpinTheWheel() {
	macroService.PollPattern(patterns.event.spinTheWheel, { DoClick: true, PredicatePattern: patterns.event.coinsOwned });
	sleep(2_000);
	let numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	while (macroService.IsRunning && !numCoins) {
		sleep(200);
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}

	while (macroService.IsRunning && numCoins > 0) {
		macroService.PollPattern(patterns.event.spinTheWheel.start, { DoClick: true, PredicatePattern: patterns.event.confirm });
		macroService.PollPattern(patterns.event.confirm, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
		numCoins = macroService.GetText(patterns.event.numCoins)?.replace(/[^0-9]/g, '');
	}
}