let patterns, settings;

macroService.clickPattern = async function (pattern, opts = {}) {
    const clickOffsetX = opts.clickOffsetX ?? 0;
    const clickOffsetY = opts.clickOffsetY ?? 0;
    const result = await this.findPattern(pattern, opts);
    if (result.isSuccess) {
        for (const point of result.points) {
            screenService.doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
        }
    }
    return result;
}

macroService.pollPattern = async function (pattern, opts = {}) {
    //logger.debug('pollPattern');
    let result = { isSuccess: false };
    let patternFound = false;
    const intervalDelayMs = opts.intervalDelayMs ?? 1000;
    const predicatePattern = opts.predicatePattern;
    const clickPattern = opts.clickPattern;
    const inversePredicatePattern = opts.inversePredicatePattern;

    if (predicatePattern || inversePredicatePattern) {
        //logger.debug('predicatePattern');
        let inversePatternNotFoundChecks = 0;
        const predicateCheckSteps = opts.predicateCheckSteps ?? 8;
        const inversePredicateCheckSteps = opts.inversePredicateCheckSteps ?? 4;
        const predicateCheckDelayMs = opts.predicateCheckDelayMs ?? 250;
        const predicateOpts = {
            threshold: opts.predicateThreshold ?? 0.0
        };

        //logger.debug('predicateCheckSteps: ' + predicateCheckSteps);
        //logger.debug('inversePredicateCheckSteps: ' + inversePredicateCheckSteps);
        //logger.debug('predicateCheckDelayMs: ' + predicateCheckDelayMs);
        for (const i = predicateCheckSteps; ; i++) {
            //logger.debug('i: ' + i);
            //screenService.debugClear();
            if (!state.isRunning) return result;

            if (predicatePattern) {
                const predicateResult = await this.findPattern(predicatePattern, predicateOpts);
                //logger.debug('predicateResult.isSuccess: ' + predicateResult.isSuccess);
                if (predicateResult.isSuccess) {
                    result.predicatePath = predicateResult.path;
                    break;
                }
                //screenService.debugClear();
            }

            if (inversePredicatePattern) {
                const inversePredicateResult = await this.findPattern(inversePredicatePattern, predicateOpts);
                //logger.debug('inversePredicateResult.isSuccess: ' + inversePredicateResult.isSuccess);
                if (!inversePredicateResult.isSuccess && ++inversePatternNotFoundChecks >= inversePredicateCheckSteps) {
                    break;
                }
                //screenService.debugClear();
            }

            if (i % predicateCheckSteps === 0) {
                //logger.debug('3 % predicateCheckSteps: === 0');
                result = await this.findPattern(pattern, opts);
                if (opts.doClick && result.isSuccess) screenService.doClick(result.point);
                await sleep(intervalDelayMs);
                //screenService.debugClear();
                if (clickPattern) await this.clickPattern(clickPattern, opts);
            }

            await sleep(predicateCheckDelayMs);
        }
        //logger.debug('predicatePattern end');
    } else {
        while (!patternFound) {
            //screenService.debugClear();
            if (!state.isRunning) return result;

            if (clickPattern) await this.clickPattern(clickPattern, opts);
            //screenService.debugClear();

            result = await this.findPattern(pattern, opts);
            //screenService.debugClear();

            patternFound = result.isSuccess;
            if (opts.doClick && result.isSuccess) await screenService.doClick(result.point);

            await sleep(intervalDelayMs);
        }
    }

    return result;
}

function resolvePath(node, path = '') {
    let currentPath = path;
    if (node.props) {
        currentPath = `${path ? path + '.' : ''}` + node.props.name;
        node.props.path = currentPath;
    }
    if (node.$isParent) {
        for (const [key, value] of Object.entries(node)) {
            if (['props', '$isParent'].includes(key)) continue;
            resolvePath(value, currentPath);
        }
    }
}

function sleep(ms) {
    return new Promise(r => setTimeout(r, ms));
}