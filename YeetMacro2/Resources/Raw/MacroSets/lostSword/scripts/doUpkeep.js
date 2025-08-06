// @isFavorite
// @position=-1

goToLobby();

if (settings.doUpkeep.claimTreasureExploration.Value) {
    claimTreasureExploration();
    goToLobby();
}

if (settings.doUpkeep.doColoseum.Value) {
    doColoseum();
    goToLobby();
}

