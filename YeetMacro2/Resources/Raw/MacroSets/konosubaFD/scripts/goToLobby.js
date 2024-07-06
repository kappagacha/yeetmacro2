// @position=999
// Go to lobby
logger.info('goToLobby: start');

//claimJobs
// => patterns.tabs.home

//doMainExpertQuests
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

//_watchAdStamina
// => patterns.general.close[trialOfTheAncients]

//doOneFameQuest
// => patterns.battle.next => patterns.battle.next2

macroService.PollPattern(patterns.titles.home, {
	ClickPattern: [
		patterns.tabs.home,
		patterns.general.back,
		patterns.skipAll.cancel,
		patterns.general.close,
		patterns.friend.close,
		patterns.quest.events.quest.skipAll.cancel,
		patterns.ad.cancel,
		patterns.battle.next,
		patterns.battle.next2,
		patterns.quest.main.skipAllAtOnce.cancel,
		patterns.general.cancel
	]
});

if (macroService.IsRunning) {
	logger.info('goToLobby: done');
} 