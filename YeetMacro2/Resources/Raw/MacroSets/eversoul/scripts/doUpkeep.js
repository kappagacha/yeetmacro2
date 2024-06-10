// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

goToLobby();

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
    doPartTimeJobAndRest();     // 2 times cause part time job can produce tired souls
    goToLobby();
}