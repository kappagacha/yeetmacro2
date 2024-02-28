// @isFavorite
// @position=-100
// Do dailies (all checked scripts)

if (settings.doDailies.claimAntiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doDailies.claimFreeShop.Value) {
    claimFreeShop();
    goToLobby();
}

if (settings.doDailies.claimFreeRecruit.Value) {
    claimFreeRecruit();
    goToLobby();
}

if (settings.doDailies.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doDailies.claimGuildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doDailies.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doDailies.doBountyHunter.Value) {
    doBountyHunter();
    goToLobby();
}

if (settings.doDailies.doBanditChase.Value) {
    doBanditChase();
    goToLobby();
}

if (settings.doDailies.doUpgradeStoneRetrieval.Value) {
    doUpgradeStoneRetrieval();
    goToLobby();
}

if (settings.doDailies.doGuildSecurityArea.Value) {
    doGuildSecurityArea();
    goToLobby();
}

if (settings.doDailies.doGuildRaid.Value) {
    doGuildRaid();
    goToLobby();
}

_watchAds();
goToLobby();

if (settings.doDailies.doDoppelganger.Value) {
    doDoppelganger();
    goToLobby();
}

if (settings.doDailies.doSpecialRequests.Value) {
    doSpecialRequests();
    goToLobby();
}

if (settings.doDailies.claimDailyMissions.Value) {
    claimDailyMissions();
    goToLobby();
}

if (settings.doDailies.doSurveyHub.Value) {
    refillStamina(50);
    doSurveyHub();
    goToLobby();
}

if (settings.doDailies.claimEventDailyMissions.Value) {
    claimEventDailyMissions();
    goToLobby();
}