using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string doEventStory()
    {
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["quest"], patterns["titles"]["events"], patterns["battle"]["report"] };
        while (macroService.IsRunning)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
                case "titles.home":
                    logger.LogInformation("doEventStory: click tab quest");
                    macroService.ClickPattern(patterns["tabs"]["quest"]);
                    break;
                case "titles.quest":
                    logger.LogInformation("doEventStory: click quest events");
                    macroService.ClickPattern(patterns["quest"]["events"]);
                    break;
                case "titles.events":
                    logger.LogInformation("doEventStory: find NEW");
                    macroService.PollPattern(patterns["quest"]["events"]["quest"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["quest"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["quest"]["events"]["new"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["watchLater"], PredicatePattern = patterns["battle"]["prepare"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    break;
                case "battle.report":
                    logger.LogInformation("doEventStory: battle report");
                    macroService.PollPattern(patterns["battle"]["next"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["battle"]["next2"], PredicatePattern = patterns["titles"]["events"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["quest"]["events"]["new"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["prompt"]["watchLater"], PredicatePattern = patterns["battle"]["prepare"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["prepare"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["titles"]["party"] });
                    new System.Threading.ManualResetEvent(false).WaitOne(500);
                    macroService.PollPattern(patterns["battle"]["begin"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["battle"]["report"] });
                    break;
            }

            new System.Threading.ManualResetEvent(false).WaitOne(1_000);
        }
        logger.LogInformation("Done...");

        return String.Empty;
    }
}
