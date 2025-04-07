// @raw-script
function selectPartyByRecommendedElement(recommendedElementSetting) {
    const recommendedElemntResult = macroService.FindPattern(patterns.party.recommendedElement);
    if (!recommendedElemntResult.IsSuccess) {
        throw new Error('Could not find recommendedElement label');
    }

    const elementPatterns = ['none', 'fire', 'water', 'lightning', 'earth', 'wind', 'light', 'dark']
        .map(e => patterns.party.recommendedElement[e])
        .map(el => {
            const clone = macroService.ClonePattern(el, {
                Path: `${el.Path}_${parseInt(recommendedElemntResult.Point.X + 58)}`,
                X: recommendedElemntResult.Point.X + 58,
                OffsetCalcType: 'None'
            });
            return clone;
        });

    const elementResult = macroService.PollPattern(elementPatterns);
    logger.info(`selectPartyByRecommendedElement: ${elementResult.Path}`);
    let targetElement = elementResult.Path.split('.').pop().split('_')[0];
    logger.debug(`targetElement: ${targetElement}`);
    const targetElementName = recommendedElementSetting[targetElement]?.Value;
    logger.debug(`targetElementName: ${targetElementName}`);
    if (!targetElementName) {
        throw new Error(`Could not find targetElementName for ${targetElement} in settings...`);
    }
    selectParty(targetElementName);
}

function selectParty(targetPartyName) {
    logger.info(`selectParty: ${targetPartyName}`);
    let currentParty = macroService.FindText(patterns.party.partyName, targetPartyName);
    let numScrolls = 0;
    while (macroService.IsRunning && currentParty != targetPartyName && numScrolls < 20) {
        scrollRight();
        numScrolls++;
        logger.debug(`numScrolls: ${numScrolls}`);
        currentParty = macroService.FindText(patterns.party.partyName, targetPartyName);
        logger.debug(`currentParty: ${currentParty}`);
        sleep(500);
    }

    if (numScrolls === 20) {
        throw new Error(`targetPartyName not found: ${targetPartyName}`);
    }
}

function scrollRight() {
    let currentX = Math.floor((macroService.FindPattern(patterns.party.slot)).Point.X);
    let prevX = currentX;
    while (macroService.IsRunning && currentX === 0) {
        currentX = Math.floor((macroService.FindPattern(patterns.party.slot)).Point.X);
        sleep(500);
    }
    while (macroService.IsRunning && (currentX === 0 || currentX === prevX)) {
        logger.debug(`currentX: ${currentX}, prevX: ${prevX}`);
        macroService.ClickPattern(patterns.party.scrollRight);
        sleep(500);
        currentX = Math.floor((macroService.FindPattern(patterns.party.slot)).Point.X);
    }
}