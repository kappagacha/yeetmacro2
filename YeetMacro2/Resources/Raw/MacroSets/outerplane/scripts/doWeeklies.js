// @isFavorite
// @position=-10
// Do weeklies (all checked scripts)

if (settings.doWeeklies.claimWeeklyMissions.Value) {
    claimWeeklyMissions();
    goToLobby();
}

if (settings.doWeeklies.doWeeklyCraft.Value) {
    doWeeklyCraft();
    goToLobby();
}

if (settings.doWeeklies.doWeeklyShop.Value) {
    doWeeklyShop();
    goToLobby();
}