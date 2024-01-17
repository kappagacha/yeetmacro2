// @position=4
// Receive and present friend hearts
const loopPatterns = [patterns.lobby.level, patterns.titles.friends];
const daily = dailyManager.GetDaily();

const isLastRunWithinHour = (Date.now() - settings.doFriends.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.doFriends.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doFriends: click menu');
			macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.lobby.menu.friends });
			macroService.PollPattern(patterns.lobby.menu.friends, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.titles.friends });
			break;
		case 'titles.friends':
			logger.info('doFriends: recieve all and present');
			macroService.ClickPattern(patterns.friends.receiveAndPresent);

			if (macroService.IsRunning) {
				daily.doFriends.count.Count++;
				settings.doFriends.lastRun.Value = new Date().toISOString();
			}
			return;
	}
	sleep(1_000);
}