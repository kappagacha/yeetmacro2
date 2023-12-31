// Go to lobby
logger.info('goToLobby: start');

//claimJobs
// => patterns.tabs.home

//doExpertMainQuests
//doFreeQuests
// => patterns.skipAll.cancel => patterns.general.back => patterns.tabs.home

//doBattleArena
//doBattleArenaEx
// => patterns.tabs.home

//claimMissionRewards
// => patterns.general.close
//doFriends
// => patterns.friend.close

//doBranchQuests

//doEventSpecialQuests
// => patterns.quest.events.quest.skipAll.cancel

macroService.PollPattern(patterns.titles.home, {
	ClickPattern: [
		patterns.tabs.home,
		patterns.general.back,
		patterns.skipAll.cancel,
		patterns.general.close,
		patterns.friend.close,
		patterns.quest.events.quest.skipAll.cancel
	]
});
if (macroService.IsRunning) {
	logger.info('goToLobby: done');
} 