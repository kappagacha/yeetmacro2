// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

goToPhone();

if (settings.doDailies.claimTribute.Value) {
    claimTribute();
    goToPhone();
}

if (settings.doDailies.doDailyShop.Value) {
    doDailyShop();
    goToPhone();
}

if (settings.doDailies.doStageWish.Value) {
    doStageWish();
    goToPhone();
}

if (settings.doDailies.doStageBounty.Value) {
    doStageBounty();
    goToPhone();
}

if (settings.doDailies.claimDailyChecklist.Value) {
    claimDailyChecklist();
    goToPhone();
}

// claim tribute twice for sweepEvent
if (settings.doDailies.claimTribute.Value) {
    settings.claimTribute.lastRun.Value = new Date().toISOString();
    claimTribute();
    goToPhone();
}

if (settings.doDailies.sweepEvent.Value) {
    sweepEvent();
    goToPhone();
}