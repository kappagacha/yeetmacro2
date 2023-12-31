// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

if (settings.doDailies.claimJobs.Value) {
    claimJobs();
    goToLobby();
}

if (settings.doDailies.doExpertMainQuests.Value) {
    doExpertMainQuests();
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