using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string doBattleArenaEx()
    {
        var looppatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["battleArena"], patterns["titles"]["party"], patterns["battle"]["report"] };
        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(looppatterns, new PollPatternFindOptions() { ClickPattern = patterns["battleArena"]["newHighScore"] });
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("doBattleArenaEx: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("doBattleArenaEx: click battle arena");
                    macroService.ClickPattern(patterns["quest"]["battleArena"]);
                    break;
                case "titles.battleArena":
                    logger.LogInformation("doBattleArena: start arena");
                    macroService.PollPattern(patterns["battleArena"]["tabs"]["arenaEX"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["battleArenaEX"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battleArena"]["begin"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battleArena"]["exRank"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battleArena"]["exRank"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["prepare"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    break;
                case "titles.party":
                    logger.LogInformation("doBattleArenaEx: select party");
                    var scoreBonuspatterns = new string[] { "physicalDamage", "lowRarity", "magicDamage", "defence", "characterBonus" }.Select(sb => patterns["battleArena"]["exScoreBonus"][sb]).ToArray();
                    var scoreBonusResult = macroService.PollPattern(scoreBonuspatterns);
                    var scoreBonus = scoreBonusResult.Path.Split(".").Last();
                    logger.LogDebug($"scoreBonus: {scoreBonus}");
                    if (!scoreBonusResult.IsSuccess)
                    {
                        return "Could not detect score bonus...";
                    }
                    var scoreBonusPartyName = settings["party"]["arenaEX"][scoreBonus].GetValue<string>();
                    if (String.IsNullOrEmpty(scoreBonusPartyName))
                    {
                        return $"Could not find scoreBonusPartyName for {scoreBonusPartyName} in settings...";
                    }

                    logger.LogDebug($"scoreBonusPartyName: {scoreBonusPartyName}");
                    if (!(selectParty(scoreBonusPartyName)))
                    {
                        return $"scoreBonusPartyName not found: {scoreBonusPartyName}";
                    }
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["newHighScore"], patterns["battleArena"]["rank"] }, PredicatePattern = patterns["battle"]["report"] });
                    break;
                case "battle.report":
                    logger.LogInformation("doBattleArenaEx: restart");
                    // 1st battle => battle.next (middle) => patterns["battle"]["replay"] => patterns["battle"]["replay"]["ok"]
                    // 2nd battle => battle.next (middle) => patterns["battle"]["replay"] => patterns["battle"]["replay"]["ok"]
                    // 3rd battle => battle.next (middle) => patterns["battle"]["replay"]["disabled"]

                    macroService.PollPattern(patterns["battle"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["newHighScore"], patterns["battleArena"]["rank"] }, PredicatePattern = new PatternNode[] { patterns["battle"]["replay"], patterns["battle"]["next3"] } });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    var replayResult = macroService.FindPattern(patterns["battle"]["replay"]);
                    if (replayResult.IsSuccess)
                    {
                        logger.LogDebug("doBattleArenaEx: found replay");
                        macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["replay"]["ok"] });
                        macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    }
                    else
                    {
                        logger.LogDebug("doBattleArenaEx: found next3");
                        var next3Result = macroService.PollPattern(patterns["battle"]["next3"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battleArena"]["prompt"]["ok"], patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = new PatternNode[] { patterns["titles"]["battleArena"], patterns["battle"]["replay"] } });
                        if (next3Result.PredicatePath == "titles.battleArena")
                        {
                            return String.Empty;
                        }
                        macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["replay"]["ok"] });
                        macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    }
                    break;
            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}
