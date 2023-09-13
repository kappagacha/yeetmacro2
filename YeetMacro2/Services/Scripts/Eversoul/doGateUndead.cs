using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    public string doGateUndead()
    {
        var loopPatterns = new PatternNode[] { patterns["lobby"]["everstone"], patterns["titles"]["adventure"], patterns["titles"]["gateBreakthrough"], patterns["gateBreakthrough"]["challenge"], patterns["gateBreakthrough"]["nextStage"], patterns["gateBreakthrough"]["retry"] };
        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(loopPatterns);
            switch (loopResult.Path)
            {
                case "lobby.everstone":
                    logger.LogInformation("doGateBreakthrough undead: click adventure");
                    macroService.ClickPattern(patterns["lobby"]["adventure"]);
                    Thread.Sleep(500);
                    break;
                case "titles.adventure":
                    logger.LogInformation("doGateBreakthrough undead: click gate breakthrough");
                    macroService.PollPattern(patterns["gateBreakthrough"], new PollPatternFindOptions() { DoClick = true, Offset = new Point(0, -60), PredicatePattern = patterns["titles"]["gateBreakthrough"] });
                    Thread.Sleep(500);
                    break;
                case "titles.gateBreakthrough":
                    logger.LogInformation("doGateBreakthrough undead: click undead gate");
                    macroService.PollPattern(patterns["gateBreakthrough"]["gates"]["undead"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["gateBreakthrough"]["challenge"] });
                    Thread.Sleep(500);
                    break;
                case "gateBreakthrough.challenge":
                    logger.LogInformation("doGateBreakthrough undead: click challenge");
                    macroService.PollPattern(patterns["gateBreakthrough"]["challenge"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["start"] });
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["close"], PredicatePattern = new PatternNode[] { patterns["gateBreakthrough"]["nextStage"], patterns["gateBreakthrough"]["retry"] } });
                    Thread.Sleep(500);
                    break;
                case "gateBreakthrough.nextStage":
                    logger.LogInformation("doGateBreakthrough undead: click next stage");
                    macroService.PollPattern(patterns["gateBreakthrough"]["nextStage"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["start"] });
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["close"], PredicatePattern = patterns["gateBreakthrough"]["nextStage"] });
                    Thread.Sleep(500);
                    break;
                case "gateBreakthrough.retry":
                    logger.LogInformation("doGateBreakthrough undead: retry");
                    return "Party defeated";
            }

            Thread.Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}