let patterns, settings;

//macroService.ClickPattern = function (pattern, opts = {}) {
//    const ClickOffsetX = opts.ClickOffsetX ?? 0;
//    const clickOffsetY = opts.clickOffsetY ?? 0;
//    const result = this.findPattern(pattern, opts);
//    if (result.IsSuccess) {
//        for (const point of result.Points) {
//            DoClick({ x: point.x + ClickOffsetX, y: point.y + clickOffsetY });
//        }
//        sleep(500);
//    }
//    return result;
//}

//macroService.PollPattern = function (pattern, opts = {}) {
//    let result = { IsSuccess: false };
//    const IntervalDelayMs = opts.IntervalDelayMs ?? 500;
//    const PredicatePattern = opts.PredicatePattern;
//    const ClickPattern = opts.ClickPattern;
//    const InversePredicatePattern = opts.InversePredicatePattern;
//    const ClickOffsetX = opts.ClickOffsetX ?? 0;
//    const clickOffsetY = opts.clickOffsetY ?? 0;

//    if (InversePredicatePattern) {
//        const InversePredicateChecks = opts.InversePredicateChecks ?? 5;
//        const InversePredicateCheckDelayMs = opts.InversePredicateCheckDelayMs ?? 100;
//        const predicateOpts = {
//            threshold: opts.PredicateThreshold ?? 0.0
//        };
//        while (macroService.IsRunning) {
//            let numChecks = 1;
//            let inversePredicateResult = this.findPattern(InversePredicatePattern, predicateOpts);
//            while (macroService.IsRunning && !inversePredicateResult.IsSuccess && numChecks < InversePredicateChecks) {
//                inversePredicateResult = this.findPattern(InversePredicatePattern, predicateOpts);
//                numChecks++;
//                sleep(InversePredicateCheckDelayMs);
//            }
//            if (!inversePredicateResult.IsSuccess) {
//                result.InversePredicatePath = inversePredicateResult.Path;
//                break;
//            }
//            result = this.findPattern(pattern, opts);
//            if (opts.DoClick && result.IsSuccess) {
//                const point = result.Point;
//                DoClick({ x: point.x + ClickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (ClickPattern) this.ClickPattern(ClickPattern, opts);
//            sleep(IntervalDelayMs);
//        }
//    } else if (PredicatePattern) {
//        const predicateOpts = {
//            threshold: opts.PredicateThreshold ?? 0.0
//        };
//        while (macroService.IsRunning) {
//            const predicateResult = this.findPattern(PredicatePattern, predicateOpts);
//            if (predicateResult.IsSuccess) {
//                result.PredicatePath = predicateResult.Path;
//                break;
//            }
//            result = this.findPattern(pattern, opts);
//            if (opts.DoClick && result.IsSuccess) {
//                const point = result.Point;
//                DoClick({ x: point.x + ClickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (ClickPattern) this.ClickPattern(ClickPattern, opts);
//            sleep(IntervalDelayMs);
//        }
//    } else {
//        while (macroService.IsRunning) {
//            result = this.findPattern(pattern, opts);
//            if (opts.DoClick && result.IsSuccess) {
//                const point = result.Point;
//                DoClick({ x: point.x + ClickOffsetX, y: point.y + clickOffsetY });
//                sleep(500);
//            }
//            if (result.IsSuccess) break;
//            if (ClickPattern) this.ClickPattern(ClickPattern, opts);
//            sleep(IntervalDelayMs);
//        }
//    }

//    return result;
//}

//function DoClick(point) {
//    const variance = 3;
//    const xSign = Math.random() < 0.5 ? -1 : 1;
//    const ySign = Math.random() < 0.5 ? -1 : 1;
//    const xVariance = Math.floor(Math.random() * variance) * xSign;
//    const yVariance = Math.floor(Math.random() * variance) * ySign;
//    point.x += xVariance;
//    point.y += yVariance;
//    macroService.DoClick(point);
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