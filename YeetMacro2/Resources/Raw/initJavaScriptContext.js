let patterns, settings;

macroService.clickPattern = async function (pattern, opts = {}) {
    const clickOffsetX = opts.clickOffsetX ?? 0;
    const clickOffsetY = opts.clickOffsetY ?? 0;
    const result = await this.findPattern(pattern, opts);
    if (result.isSuccess) {
        for (const point of result.points) {
            screenService.clickPoint({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
        }
    }
    return result;
}

macroService.pollPattern = async function (pattern, opts = {}) {
    let result = { isSuccess: false };
    let patternFound = false;
    const intervalDelayMs = opts.intervalDelayMs ?? 1000;
    const predicatePattern = opts.predicatePattern;
    const touchPattern = opts.touchPattern;
    const inversePredicatePattern = opts.inversePredicatePattern;

    if (predicatePattern || inversePredicatePattern) {
        logger.info('predicatePattern');
        let inversePatternNotFoundChecks = 0;
        const predicateCheckSteps = opts.predicateCheckSteps ?? 8;
        const inversePredicateCheckSteps = opts.inversePredicateCheckSteps ?? 4;
        const predicateCheckDelayMs = opts.predicateCheckDelayMs ?? 250;
        const predicateOpts = {
            threshold: opts.predicateThreshold ?? 0.0
        };

        for (const i = predicateCheckSteps; ; i++) {
            screenService.debugClear();
            if (!state.isRunning) return result;

            if (predicatePattern) {
                const predicateResult = await this.findPattern(predicatePattern, predicateOpts);
                if (predicateResult.isSuccess) break;
                screenService.debugClear();
            }


            if (inversePredicatePattern) {
                const inversePredicateResult = await this.findPattern(inversePredicatePattern, predicateOpts);
                if (!inversePredicateResult.isSuccess && ++inversePatternNotFoundChecks >= inversePredicateCheckSteps) {
                    break;
                }
                screenService.debugClear();
            }

            if (i % predicateCheckSteps === 0) {
                result = await this.findPattern(pattern, opts);
                if (opts.doClick && result.isSuccess) this.clickPoint(result.point);
                await sleep(intervalDelayMs);
                screenService.debugClear();
                if (touchPattern) await this.clickPattern(touchPattern, opts);
            }

            await sleep(predicateCheckDelayMs);
        }
    } else {
        logger.info('normal');
        while (!patternFound) {
            screenService.debugClear();
            if (!state.isRunning) return result;

            if (touchPattern) await this.clickPattern(touchPattern, opts);
            screenService.debugClear();

            result = await this.findPattern(pattern, opts);
            screenService.debugClear();

            patternFound = result.isSuccess;
            if (opts.doClick && result.isSuccess) await this.clickPoint(result.point);

            await sleep(intervalDelayMs);
        }
    }

    return result;
}

function resolvePath(node, path = '') {
    let currentPath = path;
    if (node.properties) {
        currentPath = `${path ? path + '.' : ''}` + node.properties.name;
        node.properties.path = currentPath;
    }
    if (node.$isParent) {
        for (const [key, value] of Object.entries(node)) {
            if (['properties', '$isParent'].includes(key)) continue;
            resolvePath(value, currentPath);
        }
    }
}

function sleep(ms) {
    return new Promise(r => setTimeout(r, ms));
}