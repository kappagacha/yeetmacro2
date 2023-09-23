using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string claimJobs()
    {
        // patterns["jobs"]["notification"]
        var looppatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["job"] };
        while (macroService.IsRunning)
        {
            var loopResult = macroService.PollPattern(looppatterns);
            switch (loopResult.Path)
            {
                case "titles.home":
                    logger.LogInformation("farmEventLoop: click jobs");
                    macroService.ClickPattern(patterns["jobs"]);
                    break;
                case "titles.job":
                    logger.LogInformation("farmEventLoop: click acceptAll");
                    var acceptAllResult = macroService.FindPattern(new PatternNode[] { patterns["jobs"]["acceptAll"]["enabled"], patterns["jobs"]["acceptAll"]["disabled"] });
                    if (acceptAllResult.IsSuccess && acceptAllResult.Path == "jobs.acceptAll.enabled")
                    {
                        logger.LogInformation("farmEventLoop: jobs.acceptAll.enabled");
                        macroService.PollPattern(patterns["jobs"]["acceptAll"]["enabled"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["jobs"]["prompt"]["ok"] });
                        macroService.PollPattern(patterns["jobs"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["titles"]["job"] });
                    }
                    else if (acceptAllResult.IsSuccess)
                    {       // jobs.acceptAll.disabled
                        logger.LogInformation("farmEventLoop: jobs.acceptAll.disabled");
                        return String.Empty;
                    }
                    break;
            }

            Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }
}
