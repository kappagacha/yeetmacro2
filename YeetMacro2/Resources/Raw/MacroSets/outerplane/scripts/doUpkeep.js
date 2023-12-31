// @isFavorite
// @position=-1
// Do upkeep (all checked scripts)

if (settings.doUpkeep.claimAntiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doUpkeep.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doUpkeep.doEcologyStudy.Value) {
    doEcologyStudy();
    goToLobby();
}

if (settings.doUpkeep.doIdentification.Value) {
    doBanditChase();
    goToLobby();
}
