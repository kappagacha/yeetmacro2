// @isFavorite
// @position=-1
// Do upkeep (all checked scripts)

if (settings.doUpkeep.claimLoot.Value) {
    claimLoot();
    goToLobby();
}

if (settings.doUpkeep.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doUpkeep.doPartTimeJobAndRest.Value) {
    doPartTimeJobAndRest();
    goToLobby();
}