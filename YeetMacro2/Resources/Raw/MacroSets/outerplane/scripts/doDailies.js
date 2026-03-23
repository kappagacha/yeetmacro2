// @tags=favorites
// @position=-99
// Do dailies (all checked scripts)
const daily = dailyManager.GetCurrentDaily();

settings.applyPreset.lastApplied.IsEnabled = true;

goToLobby();

if (settings.doDailies.claimGuildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doDailies.claimAntiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doDailies.claimFreeRecruit.Value) {
    claimFreeRecruit();
    goToLobby();
}

if (settings.doDailies.doArena.Value) {
    doArena();
    goToLobby();
}

if (settings.doDailies.doPursuitOperation.Value && daily.doPursuitOperation.count.Count == 0) {
    doPursuitOperation();
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

if (settings.doDailies.watchAds.Value) {
    watchAds();
    goToLobby();
}

if (settings.doDailies.doShop.Value) {
    doShop();
    // after getting arena tickets, reset doArena lastRun so it can run again
    settings.doArena.lastRun.Value = new Date().toISOString();
    goToLobby();
}

if (settings.doDailies.claimMailboxExpiring.Value && daily.claimMailboxExpiring.count.Count == 0) {
    claimMailboxExpiring();
    goToLobby();
}

if (settings.doDailies.claimMailboxExpiringStamina.Value && daily.claimMailboxExpiringStamina.count.Count == 0) {
    claimMailboxExpiringStamina();
    goToLobby();
}

// claim mailbox twice in case anything is missed
//if (settings.doDailies.claimMailboxExpiring.Value && daily.claimMailboxExpiring.count.Count == 1) {
//    claimMailboxExpiring();
//    goToLobby();
//}

//if (settings.doDailies.claimMailboxExpiringStamina.Value && daily.claimMailboxExpiringStamina.count.Count == 1) {
//    claimMailboxExpiringStamina();
//    goToLobby();
//}

if (settings.doDailies.sweepAll.Value) {
    sweepAll();
    goToLobby();
}

if (settings.doDailies.doSurveyHub.Value) {
    refillStamina(30);
    goToLobby();
    doSurveyHub(3);
    goToLobby();
}

if (settings.doDailies.doTerminusIsleExplorationWithSupportPack.Value) {
    refillStamina(40);
    goToLobby();
    doTerminusIsleExplorationWithSupportPack();
    goToLobby();
}

if (settings.doDailies.doInfiniteCorridor.Value) {
    doInfiniteCorridor();
    goToLobby();
}

if (settings.doDailies.claimWorldBossRewards.Value) {
    claimWorldBossRewards();
    goToLobby();
}

if (settings.doDailies.sweepEventStoryHard.Value) {
    sweepEventStoryHard();
    goToLobby();
}

if (settings.doDailies.sweepEventStoryHard2.Value) {
    sweepEventStoryHard2();
    goToLobby();
}

if (settings.doDailies.sweepJointChallenge.Value) {
    sweepJointChallenge();
    goToLobby();
}

if (settings.doDailies.doUpkeep.Value) {
    if (!daily.doUpkeep.refillStamina.IsChecked) {
        const ecologyRunsLeft = getRunsLeft('ecologyStudy');
        const identificationRunsLeft = getRunsLeft('identification');
        const totalRunsLeft = ecologyRunsLeft + identificationRunsLeft;
        const targetStamina = totalRunsLeft * 16;
        // 5 stages * 16 stamina * 3 runs = 240 * 2 (ecology study and identification) = 480 stamina
        refillStamina(targetStamina);

        daily.doUpkeep.refillStamina.IsChecked = true;
        goToLobby();
    }

    doUpkeep();
    goToLobby();
}

if (settings.doDailies.claimDailyMissions.Value) {
    claimDailyMissions();
    goToLobby();
}

if (settings.doDailies.claimEventDailyMissions.Value) {
    claimEventDailyMissions();
    goToLobby();
}

if (settings.doDailies.sellInventory.Value) {
    sellInventory();
    goToLobby();
}

if (settings.doDailies.claimEventDailyFireworks.Value) {
    claimEventDailyFireworks();
    goToLobby();
}

if (settings.doDailies.claimEventDailyMissions2.Value) {
    claimEventDailyMissions2();
    goToLobby();
}

if (settings.doDailies.doPursuitOperation.Value && daily.doPursuitOperation.count.Count == 1) {
    doPursuitOperation();
    goToLobby();
}

function getRunsLeft(stageCategory) {
    const ecologyStudyKeyToBossType = {
        0: 'masterlessGuardian',
        1: 'tyrantToddler',
        2: 'unidentifiedChimera',
        3: 'sacreedGuardian',
        4: 'grandCalamari'
    };
    const identificationKeyToBossType = {
        0: 'dekRilAndMekRil',
        1: 'glicys',
        2: 'blazingKnightMeteos',
        3: 'arsNova',
        4: 'amadeus'
    };
    const maxStageRun = 3;
    const keyToBossType = stageCategory === 'ecologyStudy'
        ? ecologyStudyKeyToBossType
        : identificationKeyToBossType;

    return Object.values(keyToBossType).reduce((total, bossType) => {
        return total + Math.max(0, maxStageRun -
            daily.doSpecialRequestsStage13[stageCategory][bossType].Count);
    }, 0);
}