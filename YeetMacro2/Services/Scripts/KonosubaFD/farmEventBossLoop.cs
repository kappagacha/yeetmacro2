using Microsoft.Extensions.Logging;
using System.Text.Json;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string farmEventBossLoop()
    {
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["events"], patterns["battle"]["report"], patterns["titles"]["bossBattle"], patterns["titles"]["bossMulti"], patterns["titles"]["party"], patterns["quest"]["events"]["bossBattle"]["prompt"]["notEnoughBossTickets"] };
        var isBossMulti = false;
        var numBattles = 0;
        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(loopPatterns);
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("farmEventBossLoop: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("farmEventBossLoop: click quest events");
                    macroService.ClickPattern(patterns["quest"]["events"]);
                    break;
                case "titles.events":
                    logger.LogInformation("farmEventBossLoop: start farm");
                    var bossBattleResult = macroService.PollPattern(patterns["quest"]["events"]["bossBattle"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["quest"]["events"]["bossBattle"]["prompt"]["chooseBattleStyle"], patterns["titles"]["bossBattle"] } });
                    if (bossBattleResult.PredicatePath == "quest.events.bossBattle.prompt.chooseBattleStyle")
                    {
                        macroService.PollPattern(patterns["quest"]["events"]["bossBattle"]["multi"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["bossMulti"] });
                    }
                    break;
                case "titles.bossBattle":
                    var dailyAttemptResult = macroService.FindPattern(patterns["quest"]["events"]["bossBattle"]["dailyAttempt"]);
                    if (dailyAttemptResult.IsSuccess)
                    {
                        macroService.PollPattern(patterns["quest"]["events"]["bossBattle"]["expert"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["prepare"] });
                        macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    }
                    else
                    {
                        var hardResult = macroService.PollPattern(patterns["quest"]["events"]["bossBattle"]["hard"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["battle"]["prepare"], patterns["quest"]["events"]["bossBattle"]["notEnoughTickets"] } });
                        if (hardResult.PredicatePath == "quest.events.bossBattle.notEnoughTickets")
                        {
                            return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
                        }
                        var currentCost = int.Parse(macroService.GetText(patterns["quest"]["events"]["bossBattle"]["cost"]));
                        for (var i = 0; macroService.IsRunning && i < 2; i++)
                        {
                            var addCostDisabledResult = macroService.FindPattern(patterns["quest"]["events"]["bossBattle"]["addCost"]["disabled"]);
                            if (addCostDisabledResult.IsSuccess)
                            {
                                break;
                            }
                            macroService.ClickPattern(patterns["quest"]["events"]["bossBattle"]["addCost"]);
                            Sleep(500);
                            currentCost = int.Parse(macroService.GetText(patterns["quest"]["events"]["bossBattle"]["cost"]));
                        }
                        logger.LogDebug($"currentCost: {currentCost}");
                        if (currentCost == 1)
                        {
                            return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
                        }
                        var prepareResult = macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["titles"]["party"], patterns["quest"]["events"]["bossBattle"]["prompt"]["notEnoughBossTickets"] } });
                        if (prepareResult.PredicatePath == "events.bossBattle.prompt.notEnoughBossTickets")
                        {
                            return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
                        }
                        macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    }
                    break;
                case "titles.bossMulti":
                    {
                        logger.LogInformation("farmEventBossLoop: max cost");
                        isBossMulti = true;
                        macroService.PollPattern(patterns["quest"]["events"]["bossBattle"]["extreme"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["prepare"] });
                        var currentCost = int.Parse(macroService.GetText(patterns["quest"]["events"]["bossBattle"]["cost"]));
                        for (var i = 0; macroService.IsRunning && i < 2; i++)
                        {
                            var addCostDisabledResult = macroService.FindPattern(patterns["quest"]["events"]["bossBattle"]["addCost"]["disabled"]);
                            if (addCostDisabledResult.IsSuccess)
                            {
                                break;
                            }
                            macroService.ClickPattern(patterns["quest"]["events"]["bossBattle"]["addCost"]);
                            Sleep(500);
                            currentCost = int.Parse(macroService.GetText(patterns["quest"]["events"]["bossBattle"]["cost"]));
                        }
                        logger.LogDebug($"currentCost: {currentCost}");
                        if (currentCost == 1)
                        {
                            return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
                        }
                        var prepareResult = macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["titles"]["party"], patterns["quest"]["events"]["bossBattle"]["prompt"]["notEnoughBossTickets"] } });
                        if (prepareResult.PredicatePath == "events.bossBattle.prompt.notEnoughBossTickets")
                        {
                            return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
                        }
                    }
                    break;
                case "titles.party":
                    logger.LogInformation("farmEventBossLoop: select party");
                    var targetPartyName = settings["party"]["eventBoss"].GetValue<string>();
                    logger.LogDebug($"targetPartyName: {targetPartyName}");
                    if (targetPartyName == "recommendedElement")
                    {
                        selectPartyByRecommendedElement(isBossMulti ? -425 : 0);    // Recommended Element icons are shifted by 425 to the left of expected location
                    }
                    //else if (!(selectParty(targetPartyName))) {
                    //	result = $"targetPartyName not found: {targetPartyName}";
                    //	done = true;
                    //	break;
                    //}
                    Sleep(500);
                    macroService.PollPattern(new PatternNode[] { patterns["battle"]["joinRoom"], patterns["battle"]["begin"] }, new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    numBattles++;
                    break;
                case "battle.report":
                    logger.LogInformation("farmEventBossLoop: leave room");
                    var endResult = macroService.FindPattern(new PatternNode[] { patterns["battle"]["next"], patterns["battle"]["replay"], patterns["battle"]["next3"] });
                    logger.LogDebug("endResult.Path: " + endResult.Path);
                    switch (endResult.Path)
                    {
                        case "battle.next":
                            macroService.PollPattern(patterns["battle"]["leaveRoom"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["battle"]["next"], PredicatePattern = patterns["titles"]["bossMulti"] });
                            break;
                        case "battle.replay":
                            macroService.PollPattern(patterns["battle"]["replay"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["battle"]["next"], patterns["battle"]["affinityLevelUp"] }, PredicatePattern = patterns["battle"]["replay"]["prompt"] });
                            Sleep(500);
                            macroService.PollPattern(patterns["battle"]["replay"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = new PatternNode[] { patterns["battle"]["report"] } });
                            numBattles++;
                            break;
                        case "battle.next3":
                            macroService.PollPattern(patterns["battle"]["next3"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["bossBattle"] });
                            break;
                    }
                    break;
                case "quest.events.bossBattle.prompt.notEnoughBossTickets":
                    return JsonSerializer.Serialize(new { numBattles = numBattles, message = "Not enough boss tickets..." }, new JsonSerializerOptions() { WriteIndented = true });
            }

            Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return JsonSerializer.Serialize(new { numBattles = numBattles }, new JsonSerializerOptions() { WriteIndented = true} );
    }
}
