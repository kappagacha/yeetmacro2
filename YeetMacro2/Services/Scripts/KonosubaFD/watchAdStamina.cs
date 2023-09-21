using Microsoft.Extensions.Logging;
namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string watchAdStamina()
    {
        // patterns["stamina"]["adNotification"]
        //var done = false;
        //var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["stamina"]["prompt"]["recoverStamina"] };
        //while (macroService.IsRunning && !done)
        //{
        //    var result = macroService.PollPattern(loopPatterns);
        //    switch (result.Path)
        //    {
        //        case "titles.home":
        //            logger.LogInformation("watchAdStamina: stamina ad");
        //            macroService.ClickPattern(patterns["stamina"]["add"]);
        //            break;
        //        case "stamina.prompt.recoverStamina":
        //            logger.LogInformation("watchAdStamina: check for stamina adNotification");
        //            var staminaAdNotificationResult = macroService.FindPattern(patterns["stamina"]["adNotification"]);
        //            logger.LogInformation("staminaAdNotificationResult.IsSuccess: " + staminaAdNotificationResult.IsSuccess);
        //            if (!staminaAdNotificationResult.IsSuccess)
        //            {
        //                logger.LogInformation("watchAdStamina: stamina ad notification not detected");
        //                var watchAdResult = macroService.PollPattern(patterns["stamina"]["watchAd"], new PollPatternFindOptions() { DoClick = true, IntervalDelayMs: 1000, PredicatePattern = new PatternNode[] { patterns["prompt"]["dailyRewardLimit"], patterns["stamina"]["adNotification"] } });
        //            if (watchAdResult.PredicatePath == "prompt.dailyRewardLimit")
        //            {
        //                logger.LogInformation("watchAdStamina: daily reward limit");
        //                macroService.PollPattern(patterns["ad"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["stamina"]["prompt"]["recoverStamina"] });
        //                done = true;
        //                break;
        //            }
        //            break;
        //    }

        //    logger.LogInformation("watchAdStamina: watching ad");
        //    logger.LogInformation("watchAdStamina: poll stamina.adNotification");
        //    macroService.PollPattern(patterns["stamina"]["adNotification"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["ad"]["prompt"]["ok"], PredicatePattern = patterns["ad"]["done"] });
        //    Sleep(1_000);
        //    logger.LogInformation("watchAdStamina: poll ad.done");
        //    macroService.PollPattern(patterns["ad"]["done"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["ad"]["prompt"]["ok"] });
        //    Sleep(1_000);
        //    logger.LogInformation("watchAdStamina: poll ad.prompt.ok 2");
        //    macroService.PollPattern(patterns["ad"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["stamina"]["add"] });

        //    done = true;
        //    break;
        //}

        //Sleep(1_000);
        //}
        //logger.LogInformation("Done...");
        return String.Empty;
    }
}
