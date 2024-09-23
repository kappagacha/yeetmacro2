// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

goToLobby();

if (settings.doUpkeep.claimJobs.Value) {
    claimJobs();
    goToLobby();
}

if (settings.doUpkeep.useFriendBeefs.Value) {
    useFriendBeefs();
    goToLobby();
}

if (settings.doUpkeep.skipEventOrMainHardQuests.Value) {
    skipEventOrMainHardQuests();
    goToLobby();
}