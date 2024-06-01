// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

if (settings.doUpkeep.claimJobs.Value) {
    claimJobs();
    goToLobby();
}

if (settings.doUpkeep.skipEventQuests.Value) {
    skipEventQuests();
    goToLobby();
}