using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    public string doArena()
    {
        var loopPatterns = new PatternNode[] { patterns["lobby"]["everstone"], patterns["titles"]["adventure"], patterns["adventure"]["arena"]["freeChallenge"], patterns["adventure"]["arena"]["startMatch"], patterns["adventure"]["arena"]["ticket"] };
        while (macroService.IsRunning)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
                case "lobby.everstone":
                    logger.LogInformation("doArena: click adventure");
                    macroService.ClickPattern(patterns["lobby"]["adventure"]);
                    break;
                case "titles.adventure":
                    logger.LogInformation("doArena: click arena");
                    macroService.PollPattern(patterns["adventure"]["tabs"]["arena"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["adventure"]["arena"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["adventure"]["arena"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["adventure"]["arena"]["info"] });
                    break;
                case "adventure.arena.freeChallenge":
                    logger.LogInformation("doArena: free challenges");
                    macroService.PollPattern(patterns["adventure"]["arena"]["freeChallenge"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["adventure"]["arena"]["startMatch"] });
                    break;
                case "adventure.arena.startMatch":
                    logger.LogInformation("doArena: start match");
                    var match1CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match1"]["cp"]), "[, ]", "");
                    var match2CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match2"]["cp"]), "[, ]", "");
                    var match3CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match3"]["cp"]), "[, ]", "");

                    logger.LogDebug("match1CP: " + match1CP);
                    logger.LogDebug("match2CP: " + match2CP);
                    logger.LogDebug("match3CP: " + match3CP);

                    var matches = new int[] { int.Parse(match1CP), int.Parse(match2CP), int.Parse(match3CP) };
                    int minIdx = matches
                        .Select((val, idx) => new { Value = val, Index = idx })
                        .Aggregate((min, current) => current.Value < min.Value ? current : min)
                        .Index;
                    var minCP = matches[minIdx];
                    var cpThreshold = int.Parse(settings["arena"]["cpThreshold"].GetValue<string>());

                    logger.LogDebug("minIdx: " + minIdx);
                    logger.LogDebug("minCP: " + minCP);
                    logger.LogDebug("cpThreshold: " + cpThreshold);
                    if (minCP <= cpThreshold)
                    {
                        macroService.PollPattern(patterns["adventure"]["arena"]["match" + (minIdx + 1)]["challenge"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["start"] });
                        new System.Threading.ManualResetEvent(false).WaitOne(500);
                        macroService.PollPattern(patterns["battle"]["skip"]["disabled"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["skip"]["enabled"] });
                        macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["prompt"]["confirm2"] });
                        macroService.PollPattern(patterns["prompt"]["confirm2"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["adventure"]["arena"]["freeChallenge"], patterns["adventure"]["arena"]["ticket"] } });
                        //macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["battle"]["skip"], PredicatePattern = new PatternNode[] { patterns["adventure"]["arena"]["freeChallenge"], patterns["adventure"]["arena"]["ticket"] } });
                    }
                    else
                    {
                        macroService.PollPattern(patterns["battle"]["rematch"], new PollPatternFindOptions() { DoClick = true, InversePredicatePattern = patterns["battle"]["rematch"] });
                    }
                    break;
                case "adventure.arena.ticket":
                    //logger.LogInformation("doArena: done");
                    //return String.Empty;
                    macroService.ClickPattern(patterns["adventure"]["arena"]["lobby"]);
                    break;
            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");

        return String.Empty;
    }
}
