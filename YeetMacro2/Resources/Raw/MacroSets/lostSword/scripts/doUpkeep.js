// @isFavorite
// @position=-1

goToLobby();

if (settings.doUpkeep.claimTreasureExploration.Value) {
    claimTreasureExploration();
    goToLobby();
}

if (settings.doUpkeep.doColosseum.Value) {
    doColosseum();
    goToLobby();
}

