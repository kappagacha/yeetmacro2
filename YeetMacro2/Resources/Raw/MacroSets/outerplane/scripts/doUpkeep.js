// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

const daily = dailyManager.GetCurrentDaily();

settings.applyPreset.lastApplied.IsEnabled = true;
goToLobby();

if (settings.doUpkeep.claimGuildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doUpkeep.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doUpkeep.claimReplenishYourStamina.Value) {
    claimReplenishYourStamina();
    goToLobby();
}

if (settings.doUpkeep.startTerminusIsleExploration.Value) {
    startTerminusIsleExploration();
    goToLobby();
}

if (settings.doUpkeep.claimAntiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doUpkeep.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doUpkeep.claimArenaRewards.Value) {
    claimArenaRewards();
    goToLobby();
}

if (settings.doUpkeep.doSpecialRequestsStage13.Value && !daily.doSpecialRequestsStage13.done.IsChecked) {
    doSpecialRequestsStage13();
    goToLobby();
}

if (settings.doUpkeep.spendStaminaScript.IsEnabled) {
    globalThis[settings.doUpkeep.spendStaminaScript.Value]();
    goToLobby();
}

if (settings.doUpkeep.doTerminusIsleExploration.Value && !daily.doTerminusIsleExploration.done.IsChecked) {
    doTerminusIsleExploration();
    return;
}