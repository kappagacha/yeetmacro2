// @position=10
// Claim friend beefs
const loopPatterns = [patterns.titles.home, patterns.titles.friendList];
const daily = dailyManager.GetDaily();
if (daily.doFriends.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.friend.greetingLog.close });
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doFriends: click friend');
			macroService.ClickPattern(patterns.friend);
			break;
		case 'titles.friendList':
			logger.info('doFriends: click greetAll');
			macroService.PollPattern(patterns.friend.greetAll, {
				DoClick: true,
				ClickPattern: [patterns.friend.greetingLog.close, patterns.friend.greetAll.greet, patterns.friend.greetAll.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp],
				PredicatePattern: patterns.friend.greetAll.disabled
			});
			if (macroService.IsRunning) {
				daily.doFriends.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}