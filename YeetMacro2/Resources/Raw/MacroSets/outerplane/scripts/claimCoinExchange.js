	// @position=17
// Claim daily event missions
const loopPatterns = [patterns.lobby.level, patterns.event.close];
const daily = dailyManager.GetCurrentDaily();
if (daily.claimCoinExchange.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('claimCoinExchange: click event');
			macroService.PollPattern(patterns.lobby.event, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.event.close });
			sleep(500);
			break;
		case 'event.close':
			logger.info('claimCoinExchange: claim rewards');
			macroService.PollPattern(patterns.event.coinExchangeShop, { SwipePattern: patterns.event.swipeDown });
			macroService.PollPattern(patterns.event.coinExchangeShop, { DoClick: true, PredicatePattern: patterns.event.coinExchangeShop.exchange, IntervalDelayMs: 3_000 });
			sleep(2_000);

			let notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
			while (notificationResult.IsSuccess) {
				macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
				sleep(1_000);
				macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.coinExchangeShop.getCoins });

				notificationResult = macroService.PollPattern(patterns.event.coinExchangeShop.exchange, { TimeoutMs: 3_000 });
			}
			
			if (macroService.IsRunning) {
				daily.claimCoinExchange.done.IsChecked = true;
			}

			return;
	}
	sleep(1_000);
}
