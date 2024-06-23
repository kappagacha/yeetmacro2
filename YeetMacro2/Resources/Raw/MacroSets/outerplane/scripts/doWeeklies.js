// @isFavorite
// @position=-1
// Do weeklies (all checked scripts)

if (settings.doWeeklies.claimWeeklyMissions.Value) {
    claimWeeklyMissions();
    goToLobby();
}

if (settings.doWeeklies.doWeeklyShop.Value) {
    doWeeklyShop();
    goToLobby();
}