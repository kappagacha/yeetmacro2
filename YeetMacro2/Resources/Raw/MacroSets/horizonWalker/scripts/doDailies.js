// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

goToLobby();

if (settings.doDailies.claimTribute.Value) {
    claimTribute();
    goToLobby();
}

if (settings.doDailies.doDailyShop.Value) {
    doDailyShop();
    goToLobby();
}

if (settings.doDailies.doStageWish.Value) {
    doStageWish();
    goToLobby();
}

if (settings.doDailies.doStageBounty.Value) {
    doStageBounty();
    goToLobby();
}

if (settings.doDailies.claimDailyRewards.Value) {
    claimDailyRewards();
    goToLobby();
}
