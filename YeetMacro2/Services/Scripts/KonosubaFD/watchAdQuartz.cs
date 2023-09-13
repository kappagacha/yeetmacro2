using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string watchAdQuartz()
    {
        // patterns["ad"]["quartz"]["notification"]
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"] };
        while (macroService.IsRunning)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
                case "titles.home":
                    logger.LogInformation("watchAdQuartz: quartz ad");
                    var quartzAdNotificationResult = macroService.FindPattern(patterns["ad"]["quartz"]["notification"]);
                    if (quartzAdNotificationResult.IsSuccess)
                    {
                        logger.LogInformation("watchAdQuartz: watching ad");
                        logger.LogInformation("watchAdQuartz: ad.quartz.notification");
                        macroService.PollPattern(patterns["ad"]["quartz"]["notification"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["ad"]["prompt"]["ok"] });
                        Thread.Sleep(1_000);
                        logger.LogInformation("watchAdQuartz: poll ad.prompt.ok 1");
                        macroService.PollPattern(patterns["ad"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["ad"]["done"] });
                        Thread.Sleep(1_000);
                        logger.LogInformation("watchAdQuartz: poll ad.done");
                        macroService.PollPattern(patterns["ad"]["done"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["ad"]["prompt"]["youGot"], PredicatePattern = patterns["titles"]["home"] });
                    }
                    return String.Empty;
            }

            Thread.Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}