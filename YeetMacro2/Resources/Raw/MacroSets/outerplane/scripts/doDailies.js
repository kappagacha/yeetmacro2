// @tags=favorites
// @position=-99
// Do dailies (all checked scripts)
const daily = dailyManager.GetCurrentDaily();

settings.applyPreset.lastApplied.IsEnabled = true;

goToLobby();

if (settings.doDailies.claim.guildBuff.Value) {
    claimGuildBuff();
    goToLobby();
}

if (settings.doDailies.claim.antiparticle.Value) {
    claimAntiparticle();
    goToLobby();
}

if (settings.doDailies.claim.freeRecruit.Value) {
    claimFreeRecruit();
    goToLobby();
}

if (settings.doDailies.doArena.standard.Value) {
    doArenaStandard();
    goToLobby();
}

if (settings.doDailies.doArena.realTime.Value) {
    doArenaRealTime();
    goToLobby();
}

if (settings.doDailies.doPursuitOperation.Value && daily.doPursuitOperation.count.Count == 0) {
    doPursuitOperation('normal');
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
    settings.doArena.lastRun.Value = null;
    goToLobby();
}

if (settings.doDailies.claim.mailboxExpiring.Value) {
    claim('mailboxExpiring');
    goToLobby();
}

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

if (settings.doDailies.doTerminusIsle.withSupportPack.Value) {
    refillStamina(40);
    goToLobby();
    doTerminusIsle('withSupportPack');
    goToLobby();
}

if (settings.doDailies.doInfiniteCorridor.Value) {
    doInfiniteCorridor();
    goToLobby();
}

if (settings.doDailies.claim.worldBossRewards.Value) {
    claimWorldBossRewards();
    goToLobby();
}

if (settings.doDailies.doEvent.storyHard1.Value) {
    doEvent('storyHard1');
    goToLobby();
}

if (settings.doDailies.doEvent.storyHard2.Value) {
    doEvent('storyHard2');
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

if (settings.doDailies.claim.dailyMissions.Value) {
    claimDailyMissions();
    goToLobby();
}

if (settings.doDailies.claim.eventDailyMissions.Value) {
    claimEventDailyMissions();
    goToLobby();
}

if (settings.doDailies.sellInventory.Value) {
    sellInventory();
    goToLobby();
}

if (settings.doDailies.claim.eventDailyFireworks.Value) {
    claimEventDailyFireworks();
    goToLobby();
}

if (settings.doDailies.claim.eventDailyMissions2.Value) {
    claimEventDailyMissions2();
    goToLobby();
}

if (settings.doDailies.doPursuitOperation.Value && daily.doPursuitOperation.count.Count == 1) {
    doPursuitOperation('normal');
    goToLobby();
}

if (settings.doDailies.claim.coinExchange.Value) {
    claimCoinExchange();
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
            daily.doSpecialRequest.stage13[stageCategory][bossType].Count);
    }, 0);
}