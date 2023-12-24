// Do dailies (all checked scripts)

if (settings.doDailies.claimLoot.Value) {
    claimLoot();
    goToLobby();
}

if (settings.doDailies.claimMail.Value) {
    claimMail();
    goToLobby();
}

if (settings.doDailies.claimFreeShop.Value) {
    claimFreeShop();
    goToLobby();
}

if (settings.doDailies.claimFreeSummon.Value) {
    claimFreeSummon();
    goToLobby();
}

if (settings.doDailies.claimChampsArena.Value) {
    claimChampsArena();
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

if (settings.doDailies.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doDailies.doOutings.Value) {
    doOutings();
    goToLobby();
}

if (settings.doDailies.doChampsArena.Value) {
    doChampsArena();
    goToLobby();
}
