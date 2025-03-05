// @position=0
// Claim replenish your stamina
const popupPatterns = [patterns.lobby.expedition, patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.general.startMessageClose];
const loopPatterns = [patterns.lobby.level, patterns.event.close, ...popupPatterns];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const utcHour = new Date().getUTCHours();
const isStamina1 = utcHour < 12;

if ((isStamina1 && daily.claimReplenishYourStamina.done1.IsChecked) || (!isStamina1 && daily.claimReplenishYourStamina.done2.IsChecked)) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.expedition':
		case 'general.tapEmptySpace':
		case 'settings.goToLobby.userClickPattern':
		case 'general.exitCheckIn':
		case 'general.startMessageClose':
			goToLobby();
			break;
		case 'lobby.level':
			logger.info('claimReplenishYourStamina: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimReplenishYourStamina: claim rewards');
			const topLeft = macroService.GetTopLeft();
			const xLocation = topLeft.X + 300 + (resolution.Width - 1920) / 2.0;
			macroService.SwipePollPattern(patterns.event.replenishYourStamina, { MaxSwipes: 5, Start: { X: xLocation, Y: 800 }, End: { X: xLocation, Y: 280 } });
			macroService.PollPattern(patterns.event.replenishYourStamina, { DoClick: true, PredicatePattern: patterns.event.replenishYourStamina.utc });

			const notificationResult = macroService.PollPattern(patterns.event.replenishYourStamina.notification, { TimeoutMs: 3_000 });
			if (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.replenishYourStamina.notification, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.replenishYourStamina.utc });
			}

			//const claimed2Result = macroService.FindPattern(patterns.event.replenishYourStamina.claimed2);

			if (macroService.IsRunning) {
				daily.claimReplenishYourStamina[isStamina1 ? 'done1' : 'done2'].IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}