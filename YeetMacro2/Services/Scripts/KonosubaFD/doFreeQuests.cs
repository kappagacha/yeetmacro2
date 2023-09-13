using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string doFreeQuests()
    {
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["freeQuests"] };
        var offset = macroService.CalcOffset(patterns["titles"]["home"]);

        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(loopPatterns);
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("doFreeQuests: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("doFreeQuests: click quest free quests");
                    macroService.ClickPattern(patterns["quest"]["freeQuests"]);
                    break;
                case "titles.freeQuests":
                    logger.LogInformation("doFreeQuests: target upgrade stone");
                    var upgradeStoneTargetLevel = settings["freeQuests"]["upgradeStone"]["targetLevel"].GetValue<string>() ?? "intermediate";
                    if (upgradeStoneTargetLevel != "extreme")
                    {
                        macroService.PollPattern(patterns["freeQuests"]["upgradeStone"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["freeQuests"]["upgradeStone"][upgradeStoneTargetLevel] });
                        macroService.PollPattern(patterns["freeQuests"]["upgradeStone"][upgradeStoneTargetLevel], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["tickets"]["add"] });
                        // sample text capture: "25 x1" (it catches some of the words)
                        var numTickets = int.Parse(macroService.GetText(patterns["tickets"]["numTickets"]).Split("x")[1]);
                        while (macroService.IsRunning && numTickets < 2)
                        {
                            macroService.ClickPattern(patterns["tickets"]["add"]);
                            Thread.Sleep(500);
                            numTickets = int.Parse(macroService.GetText(patterns["tickets"]["numTickets"]).Split("x")[1]);
                        }
                        macroService.PollPattern(patterns["tickets"]["use"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["tickets"]["prompt"]["ok"] });
                        macroService.PollPattern(patterns["tickets"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["freeQuests"] });
                    }

                    logger.LogInformation("doFreeQuests: skip all");
                    macroService.PollPattern(patterns["freeQuests"]["eris"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["skipQuest"] });
                    macroService.PollPattern(patterns["skipAll"]["skipQuest"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["button"] });
                    var filterOffResult = macroService.FindPattern(patterns["skipAll"]["search"]["filter"]["off"]);
                    if (filterOffResult.IsSuccess)
                    {
                        macroService.PollPattern(patterns["skipAll"]["search"]["filter"]["off"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["search"]["filter"] });
                        Thread.Sleep(500);
                        var checkResult = macroService.FindPattern(patterns["skipAll"]["search"]["filter"]["check"], new FindOptions() { Limit = 5 });
                        foreach (var point in checkResult.Points)
                        {
                            if (point.X < offset.X + 300.0) continue;       // skip 4 stars
                            macroService.DoClick(point);
                            Thread.Sleep(250);
                        }
                        macroService.PollPattern(patterns["skipAll"]["search"]["filter"]["close"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["title"] });
                        Thread.Sleep(500);
                    }

                    var maxNumSkips = int.Parse(macroService.GetText(patterns["skipAll"]["maxNumSkips"]));
                    while (macroService.IsRunning && maxNumSkips < 2)
                    {
                        macroService.ClickPattern(patterns["skipAll"]["addMaxSkips"]);
                        Thread.Sleep(500);
                        maxNumSkips = int.Parse(macroService.GetText(patterns["skipAll"]["maxNumSkips"]));
                    }
                    macroService.PollPattern(patterns["skipAll"]["button"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["prompt"]["ok"] });
                    Thread.Sleep(1_000);
                    macroService.PollPattern(patterns["skipAll"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["skipComplete"] });
                    macroService.PollPattern(patterns["skipAll"]["skipComplete"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["skipAll"]["prompt"]["ok"], patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["skipAll"]["title"] });
                    return String.Empty;
            }

            Thread.Sleep(1_000);
        }
        logger.LogInformation("Done...");

        return String.Empty;
    }
}
