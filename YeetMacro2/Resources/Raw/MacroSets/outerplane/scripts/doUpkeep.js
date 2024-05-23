// @isFavorite
// @position=-100
// Do upkeep (all checked scripts)

const daily = dailyManager.GetCurrentDaily();
const utcHour = new Date().getUTCHours();
const isStamina1 = utcHour < 11;

if ((isStamina1 && !daily.claimReplenishYourStamina.done1.IsChecked) || (!isStamina1 && !daily.claimReplenishYourStamina.done2.IsChecked)) {
    goToLobby();
    claimReplenishYourStamina();
    goToLobby();
}

if (!daily.claimArenaRewards.done.IsChecked) {
    goToLobby();
    claimArenaRewards();
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