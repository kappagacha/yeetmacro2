let patterns, settings;

//macroService.clickPattern = function (pattern, opts = {}) {
//    const clickOffsetX = opts.clickOffsetX ?? 0;
//    const clickOffsetY = opts.clickOffsetY ?? 0;
//    const result = this.findPattern(pattern, opts);
//    if (result.isSuccess) {
//        for (const point of result.points) {
//            doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
//        }
//        sleep(500);
//    }
//    return result;
//}

//macroService.pollPattern = function (pattern, opts = {}) {
//    let result = { isSuccess: false };
//    const intervalDelayMs = opts.intervalDelayMs ?? 500;
//    const predicatePattern = opts.predicatePattern;
//    const clickPattern = opts.clickPattern;
//    const inversePredicatePattern = opts.inversePredicatePattern;
//    const clickOffsetX = opts.clickOffsetX ?? 0;
//    const clickOffsetY = opts.clickOffsetY ?? 0;

//    if (inversePredicatePattern) {
//        const inversePredicateChecks = opts.inversePredicateChecks ?? 5;
//        const inversePredicateCheckDelayMs = opts.inversePredicateCheckDelayMs ?? 100;
//        const predicateOpts = {
//            threshold: opts.predicateThreshold ?? 0.0
//        };
//        while (state.isRunning()) {
//            let numChecks = 1;
//            let inversePredicateResult = this.findPattern(inversePredicatePattern, predicateOpts);
//            while (state.isRunning() && !inversePredicateResult.isSuccess && numChecks < inversePredicateChecks) {
//                inversePredicateResult = this.findPattern(inversePredicatePattern, predicateOpts);
//                numChecks++;
//                sleep(inversePredicateCheckDelayMs);
//            }
//            if (!inversePredicateResult.isSuccess) {
//                result.inversePredicatePath = inversePredicateResult.path;
//                break;
//            }
//            result = this.findPattern(pattern, opts);
//            if (opts.doClick && result.isSuccess) {
//                const point = result.point;
//                doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (clickPattern) this.clickPattern(clickPattern, opts);
//            sleep(intervalDelayMs);
//        }
//    } else if (predicatePattern) {
//        const predicateOpts = {
//            threshold: opts.predicateThreshold ?? 0.0
//        };
//        while (state.isRunning()) {
//            const predicateResult = this.findPattern(predicatePattern, predicateOpts);
//            if (predicateResult.isSuccess) {
//                result.predicatePath = predicateResult.path;
//                break;
//            }
//            result = this.findPattern(pattern, opts);
//            if (opts.doClick && result.isSuccess) {
//                const point = result.point;
//                doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (clickPattern) this.clickPattern(clickPattern, opts);
//            sleep(intervalDelayMs);
//        }
//    } else {
//        while (state.isRunning()) {
//            result = this.findPattern(pattern, opts);
//            if (opts.doClick && result.isSuccess) {
//                const point = result.point;
//                doClick({ x: point.x + clickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (result.isSuccess) break;
//            if (clickPattern) this.clickPattern(clickPattern, opts);
//            sleep(intervalDelayMs);
//        }
//    }

//    return result;
//}

//function doClick(point) {
//    const variance = 3;
//    const xSign = Math.random() < 0.5 ? -1 : 1;
//    const ySign = Math.random() < 0.5 ? -1 : 1;
//    const xVariance = Math.floor(Math.random() * variance) * xSign;
//    const yVariance = Math.floor(Math.random() * variance) * ySign;
//    point.x += xVariance;
//    point.y += yVariance;
//    screenService.doClick(point);
//}

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

//function sleep(ms) {
//    return new Promise(r => setTimeout(r, ms));
//}