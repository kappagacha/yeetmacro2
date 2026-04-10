// @isFavorite
// @tags=favorites
// @position=-100
// Do upkeep (all checked scripts)

const daily = dailyManager.GetCurrentDaily();

settings.applyPreset.lastApplied.IsEnabled = true;
goToLobby();

if (settings.doUpkeep.claim.guildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doUpkeep.doFriends.Value) {
    doFriends();
    goToLobby();
}

if (settings.doUpkeep.doTerminusIsle.start.Value) {
    doTerminusIsle('start');
    goToLobby();
}

if (settings.doUpkeep.claim.antiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doUpkeep.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doUpkeep.claim.arenaRewards.Value) {
    claimArenaRewards();
    goToLobby();
}

if (settings.doUpkeep.doSpecialRequestStage13.Value && !daily.doSpecialRequest.stage13.done.IsChecked) {
    doSpecialRequestStage13();
    goToLobby();
}

if (settings.doUpkeep.doSpecialRequestStage13.Value && !daily.doSpecialRequest.stage13.doneTarget.IsChecked) {
    doSpecialRequestStage13();
    goToLobby();
}

if (settings.doUpkeep.doSpecialRequestStage13.Value && daily.doSpecialRequest.stage13.doneTarget.IsChecked && !daily.doSpecialRequest.stage13.done.IsChecked) {
    doSpecialRequestStage13();
    goToLobby();
}

if (settings.doUpkeep.spendStaminaScript.IsEnabled && daily.doSpecialRequest.stage13.done.IsChecked) {
    if (settings.doUpkeep.spendStaminaScript.Value === 'dropRateUp') {
        macroService.PollPattern(patterns.tabs.adventure, { DoClick: true, PredicatePattern: patterns.titles.adventure });
        macroService.PollPattern(patterns.adventure.challenge, { DoClick: true, PredicatePattern: patterns.titles.challenge });

        const ecologyStudyDropRateUpResult = macroService.FindPattern(patterns.challenge.ecologyStudy.dropRateUp);
        if (ecologyStudyDropRateUpResult.IsSuccess) {
            doEcologyStudy();
        } else {
            const identificationDropRateUpResult = macroService.FindPattern(patterns.challenge.identification.dropRateUp);
            if (identificationDropRateUpResult.IsSuccess) {
                doIdentification();
            } else {    // default action
                globalThis[settings.doUpkeep.specialRequestPreference.Value]();
            }
        }
    } else {
        globalThis[settings.doUpkeep.spendStaminaScript.Value]();
    }
    goToLobby();
}

if (settings.doUpkeep.doTerminusIsle.normal.Value && !daily.doTerminusIsle.done.IsChecked) {
    doTerminusIsle('normal');
    return;
}