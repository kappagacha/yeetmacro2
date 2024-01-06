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

if (settings.doUpkeep.spendStaminaScript.IsEnabled) {
    globalThis[settings.doUpkeep.spendStaminaScript.Value]();
    goToLobby();
}