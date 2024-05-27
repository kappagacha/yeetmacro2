// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

if (settings.doDailies.watchAdQuartz.Value) {
    watchAdQuartz();
    goToLobby();
}

if (settings.doDailies.watchAdStamina.Value) {
    watchAdStamina();
    goToLobby();
}

if (settings.doDailies.claimJobs.Value) {
    claimJobs();
    goToLobby();
}

if (settings.doDailies.doMainExpertQuests.Value) {
    doMainExpertQuests();
    goToLobby();
}

if (settings.doDailies.doFreeQuests.Value) {
    doFreeQuests();
    goToLobby();
}

if (settings.doDailies.doBattleArena.Value) {
    doBattleArena();
    goToLobby();
}

if (settings.doDailies.doBattleArenaEx.Value) {
    doBattleArenaEx();
    goToLobby();
}

if (settings.doDailies.doMainOrEventHardQuests.Value) {
    doMainOrEventHardQuests();
    goToLobby();
}

if (settings.doDailies.doOneFameQuest.Value) {
    doOneFameQuest();
    goToLobby();
}

if (settings.doDailies.farmEventBossLoop.Value) {
    farmEventBossLoop();
    goToLobby();
}

if (settings.doDailies.claimMissionRewards.Value) {
    claimMissionRewards();
    goToLobby();
}

if (settings.doDailies.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doDailies.doBranchQuests.Value) {
    doBranchQuests();
    goToLobby();
}