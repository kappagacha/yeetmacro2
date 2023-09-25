// @raw-script
async function selectPartyByRecommendedElement(xOffset) {
    let elementPatterns = ['none', 'fire', 'water', 'lightning', 'earth', 'wind', 'light', 'dark'].map(e => patterns.party.recommendedElement[e]);
    if (xOffset) {
        elementPatterns = elementPatterns.map(el => {
            const clone = macroService.ClonePattern(el);
            clone.Path += `_xOffset${xOffset}`;
            for (const pattern of clone.Patterns) {
                pattern.Rect = pattern.Rect.Offset(xOffset, 0);
                pattern.OffsetCalcType = "None";
            }
            return clone;
        });
    }
    const elementResult = macroService.PollPattern(elementPatterns);
    logger.info(`selectPartyByRecommendedElement: ${elementResult.Path}`);
    if (!elementResult.IsSuccess) return false;
    let targetElement = elementResult.Path.split('.').pop();
    logger.debug(`targetElement: ${targetElement}`);
    if (xOffset) {
        targetElement = targetElement.split('_')[0];
    }
    logger.debug(`targetElement2: ${targetElement}`);
    const targetElementName = settings.party.recommendedElement[targetElement]?.Value;
    logger.debug(`targetElementName: ${targetElementName}`);
    if (!targetElementName) {
        logger.debug(`Could not find targetElementName for ${targetElement} in settings...`);
        return false;
    }
    return selectParty(targetElementName);
}

async function selectParty(targetPartyName) {
    logger.info(`selectParty: ${targetPartyName}`);
    let currentParty = macroService.GetText(patterns.party.partyName, targetPartyName);
    let numScrolls = 0;
    while (macroService.IsRunning && currentParty != targetPartyName && numScrolls < 20) {
        scrollRight();
        numScrolls++;
        logger.debug(`numScrolls: ${numScrolls}`);
        currentParty = macroService.GetText(patterns.party.partyName, targetPartyName);
        logger.debug(`currentParty: ${currentParty}`);
        sleep(500);
    }

    return numScrolls === 20 ? false : true;
}

async function scrollRight() {
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