using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    public string doGateBeast()
    {
        var done = false;
        var loopPatterns = new PatternNode[] { patterns["lobby"]["everstone"], patterns["titles"]["adventure"], patterns["titles"]["gateBreakthrough"], patterns["gateBreakthrough"]["challenge"], patterns["gateBreakthrough"]["nextStage"], patterns["gateBreakthrough"]["retry"] };
        while (macroService.IsRunning && !done)
        {
            var loopResult = macroService.PollPattern(loopPatterns);
            switch (loopResult.Path)
            {
                case "lobby.everstone":
                    logger.LogInformation("doGateBreakthrough beast: click adventure");
                    macroService.ClickPattern(patterns["lobby"]["adventure"]);
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    break;
                case "titles.adventure":
                    logger.LogInformation("doGateBreakthrough beast: click gate breakthrough");
                    macroService.PollPattern(patterns["gateBreakthrough"], new PollPatternFindOptions() { DoClick = true, Offset = new Point(0, -60), PredicatePattern = patterns["titles"]["gateBreakthrough"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    break;
                case "titles.gateBreakthrough":
                    logger.LogInformation("doGateBreakthrough beast: click beast gate");
                    macroService.PollPattern(patterns["gateBreakthrough"]["gates"]["beast"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["gateBreakthrough"]["challenge"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    break;
                case "gateBreakthrough.challenge":
                    logger.LogInformation("doGateBreakthrough beast: click challenge");
                    macroService.PollPattern(patterns["gateBreakthrough"]["challenge"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["start"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["close"], PredicatePattern = new PatternNode[] { patterns["gateBreakthrough"]["nextStage"], patterns["gateBreakthrough"]["retry"] } });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    break;
                case "gateBreakthrough.nextStage":
                    logger.LogInformation("doGateBreakthrough beast: click next stage");
                    macroService.PollPattern(patterns["gateBreakthrough"]["nextStage"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["start"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["close"], PredicatePattern = patterns["gateBreakthrough"]["nextStage"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    break;
                case "gateBreakthrough.retry":
                    logger.LogInformation("doGateBreakthrough beast: retry");
                    return "Party defeated";

            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}