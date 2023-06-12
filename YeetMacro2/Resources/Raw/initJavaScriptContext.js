let patterns, settings;

macroService.clickPattern = async function (pattern, opts = {}) {
    const clickOffsetX = opts.clickOffsetX ?? 0;
    const clickOffsetY = opts.clickOffsetY ?? 0;
    const result = await this.findPattern(pattern, opts);
    if (result.isSuccess) {
        for (const point of result.points) {
            screenService.doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
        }
        await sleep(500);
    }
    return result;
}

macroService.pollPattern = async function (pattern, opts = {}) {
    let result = { isSuccess: false };
    const intervalDelayMs = opts.intervalDelayMs ?? 500;
    const predicatePattern = opts.predicatePattern;
    const clickPattern = opts.clickPattern;
    const inversePredicatePattern = opts.inversePredicatePattern;
    const clickOffsetX = opts.clickOffsetX ?? 0;
    const clickOffsetY = opts.clickOffsetY ?? 0;

    if (inversePredicatePattern) {
        const inversePredicateChecks = opts.inversePredicateChecks ?? 5;
        const inversePredicateCheckDelayMs = opts.inversePredicateCheckDelayMs ?? 100;
        const predicateOpts = {
            threshold: opts.predicateThreshold ?? 0.0
        };
        while (state.isRunning) {
            let numChecks = 1;
            let inversePredicateResult = await this.findPattern(inversePredicatePattern, predicateOpts);
            while (!inversePredicateResult.isSuccess && numChecks < inversePredicateChecks) {
                inversePredicateResult = await this.findPattern(inversePredicatePattern, predicateOpts);
                numChecks++;
                await sleep(inversePredicateCheckDelayMs);
            }
            if (!inversePredicateResult.isSuccess) {
                result.inversePredicatePath = inversePredicateResult.path;
                break;
            }
            result = await this.findPattern(pattern, opts);
            if (opts.doClick && result.isSuccess) {
                const point = result.point;
                screenService.doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
                await sleep(500);
            }
            if (clickPattern) await this.clickPattern(clickPattern, opts);
            await sleep(intervalDelayMs);
        }
    } else if (predicatePattern) {
        const predicateOpts = {
            threshold: opts.predicateThreshold ?? 0.0
        };
        while (state.isRunning) {
            const predicateResult = await this.findPattern(predicatePattern, predicateOpts);
            if (predicateResult.isSuccess) {
                result.predicatePath = predicateResult.path;
                break;
            }
            result = await this.findPattern(pattern, opts);
            if (opts.doClick && result.isSuccess) {
                const point = result.point;
                screenService.doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
                await sleep(500);
            }
            if (clickPattern) await this.clickPattern(clickPattern, opts);
            await sleep(intervalDelayMs);
        }
    } else {
        while (state.isRunning) {
            result = await this.findPattern(pattern, opts);
            if (opts.doClick && result.isSuccess) {
                const point = result.point;
                screenService.doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
                await sleep(500);
            }
            if (result.isSuccess) break;
            if (clickPattern) await this.clickPattern(clickPattern, opts);
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