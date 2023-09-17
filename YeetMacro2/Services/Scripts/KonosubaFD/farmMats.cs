using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public string farmMats()
    {
        var done = false;
        var loopPatterns = new PatternNode[] { patterns["titles"]["home"], patterns["titles"]["smithy"], patterns["titles"]["craft"], patterns["skipAll"]["title"] };

        while (macroService.IsRunning && !done)
        {
            var result = macroService.PollPattern(loopPatterns);
            switch (result.Path)
            {
                case "titles.home":
                    logger.LogInformation("farmMats: click smithy tab");
                    macroService.ClickPattern(patterns["tabs"]["smithy"]);
                    break;
                case "titles.smithy":
                    logger.LogInformation("farmMats: click craft");
                    macroService.ClickPattern(patterns["smithy"]["craft"]);
                    break;
                case "titles.craft":
                    macroService.PollPattern(patterns["smithy"]["craft"]["jewelry"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["smithy"]["craft"]["jewelry"]["list"] });
                    Thread.Sleep(500);
                    macroService.PollPattern(patterns["smithy"]["craft"]["materials"]["archAngelFeather"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["smithy"]["prompt"]["howToAcquire"] });
                    Thread.Sleep(1_000);
                    macroService.PollPattern(patterns["smithy"]["prompt"]["skipAll"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["title"] });
                    Thread.Sleep(500);
                    break;
                case "skipAll.title":
                    logger.LogInformation("farmMats: farm extreme levels");
                    farmMat(new PatternNode[] { patterns["skipAll"]["search"]["select"]["mithrilOre"], patterns["skipAll"]["search"]["select"]["yggdrasilBranch"], patterns["skipAll"]["search"]["select"]["platinumOre"] }, 500, 1);
                    Thread.Sleep(1_000);
                    logger.LogInformation("farmMats: farm skyDragonScale");
                    farmMat(new PatternNode[] { patterns["skipAll"]["search"]["select"]["skyDragonScale"] }, 500, 3);
                    done = true;
                    break;
            }

            Thread.Sleep(1_000);
        }
        logger.LogInformation("Done...");
        return String.Empty;
    }

    public void farmMat(PatternNode[] targetMats, int staminaCost, int numSkips)
    {
        var offset = macroService.CalcOffset(patterns["titles"]["home"]);
        macroService.PollPattern(patterns["skipAll"]["material"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["search"] });
        Thread.Sleep(500);
        var filterOffResult = macroService.FindPattern(patterns["skipAll"]["search"]["filter"]["off"]);
        if (filterOffResult.IsSuccess)
        {
            macroService.PollPattern(patterns["skipAll"]["search"]["filter"]["off"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["search"]["filter"] });
            Thread.Sleep(500);
            var checkResult = macroService.FindPattern(patterns["skipAll"]["search"]["filter"]["check"], new FindOptions() { Limit = 4 });
            foreach (var point in checkResult.Points)
            {
                if (point.X < offset.X + 400.0) continue;       // skip 4 stars
                macroService.DoClick(point);
                Thread.Sleep(250);
            }
            macroService.PollPattern(patterns["skipAll"]["search"]["filter"]["close"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["search"] });
            Thread.Sleep(500);
        }

        macroService.PollPattern(patterns["skipAll"]["search"]["select"]["check"], new PollPatternFindOptions() { DoClick = true, InversePredicatePattern = patterns["skipAll"]["search"]["select"]["check"] });
        foreach (var mat in targetMats)
        {
            var matResult = macroService.FindPattern(mat);
            if (matResult.IsSuccess)
            {
                var matCheckPattern = PatternNodeManagerViewModel.CloneNode(patterns["skipAll"]["search"]["select"]["check"]);
                matCheckPattern.Path += $"_{mat.Path}";
                foreach (var p in matCheckPattern.Patterns)
                {
                    p.Rect = new Rect(matResult.Point.X - 115.0, matResult.Point.Y - 105.0, 110.0, 85.0);
                    p.OffsetCalcType = OffsetCalcType.None;
                }
                macroService.PollPattern(mat, new PollPatternFindOptions() { DoClick = true, PredicatePattern = matCheckPattern, IntervalDelayMs = 1_000 });
            }
        }

        Thread.Sleep(500);
        macroService.PollPattern(patterns["skipAll"]["search"]["button"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["title"] });
        Thread.Sleep(2000);

        var currentStaminaCost = int.Parse(macroService.GetText(patterns["skipAll"]["totalCost"]));
        if (currentStaminaCost < staminaCost)
        {
            macroService.PollPattern(patterns["skipAll"]["addStamina"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["stamina"]["prompt"]["recoverStamina"] });
            macroService.PollPattern(patterns["stamina"]["meat"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["stamina"]["prompt"]["recoverStamina2"] });
            var targetStamina = int.Parse(macroService.GetText(patterns["stamina"]["target"]));
            while (macroService.IsRunning && targetStamina < staminaCost)
            {
                macroService.ClickPattern(patterns["stamina"]["plusOne"]);
                Thread.Sleep(500);
                targetStamina = int.Parse(macroService.GetText(patterns["stamina"]["target"]));
            }
            macroService.PollPattern(patterns["stamina"]["prompt"]["recover"], new PollPatternFindOptions() { DoClick = true, ClickPattern = patterns["stamina"]["prompt"]["ok"], PredicatePattern = patterns["skipAll"]["addMaxSkips"], IntervalDelayMs = 1_000 });
        }

        var maxNumSkips = int.Parse(macroService.GetText(patterns["skipAll"]["maxNumSkips"]));
        while (macroService.IsRunning && maxNumSkips < numSkips)
        {
            macroService.ClickPattern(patterns["skipAll"]["addMaxSkips"]);
            Thread.Sleep(500);
            maxNumSkips = int.Parse(macroService.GetText(patterns["skipAll"]["maxNumSkips"]));
        }

        macroService.PollPattern(patterns["skipAll"]["button"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["prompt"]["ok"] });
        Thread.Sleep(1_000);
        macroService.PollPattern(patterns["skipAll"]["prompt"]["ok"], new PollPatternFindOptions() { DoClick = true, PredicatePattern = patterns["skipAll"]["skipComplete"] });
        macroService.PollPattern(patterns["skipAll"]["skipComplete"], new PollPatternFindOptions() { DoClick = true, ClickPattern = new PatternNode[] { patterns["skipAll"]["prompt"]["ok"], patterns["branchEvent"]["availableNow"], patterns["branchEvent"]["playLater"], patterns["prompt"]["playerRankUp"] }, PredicatePattern = patterns["skipAll"]["title"] });
    }
}