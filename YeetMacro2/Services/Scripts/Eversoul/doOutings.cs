using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    public string doOutings()
    {
        var done = false;
        var loopPatterns = new PatternNode[] { patterns["lobby"]["everstone"], patterns["town"]["evertalk"], patterns["town"]["outings"]["outingsCompleted"], patterns["town"]["outings"], patterns["titles"]["outingGo"], patterns["town"]["outings"]["selectAKeyword"] };
        var targetSoul = PatternNodeManagerViewModel.CloneNode(settings["outings"]["target"].GetValue<PatternNode>());
        targetSoul.Path = "settings.outings.target";
        foreach (var p in targetSoul.Patterns)
        {
            // original width: 1005.4752807617188 (should calculate using resolution comparisons instead)
            p.Rect = new Rect(275.85736083984375, 82.9250717163086, 1500.4752807617188, 857.20263671875);
        }
        while (macroService.IsRunning && !done)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
                case "lobby.everstone":
                    logger.LogInformation("doOutings: click town");
                    macroService.PollPattern(patterns["lobby"]["town"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["town"]["enter"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["town"]["enter"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["town"]["evertalk"] });
                    break;
                case "town.evertalk":
                    logger.LogInformation("doOutings: click info with Offset");
                    macroService.PollPattern(patterns["town"]["info"], new PollPatternFindOptions() { DoClick = true, Offset = new Point(-60, 0), PredicatePattern = patterns["town"]["outings"] });
                    break;
                case "town.outings":
                    logger.LogInformation("doOutings: click outing target");
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.ClickPattern(targetSoul);
                    macroService.PollPattern(targetSoul, new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["town"]["outings"]["call"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["town"]["outings"]["call"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["prompt"]["confirm"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["prompt"]["confirm"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["town"]["outings"]["goOnOuting"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["town"]["outings"]["goOnOuting"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["next"], PredicatePattern = patterns["titles"]["outingGo"] });
                    break;
                case "titles.outingGo":
                    logger.LogInformation("doOutings: select outing");
                    var outingNumber = macroService.Random(1, 4);
                    logger.LogDebug("outingNumber: " + outingNumber);

                    macroService.PollPattern(patterns["town"]["outings"]["outing" + outingNumber], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["prompt"]["confirm"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["prompt"]["confirm"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["next"], PredicatePattern = patterns["town"]["outings"]["keywordSelectionOpportunity"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["prompt"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["middleTap"], PredicatePattern = patterns["town"]["outings"]["selectAKeyword"] });
                    break;
                case "town.outings.selectAKeyword":
                    logger.LogInformation("doOutings: select keyword");
                    var keywordPoints1 = Regex.Replace(macroService.GetText(patterns["town"]["outings"]["keywordPoints1"]), "[, ]", "");
                    var keywordPoints2 = Regex.Replace(macroService.GetText(patterns["town"]["outings"]["keywordPoints2"]), "[, ]", "");
                    var keywordPoints3 = Regex.Replace(macroService.GetText(patterns["town"]["outings"]["keywordPoints3"]), "[, ]", "");
                    logger.LogInformation("keywordPoints1: " + keywordPoints1);
                    logger.LogInformation("keywordPoints2: " + keywordPoints2);
                    logger.LogInformation("keywordPoints3: " + keywordPoints3);

                    var keywordPoints = new int[] { int.Parse(keywordPoints1), int.Parse(keywordPoints2), int.Parse(keywordPoints3) };
                    int maxIdx = keywordPoints
                        .Select((val, idx) => new { Value = val, Index = idx })
                        .Aggregate((max, current) => current.Value > max.Value ? current : max)
                        .Index;
                    logger.LogDebug("keywordTarget: " + (maxIdx + 1));
                    macroService.PollPattern(patterns["town"]["outings"]["keywordPoints" + (maxIdx + 1)], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["prompt"]["next"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["prompt"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["prompt"]["next"], patterns["prompt"]["tapTheScreen"], patterns["prompt"]["middleTap"] }, PredicatePattern = new PatternNode[] { patterns["town"]["outings"]["selectAKeyword"], patterns["town"]["outings"] } });
                    break;
                case "town.outings.outingsCompleted":
			    done = true;
                break;
            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}