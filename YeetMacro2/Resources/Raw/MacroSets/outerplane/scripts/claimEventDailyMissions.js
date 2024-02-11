// Claim daily missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetDaily();
const dailyMissionPattern = macroService.ClonePattern(settings.claimEventDailyMissions.dailyMissionPattern.Value, {
	X: 90,
	Y: 200,
	Width: 400,
	Height: 800,
	Path: 'settings.claimEventDailyMissions.dailyMissionPattern',
	OffsetCalcType: 'Default'
});

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
			macroService.PollPattern(dailyMissionPattern, { DoClick: true, PredicatePattern: patterns.event.daily.info });
			macroService.PollPattern(patterns.event.daily.firstDone, { ClickPattern: [patterns.event.daily.firstNotification, patterns.event.ok] });

			const finalNotificationResult = macroService.FindPattern(patterns.event.daily.finalNotification);
			if (finalNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.daily.finalNotification, { DoClick: true, PredicatePattern: patterns.event.ok });
				macroService.PollPattern(patterns.event.ok, { DoClick: true, PredicatePattern: patterns.event.daily.info });
				const miniGameResult = macroService.PollPattern(patterns.event.rockPaperScissors);
				if (miniGameResult.Path === 'event.rockPaperScissors') {
					doRockPaperScissors();
				}

				if (macroService.IsRunning) {
					daily.claimEventDailyMissions.done.IsChecked = true;
				}
			}
			return;
	}
	sleep(1_000);
}

function doRockPaperScissors() {
	const rockPaperScissors = ['rock', 'paper', 'scissor'];
	macroService.PollPattern(patterns.event.rockPaperScissors, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
	sleep(1_000);
	let numCoins = macroService.GetText(patterns.event.rockPaperScissors.numCoins)?.replace(/[^0-9]/g, '');
	while (numCoins > 0) {
		const randomNumber = macroService.Random(0, 2);
		const rockPaperOrScissorPattern = patterns.event.rockPaperScissors[rockPaperScissors[randomNumber]];
		macroService.PollPattern(rockPaperOrScissorPattern, { DoClick: true, PredicatePattern: patterns.event.ok });
		macroService.PollPattern(patterns.event.ok, { DoClick: true, PredicatePattern: patterns.event.coinInfo });
		numCoins = macroService.GetText(patterns.event.rockPaperScissors.numCoins)?.replace(/[^0-9]/g, '');
	}
}