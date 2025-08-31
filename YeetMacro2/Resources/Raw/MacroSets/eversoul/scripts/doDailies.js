// @isFavorite
// @position=-1
// Do dailies (all checked scripts)

result = {};

goToLobby();

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

if (settings.doDailies.doShop.Value) {
    doShop();
    goToLobby();
}

if (settings.doDailies.doExpeditions.Value) {
    doExpeditions();
    goToLobby();
}

if (settings.doDailies.doPartTimeJobAndRest.Value) {
    doPartTimeJobAndRest();
    const date = new Date();
    date.setDate(date.getDate() - 1);
    // set date to yesterday for doPartTimeJobAndRest to run
    settings.doPartTimeJobAndRest.lastRun.Value = date.toISOString();
    doPartTimeJobAndRest();     // 2 times cause part time job can produce tired souls
    goToLobby();
}

if (settings.doDailies.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doDailies.doChampsArena.Value) {
    doChampsArena();
    goToLobby();
}

if (settings.doDailies.sweepDecoyOperation.Value) {
    sweepDecoyOperation();
    goToLobby();
}

if (settings.doDailies.sweepDimensionalLabyrinth.Value) {
    sweepDimensionalLabyrinth();
    goToLobby();
}

if (settings.doDailies.sweepEvent.Value) {
    sweepEvent();
    goToLobby();
}

if (settings.doDailies.sweepEventMiniGame.Value) {
    sweepEventMiniGame();
    goToLobby();
}

if (settings.doDailies.doOperationEdenAlliance.Value) {
    doOperationEdenAlliance();
    goToLobby();
}

if (settings.doDailies.levelSoulOnce.Value) {
    levelSoulOnce();
    goToLobby();
}

if (settings.doDailies.doUnlimitedGateOnce.Value) {
    doUnlimitedGateOnce();
    goToLobby();
}

if (settings.doDailies.claimDailyQuests.Value) {
    claimDailyQuests();
    goToLobby();
}


if (settings.doDailies.sweepGuildRaid.Value) {
    const sweepGuildRaidResult = sweepGuildRaid();
    if (sweepGuildRaidResult) result.sweepGuildRaid = sweepGuildRaidResult;
    goToLobby();
}

if (settings.doDailies.sweepEvilSoulSubjugation.Value) {
    const sweepEvilSoulSubjugationResult = sweepEvilSoulSubjugation();
    if (sweepEvilSoulSubjugationResult) result.sweepEvilSoulSubjugation = sweepEvilSoulSubjugationResult;
    goToLobby();
}

if (settings.doDailies.doOutings.Value) {
    doOutings();
    goToLobby();
}

if (settings.doDailies.doManageTown.Value) {
    doManageTown();
    goToLobby();
}

if (settings.doDailies.doIridescentScenicInstance.Value) {
    doIridescentScenicInstance();
    goToLobby();
}

if (!Object.keys(result).length) {
    result = null;
} else {
    return result;
}