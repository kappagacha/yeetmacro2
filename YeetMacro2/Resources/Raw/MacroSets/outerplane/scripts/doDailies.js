// @isFavorite
// @position=-99
// Do dailies (all checked scripts)

// call multiple times to get past popups
goToLobby();
goToLobby();
goToLobby();

if (settings.doDailies.claimGuildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doDailies.claimReplenishYourStamina.Value) {
    claimReplenishYourStamina();
    goToLobby();
}

if (settings.doDailies.claimAntiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doDailies.startTerminusIsleExploration.Value) {
    startTerminusIsleExploration();
    goToLobby();
}

if (settings.doDailies.claimFreeShop.Value) {
    claimFreeShop();
    goToLobby();
}

if (settings.doDailies.doShop.Value) {
    doShop();
    goToLobby();
}

if (settings.doDailies.claimFreeRecruit.Value) {
    claimFreeRecruit();
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

if (settings.doDailies.claimMailboxExpiring.Value) {
    claimMailboxExpiring();
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

if (settings.doDailies.watchAds.Value) {
    watchAds();
    goToLobby();
}

if (settings.doDailies.claimMailboxExpiringStamina.Value) {
    claimMailboxExpiringStamina();
    goToLobby();
}

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
    refillStamina(30);
    goToLobby();
    doSurveyHub(3);
    goToLobby();
}

if (settings.doDailies.claimEventDailyMissions.Value) {
    claimEventDailyMissions();
    goToLobby();
}

if (settings.doDailies.doInfiniteCorridor.Value) {
    doInfiniteCorridor();
    goToLobby();
}

if (settings.doDailies.claimBossDailyMissions.Value) {
    claimBossDailyMissions();
    goToLobby();
}

if (settings.doDailies.claimBossDailyMissions.Value) {
    claimBossDailyMissions();
    goToLobby();
}

if (settings.doDailies.claimWorldBossRewards.Value) {
    claimWorldBossRewards();
    goToLobby();
}

if (settings.doDailies.sweepEventStoryHard.Value) {
    sweepEventStoryHard();
    goToLobby();
}

if (settings.doDailies.sweepJointChallenge.Value) {
    sweepJointChallenge();
    goToLobby();
}

if (settings.doDailies.doUpkeep.Value) {
    doUpkeep();
    goToLobby();
}

if (settings.doDailies.sellInventory.Value) {
    sellInventory();
    goToLobby();
}