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

macroService.PollPattern(patterns.lobby.level, {
	ClickPattern: [
		patterns.general.back,
		patterns.town.cancel,
		patterns.town.menu,
		patterns.town.menu.returnToLobby,
		patterns.battle.exit,
		patterns.login.pressToStart,
		patterns.login.confirm,
		patterns.login.pressToStart2,
		patterns.login.skip,
		patterns.login.newCharacter,
		patterns.iridescentScenicInstance.expeditionDispatch,
	]
});
if (macroService.IsRunning) {
	logger.info('goToLobby: done');
}