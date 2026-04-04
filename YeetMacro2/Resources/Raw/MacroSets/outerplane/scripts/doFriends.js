// @position=4
// Receive and present friend hearts
const popupPatterns = [patterns.general.tapEmptySpace, settings.goToLobby.userClickPattern.Value, patterns.general.exitCheckIn, patterns.lobby.popup.doNotShowAgainToday];
const loopPatterns = [patterns.lobby.level, patterns.titles.friends, ...popupPatterns];
const daily = dailyManager.GetCurrentDaily();

const isLastRunWithinHour = (Date.now() - settings.doFriends.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.doFriends.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.friends.ok });
	switch (loopResult.Path) {
		case 'general.tapEmptySpace':
		case 'settings.goToLobby.userClickPattern':
		case 'general.exitCheckIn':
		case 'lobby.popup.doNotShowAgainToday':
			goToLobby();
			break;
		case 'lobby.level':
			logger.info('doFriends: click menu');
			const lobbyMenuResult = macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.lobby.menu.friends, TimeoutMs: 2_500 });
			if (!lobbyMenuResult.IsSuccess) {
				macroService.PollPattern(patterns.lobby.menu.close, { DoClick: true, InversePredicatePattern: patterns.lobby.menu.close });
				continue;
			}

			macroService.PollPattern(patterns.lobby.menu.friends, { DoClick: true, ClickOffset: { Y: -30 }, PredicatePattern: patterns.titles.friends });
			break;
		case 'titles.friends':
			logger.info('doFriends: recieve all and present');
			macroService.PollPattern(patterns.friends.friendList, { DoClick: true, PredicatePattern: patterns.friends.friendList.selected });
			macroService.ClickPattern(patterns.friends.receiveAndPresent);

			if (macroService.IsRunning) {
				daily.doFriends.count.Count++;
				settings.doFriends.lastRun.Value = new Date().toISOString();
			}
			return;
	}
	sleep(1_000);
}

function manageFriends() {
	macroService.PollPattern(patterns.friends.friendList, { DoClick: true, PredicatePattern: patterns.friends.friendList.selected });
	macroService.PollPattern(patterns.friends.friendList.sortByLastLogin, { DoClick: true, PredicatePattern: patterns.friends.friendList.sortByLastLogin.desc });

	const daysResult = macroService.FindPattern(patterns.friends.friendList.day, { Limit: 5 });
	if (daysResult.IsSuccess) {
		for (const p of daysResult.Points.sort((a, b) => a.Y - b.Y)) {
			const dayBounds = { X: p.X - 27, Y: p.Y - 15, Width: 30, Height: 30 };
			const numDays = parseInt(macroService.FindTextWithBounds(dayBounds, "1234567890").slice(0, -1));

			if (numDays > 2) {
				macroService.PollPoint(p, { DoClick: true, PredicatePattern: patterns.friends.friendList.delete });
				macroService.PollPattern(patterns.friends.friendList.delete, { DoClick: true, PredicatePattern: patterns.friends.friendList.delete.ok });
				macroService.PollPattern(patterns.friends.friendList.delete.ok, { DoClick: true, PredicatePattern: patterns.friends.friendList.delete });
				macroService.PollPattern(patterns.friends.friendList.delete.cancel, { DoClick: true, PredicatePattern: patterns.titles.friends });
			};
		}
	}

	let numFriends = parseInt(macroService.FindText(patterns.friends.numFriends).split('/')[0]);
	if (numFriends < 50) {
		macroService.PollPattern(patterns.friends.sentRequests, { DoClick: true, PredicatePattern: patterns.friends.sentRequests.selected });

		let cancelRequestResult = macroService.PollPattern(patterns.friends.sentRequests.cancelRequest, { TimeoutMs: 2_000 });
		while (macroService.IsRunning && cancelRequestResult.IsSuccess) {
			macroService.ClickPattern(patterns.friends.sentRequests.cancelRequest);
			cancelRequestResult = macroService.PollPattern(patterns.friends.sentRequests.cancelRequest, { TimeoutMs: 2_000 });
		}

		macroService.PollPattern(patterns.friends.receivedRequests, { DoClick: true, PredicatePattern: patterns.friends.receivedRequests.selected });
		let approveRequestResult = macroService.PollPattern(patterns.friends.receivedRequests.approve, { TimeoutMs: 2_000 });
		while (macroService.IsRunning && approveRequestResult.IsSuccess) {
			macroService.ClickPattern(patterns.friends.receivedRequests.approve);
			approveRequestResult = macroService.PollPattern(patterns.friends.receivedRequests.approve, { TimeoutMs: 2_000 });
		}

		macroService.PollPattern(patterns.friends.findFriend, { DoClick: true, PredicatePattern: patterns.friends.findFriend.selected });
		let numFriendRequest = 0;
		const requestFriendResult = macroService.FindPattern(patterns.friends.findFriend.requestFriend, { Limit: 5 });
		if (requestFriendResult.IsSuccess) {
			for (const p of requestFriendResult.Points) {
				const noticeOkResult = macroService.FindPattern(patterns.friends.findFriend.noticeOk);
				if (noticeOkResult.IsSuccess) {
					numFriendRequest--;
					macroService.PollPattern(patterns.friends.findFriend.noticeOk, { DoClick: true, InversePredicatePattern: patterns.friends.findFriend.noticeOk });
				}

				if (numFriendRequest > (50 - numFriends)) break;
				macroService.DoClick(p);
				sleep(100);
				macroService.DoClick(p);
				numFriendRequest++;
				sleep(1_000);
			}
		}
	}
	
}