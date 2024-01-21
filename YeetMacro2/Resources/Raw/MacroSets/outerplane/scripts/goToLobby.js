// @position=1000
// Go to lobby
logger.info('goToLobby: start');

//claimFreeShop
//claimFreeRecruit
//claimGuildBuff
//doArena
// => patterns.general.back
//doBountyHunter
//doBanditChase
//doUpgradeStoneRetrieval
//doSideStory1
//doSideStory2
//doGuildSecurityArea
// => (auto) patterns.battle.setup.enter.ok => patterns.battle.exit => patterns.general.back
// => (sweep) patterns.general.back
//watchAds
// => patterns.stamina.cancel
//doSpecialRequests

macroService.PollPattern(patterns.lobby.level, { ClickPattern: [patterns.general.back, patterns.battle.setup.enter.ok, patterns.battle.exit, patterns.stamina.cancel] });
if (macroService.IsRunning) {
	logger.info('goToLobby: done');
}