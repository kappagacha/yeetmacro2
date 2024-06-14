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

claimGuildBuff();
goToLobby();

claimReplenishYourStamina();
goToLobby();

claimArenaRewards();
goToLobby();

startTerminusIsleExploration();
goToLobby();

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