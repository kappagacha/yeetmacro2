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
    const date = new Date();
    date.setDate(date.getDate() - 1);
    // set date to yesterday for doPartTimeJobAndRest to run
    settings.doPartTimeJobAndRest.lastRun.Value = date.toISOString();
    doPartTimeJobAndRest();     // 2 times cause part time job can produce tired souls
    goToLobby();
}