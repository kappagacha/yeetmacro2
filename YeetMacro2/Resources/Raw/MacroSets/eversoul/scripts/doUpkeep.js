// Do upkeep (all checked scripts)

if (settings.doDailies.claimLoot.Value) {
    claimLoot();
    goToLobby();
}

if (settings.doDailies.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doDailies.doPartTimeJobAndRest.Value) {
    doPartTimeJobAndRest();
    goToLobby();
}