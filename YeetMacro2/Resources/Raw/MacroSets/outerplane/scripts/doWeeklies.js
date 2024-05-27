// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

if (settings.doWeeklies.sellInventory.Value) {
    sellInventory();
    goToLobby();
}

if (settings.doWeeklies.claimWeeklyMissions.Value) {
    claimWeeklyMissions();
    goToLobby();
}

if (settings.doWeeklies.doWeeklyShop.Value) {
    doWeeklyShop();
    goToLobby();
}