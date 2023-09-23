using Microsoft.Extensions.Logging;
using System.Text.Json;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string farmEventLoop()
    {
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["party"], patterns["titles"]["events"], patterns["battle"]["report"] };
        var numBattles = 0;
        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(loopPatterns);
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("farmEventLoop: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("farmEventLoop: click quest events");
                    macroService.ClickPattern(patterns["quest"]["events"]);
                    break;
                case "titles.events":
                    logger.LogInformation("farmEventLoop: start farm");
                    var targetFarmLevel = settings["farmEvent"]["targetLevel"].GetValue<string>() ?? "12";
                    macroService.PollPattern(patterns["quest"]["events"]["quest"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["quest"] });
                    Sleep(500);
                    macroService.PollPattern(patterns["quest"]["events"]["quest"]["normal"][targetFarmLevel], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["events"] });
                    Sleep(500);
                    macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    Sleep(500);
                    break;
                case "titles.party":
                    logger.LogInformation("farmEventLoop: select party");
                    var targetPartyName = settings["party"]["farmEventLoop"].GetValue<string>();
                    logger.LogDebug($"targetPartyName: {targetPartyName}");
                    if (targetPartyName == "recommendedElement")
                    {
                        selectPartyByRecommendedElement();
                    }
                    else if (!String.IsNullOrWhiteSpace(targetPartyName))
                    {
                        if (!(selectParty(targetPartyName)))
                        {
                            return $"targetPartyName not found: {targetPartyName}";
                        }
                    }

                    Sleep(500);
                    var beginResult = macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = new PatternNode[] { patterns["battle"]["report"], patterns["stamina"]["prompt"]["recoverStamina"] } });
                    if (beginResult.PredicatePath == "stamina.prompt.recoverStamina")
                    {
                        return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Out of stamina..." }, new JsonSerializerOptions() { WriteIndented = true });
                    }
                    numBattles++;
                    break;
                case "battle.report":
                    logger.LogInformation("farmEventLoop: replay battle");
                    macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battle"]["next"], patterns["battle"]["affinityLevelUp"], patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["battle"]["replay"]["prompt"] });
                    Sleep(500);
                    var replayResult = macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["battle"]["report"], patterns["stamina"]["prompt"]["recoverStamina"] } });
                    if (replayResult.PredicatePath == "stamina.prompt.recoverStamina")
                    {
                        return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Out of stamina..." }, new JsonSerializerOptions() { WriteIndented = true });
                    }
                    numBattles++;
                    break;
            }

            Sleep(1_000);
        }
        logger.LogInformation("Done...");

        return String.Empty;
    }
}
