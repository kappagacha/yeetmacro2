using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    public string doChampsArena()
    {
        var loopPatterns = new PatternNode[] {
                patterns["lobby"]["everstone"],
        };
        while (macroService.IsRunning)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
            }
            Sleep(1_000);
        }
        logger.LogInformation("Done...");

        return String.Empty;
    }

    //public string doChampsArena()
    //{
    //    var loopPatterns = new PatternNode[] { patterns["lobby"]["everstone"], patterns["titles"]["adventure"], patterns["adventure"]["arena"]["freeChallenge"], patterns["adventure"]["arena"]["startMatch"], patterns["adventure"]["champsArena"]["buyTicket"] };
    //    while (macroService.IsRunning)
    //    {
    //        var result = macroService.PollPattern(loopPatterns);
    //        switch (result.Path)
    //        {
    //            case "lobby.everstone":
    //                logger.LogInformation("doChampsArena: click adventure");
    //                macroService.ClickPattern(patterns["lobby"]["adventure"]);
    //                break;
    //            case "titles.adventure":
    //                logger.LogInformation("doChampsArena: click champs arena");
    //                macroService.PollPattern(patterns["adventure"]["tabs"]["arena"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["adventure"]["champsArena"] });
    //                Sleep(500);
    //                macroService.PollPattern(patterns["adventure"]["champsArena"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["champsArena"] });
    //                break;
    //            case "adventure.arena.freeChallenge":
    //                logger.LogInformation("doChampsArena: free challenges");
    //                macroService.PollPattern(patterns["adventure"]["arena"]["freeChallenge"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["adventure"]["arena"]["startMatch"], IntervalDelayMs = 1_000 });
    //                break;
    //            case "adventure.arena.startMatch":
    //                logger.LogInformation("doChampsArena: start match");
    //                var match1CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match1"]["cp"]), "[, ]", "");
    //                var match2CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match2"]["cp"]), "[, ]", "");
    //                var match3CP = Regex.Replace(macroService.GetText(patterns["adventure"]["arena"]["match3"]["cp"]), "[, ]", "");
    //                logger.LogDebug("match1CP: " + match1CP);
    //                logger.LogDebug("match2CP: " + match2CP);
    //                logger.LogDebug("match3CP: " + match3CP);

    //                var matches = new int[] { int.Parse(match1CP), int.Parse(match2CP), int.Parse(match3CP) };
    //                int minIdx = matches
    //                    .Select((val, idx) => new { Value = val, Index = idx })
    //                    .Aggregate((min, current) => current.Value < min.Value ? current : min)
    //                    .Index;
    //                var minCP = matches[minIdx];
    //                var cpThreshold = int.Parse(settings["champsArena"]["cpThreshold"].GetValue<string>());

    //                logger.LogInformation("minIdx: " + minIdx);
    //                logger.LogInformation("minCP: " + minCP);
    //                logger.LogInformation("cpThreshold: " + cpThreshold);
    //                if (minCP <= cpThreshold)
    //                {
    //                    macroService.PollPattern(patterns["adventure"]["arena"]["match" + (minIdx + 1)]["challenge"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["adventure"]["champsArena"]["nextTeam"], PredicatePattern = patterns["battle"]["start"], IntervalDelayMs = 1_000 });
    //                    Sleep(500);
    //                    macroService.PollPattern(patterns["battle"]["start"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["battle"]["skip"], PredicatePattern = new PatternNode[] { patterns["adventure"]["arena"]["freeChallenge"], patterns["adventure"]["champsArena"]["buyTicket"] } });
    //                }
    //                else
    //                {
    //                    macroService.PollPattern(patterns["battle"]["rematch"], new PollPatternFindOptions() { DoClick = true, InversePredicatePattern = patterns["battle"]["rematch"] });
    //                }
    //                break;
    //            case "adventure.champsArena.buyTicket":
    //                return String.Empty;
    //        }

    //        Sleep(1_000);
    //    }
    //    logger.LogInformation("Done...");

    //    return String.Empty;
    //}
}
