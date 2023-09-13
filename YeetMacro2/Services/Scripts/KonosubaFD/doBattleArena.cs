using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string doBattleArena()
    {
        var looppatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["battleArena"], patterns["titles"]["party"], patterns["battle"]["report"] };
        var done = false;
        while (macroService.IsRunning && !done)
        {
            var loopResult = macroService.PollPattern(looppatterns, new PollPatternFindOptions() { ClickPattern = new PatternNode[] { patterns["battleArena"]["newHighScore"], patterns["battleArena"]["rank"] } });
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("doBattleArena: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("doBattleArena: click battle arena");
                    macroService.ClickPattern(patterns["quest"]["battleArena"]);
                    break;
                case "titles.battleArena":
                    logger.LogInformation("doBattleArena: start arena");
                    macroService.PollPattern(patterns["battleArena"]["begin"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battleArena"]["advanced"] });
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["battleArena"]["advanced"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["prepare"] });
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    break;
                case "titles.party":
                    logger.LogInformation("doBattleArena: select party");
                    var targetPartyName = settings["party"]["battleArena"].GetValue<string>();
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
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["report"] });
                    break;
                case "battle.report":
                    logger.LogInformation("doBattleArena: restart");
                    // 1st battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
                    // OR         => battle.next (middle) => battle.replay => battle.replay.ok
                    // 2nd battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay => battle.replay.ok
                    // OR         => battle.next (middle) => battle.replay => battle.replay.ok
                    // 3rd battle => battle.next (middle) => battle.next3 (right) => battleArena.prompt.ok => battle.replay.disabled
                    // OR         => battle.next (middle) => battle.replay.disabled

                    macroService.PollPattern(patterns["battle"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["newHighScore"], patterns["battleArena"]["rank"] }, PredicatePattern = new PatternNode[] { patterns["battle"]["replay"], patterns["battle"]["next3"] } });
                    Thread.Sleep(500);
                    var replayResult = macroService.FindPattern(patterns["battle"]["replay"]);
                    if (replayResult.IsSuccess)
                    {
                        logger.LogDebug("doBattleArena: found replay");
                        macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["replay"]["ok"] });
                        macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    }
                    else
                    {
                        logger.LogDebug("doBattleArena: found next3");
                        var next3Result = macroService.PollPattern(patterns["battle"]["next3"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["prompt"]["ok"], patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = new PatternNode[] { patterns["titles"]["battleArena"], patterns["battle"]["replay"] } });
                        if (next3Result.PredicatePath == "titles.battleArena")
                        {
                            done = true;
                            break;
                        }
                        macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["replay"]["ok"] });
                        macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    }
                    break;
            }

            Thread.Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}
