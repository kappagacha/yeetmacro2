// @raw-script
async function selectPartyByRecommendedElement(xOffset) {
    let elementPatterns = ['none', 'fire', 'water', 'lightning', 'earth', 'wind', 'light', 'dark'].map(e => patterns.party.recommendedElement[e]);
    if (xOffset) {
        elementPatterns = elementPatterns.map(el => ({
            ...el,
            props: {
                ...el.props,
                path: el.props.path + '_xOffset' + xOffset,
                patterns: el.props.patterns.map(p => ({
                    ...p,
                    rect: {
                        ...p.rect,
                        x: p.rect.x + xOffset
                    },
                })),
            }
        }));
    }
    const elementResult = macroService.pollPattern(elementPatterns);
    logger.info(`selectPartyByRecommendedElement: ${elementResult.path}`);
    if (!elementResult.isSuccess) return false;
    let targetElement = elementResult.path.split('.').pop();
    logger.debug(`targetElement: ${targetElement}`);
    if (xOffset) {
        targetElement = targetElement.split('_')[0];
    }
    logger.debug(`targetElement2: ${targetElement}`);
    const targetElementName = settings.party.recommendedElement[targetElement]?.props.value;
    logger.debug(`targetElementName: ${targetElementName}`);
    if (!targetElementName) {
        logger.debug(`Could not find targetElementName for ${targetElement} in settings...`);
        return false;
    }
    return selectParty(targetElementName);
}

async function selectParty(targetPartyName) {
    logger.info(`selectParty: ${targetPartyName}`);
    let currentParty = screenService.getText(patterns.party.name, targetPartyName);
    let numScrolls = 0;
    while (state.isRunning && currentParty != targetPartyName && numScrolls < 20) {
        scrollRight();
        numScrolls++;
        logger.debug(`numScrolls: ${numScrolls}`);
        currentParty = screenService.getText(patterns.party.name, targetPartyName);
        logger.debug(`currentParty: ${currentParty}`);
        sleep(500);
    }

    return numScrolls === 20 ? false : true;
}

async function scrollRight() {
    let currentX = Math.floor((macroService.findPattern(patterns.party.slot)).point.x);
    let prevX = currentX;
    while (state.isRunning && currentX === 0) {
        currentX = Math.floor((macroService.findPattern(patterns.party.slot)).point.x);
        sleep(500);
    }
    while (state.isRunning && (currentX === 0 || currentX === prevX)) {
        logger.debug(`currentX: ${currentX}, prevX: ${prevX}`);
        macroService.clickPattern(patterns.party.scrollRight);
        sleep(500);
        currentX = Math.floor((macroService.findPattern(patterns.party.slot)).point.x);
    }
}