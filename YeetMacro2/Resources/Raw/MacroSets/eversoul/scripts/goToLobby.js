// Go to lobby
logger.info('goToLobby: start');

//claimLoot
//claimMail
//claimFreeShop
//claimFreeSummon
//claimChampsArena
//doFriends

//doPartTimeJobAndRest => patterns.town.cancel
//doArena
// => patterns.general.back
//doOutings => patterns.general.back => patterns.town.menu => patterns.town.returnToLobby
//doChampsArena
// => patterns.general.back

macroService.PollPattern(patterns.lobby.level, { ClickPattern: [patterns.general.back, patterns.town.cancel, patterns.town.menu, patterns.town.menu.returnToLobby] });
if (macroService.IsRunning) {
	logger.info('goToLobby: done');
}