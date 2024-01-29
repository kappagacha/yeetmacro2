// @isFavorite
// @position=-1
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

if (settings.doDailies.doSideStory1.Value) {
    refillStamina(30);
    goToLobby();
    doSideStory1();
    goToLobby();
}

if (settings.doDailies.doSideStory2.Value) {
    refillStamina(30);
    goToLobby();
    doSideStory2();
    goToLobby();
}

if (settings.doDailies.doSpecialRequests.Value) {
    refillStamina(140);
    goToLobby();
    doSpecialRequests();
    goToLobby();
}