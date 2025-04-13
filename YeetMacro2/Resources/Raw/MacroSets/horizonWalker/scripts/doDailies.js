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

// claim tribute twice for sweepEvent
if (settings.doDailies.claimTribute.Value) {
    settings.claimTribute.lastRun.Value = date.toISOString();
    claimTribute();
    goToLobby();
}

if (settings.doDailies.sweepEvent.Value) {
    sweepEvent();
    goToLobby();
}