// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

const daily = dailyManager.GetCurrentDaily();
const utcHour = new Date().getUTCHours();
const isStamina1 = utcHour < 12;

// call multiple times to get past popups
goToLobby();
goToLobby();
goToLobby();
goToLobby();
goToLobby();

if (settings.doUpkeep.claimGuildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doUpkeep.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doUpkeep.claimReplenishYourStamina.Value) {
    claimReplenishYourStamina();
    goToLobby();
}

if (settings.doUpkeep.claimArenaRewards.Value) {
    claimArenaRewards();
    goToLobby();
}

if (settings.doUpkeep.startTerminusIsleExploration.Value) {
    startTerminusIsleExploration();
    goToLobby();
}

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