// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

goToLobby();

if (settings.doDailies.claimTreasureExploration.Value) {
    claimTreasureExploration();
    goToLobby();
}

if (settings.doDailies.doRoulette.Value) {
    doRoulette();
    goToLobby();
}

if (settings.doDailies.claimMailbox.Value) {
    claimMailbox();
    goToLobby();
}

if (settings.doDailies.claimFreeSummon.Value) {
    claimFreeSummon();
    goToLobby();
}

if (settings.doDailies.doCharacterChat.Value) {
    doCharacterChat();
    goToLobby();
}

if (settings.doDailies.doShop.Value) {
    doShop();
    goToLobby();
}

if (settings.doDailies.doGuild.Value) {
    doGuild();
    goToLobby();
}

if (settings.doDailies.doBossRaid.Value) {
    doBossRaid();
    goToLobby();
}

if (settings.doDailies.doColosseum.Value) {
    doColosseum();
    goToLobby();
}

if (settings.doDailies.doStonehenge.Value) {
    doStonehenge();
    goToLobby();
}

if (settings.doDailies.doGold.Value) {
    doGold();
    goToLobby();
}

if (settings.doDailies.doBookOfExperience.Value) {
    doBookOfExperience();
    goToLobby();
}

if (settings.doDailies.doElementalStone.Value) {
    doElementalStone();
    goToLobby();
}

if (settings.doDailies.doPartyDungeon.Value) {
    doPartyDungeon();
    goToLobby();
}

if (settings.doDailies.doEvent1.Value) {
    doEvent1();
    goToLobby();
}

if (settings.doDailies.claimDailyMissions.Value) {
    claimDailyMissions();
    goToLobby();
}
