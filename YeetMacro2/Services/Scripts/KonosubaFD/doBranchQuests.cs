using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string doBranchQuests()
    {
        var done = false;
        var looppatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["branchEvent"], patterns["titles"]["branch"], patterns["branchEvent"]["explosionWalk"]["chant"]["disabled"], patterns["titles"]["party"] };
        string eventName = String.Empty;

        while (macroService.IsRunning && !done)
        {
            var loopResult = macroService.PollPattern(looppatterns);
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("doBranchQuests: check others notification");
                    var othersNotificationResult = macroService.FindPattern(patterns["others"]["notification"]);
                    if (othersNotificationResult.IsSuccess)
                    {
                        macroService.PollPattern(patterns["others"]["notification"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["branch"] });
                        //macroService.PollPattern(patterns["others"]["notification"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["others"]["branch"]["notification"] });
                        //macroService.PollPattern(patterns["others"]["branch"]["notification"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["branch"] });
                    }
                    else
                    {
                        done = true;
                    }
                    break;
                case "titles.branch":
                    logger.LogInformation("doBranchQuests: pick quest");
                    var eventResult = macroService.PollPattern(new PatternNode[] { patterns["branchEvent"]["cabbageHunting"], patterns["branchEvent"]["explosionWalk"], patterns["branchEvent"]["pitAPatBox"] }, new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["branchEvent"] });
                    eventName = eventResult.Path;
                    logger.LogDebug("event: " + eventName);
                    break;
                case "titles.branchEvent":
                    logger.LogInformation("doBranchQuests: start");
                    macroService.PollPattern(new PatternNode[] { patterns["branchEvent"]["prepare"], patterns["branchEvent"]["start"] }, new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["branchEvent"]["pitAPatBox"]["noVoices"], PredicatePattern = new PatternNode[] { patterns["branchEvent"]["explosionWalk"]["chant"]["disabled"], patterns["titles"]["party"], patterns["branchEvent"]["pitAPatBox"]["skip"] } });
                    new System.Threading.ManualResetEvent(false).WaitOne(1000);

                    if (eventName == "branchEvent.pitAPatBox")
                    {
                        var boxes = new string[] { "simpleWoodenBox", "veryHeavyBox", "woodenBox", "clothPouch" }.Select(option => patterns["branchEvent"]["pitAPatBox"][option]).ToArray();
                        macroService.PollPattern(patterns["branchEvent"]["pitAPatBox"]["skip"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = boxes });
                        macroService.PollPattern(boxes, new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["pitAPatBox"]["rewardGained"], patterns["branchEvent"]["pitAPatBox"]["noVoices"], patterns["branchEvent"]["pitAPatBox"]["skip"], patterns["branchEvent"]["prompt"]["ok"] }, PredicatePattern = patterns["titles"]["home"] });
                    }
                    break;
                case "branchEvent.explosionWalk.chant.disabled":
                    logger.LogInformation("doBranchQuests: explosion walk");
                    var optionNumber = macroService.Random(1, 4);
                    logger.LogDebug("option " + optionNumber);
                    macroService.PollPattern(patterns["branchEvent"]["explosionWalk"]["option" + optionNumber], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["branchEvent"]["explosionWalk"]["chant"]["enabled"] });
                    macroService.PollPattern(patterns["branchEvent"]["explosionWalk"]["chant"]["enabled"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battle"]["next"], patterns["branchEvent"]["skip"] }, PredicatePattern = patterns["titles"]["home"] });
                    break;
                case "titles.party":
                    logger.LogInformation("doBranchQuests: select party");
                    var targetPartyName = settings["party"]["cabbageHunting"].GetValue<string>();
                    logger.LogDebug($"targetPartyName: {targetPartyName}");
                    if (targetPartyName == "recommendedElement")
                    {
                        selectPartyByRecommendedElement();
                    }
                    else
                    {
                        if (!(selectParty(targetPartyName)))
                        {
                            return $"targetPartyName not found: {targetPartyName}";
                        }
                    }
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    macroService.PollPattern(patterns["battle"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["newHighScore"], patterns["battleArena"]["rank"] }, PredicatePattern = patterns["titles"]["home"] });
                    break;
            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}
