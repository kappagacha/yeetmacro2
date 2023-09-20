using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services.Scripts.KonosubaFD;
public partial class KonosubaFDScripts
{
    public bool selectPartyByRecommendedElement(double xOffset = 0.0)
    {
        var elementpatterns = new string[] { "none", "fire", "water", "lightning", "earth", "wind", "light", "dark" }
            .Select(e => (PatternNode)patterns["party"]["recommendedElement"][e]).ToArray();
        if (xOffset != 0)
        {
            elementpatterns = elementpatterns.Select(e =>
            {
                var clone = PatternNodeManagerViewModel.CloneNode(e);
                clone.Path += $"_xOffset{xOffset}";
                foreach (var p in clone.Patterns)
                {
                    p.Rect = p.Rect.Offset(xOffset, 0.0);
                }
                return clone;
            }).ToArray();
        }
        var elementResult = macroService.PollPattern(elementpatterns);
        logger.LogInformation($"selectPartyByRecommendedElement: {elementResult.Path}");
        if (!elementResult.IsSuccess) return false;
        var targetElement = elementResult.Path.Split(".").Last();
        logger.LogDebug($"targetElement: {targetElement}");
        if (xOffset != 0)
        {
            targetElement = targetElement.Split("_").First();
        }
        logger.LogDebug($"targetElement2: {targetElement}");
        var targetElementName = settings["party"]["recommendedElement"][targetElement].GetValue<string>();
        logger.LogDebug($"targetElementName: {targetElementName}");
        if (String.IsNullOrWhiteSpace(targetElementName))
        {
            logger.LogDebug($"Could not find targetElementName for {targetElement} in settings...");
            return false;
        }
        return selectParty(targetElementName);
    }

    public bool selectParty(string targetPartyName)
    {
        logger.LogInformation($"selectParty: {targetPartyName}");
        var currentParty = macroService.GetText(patterns["party"]["name"], targetPartyName);
        var numScrolls = 0;
        while (macroService.IsRunning && currentParty != targetPartyName && numScrolls < 20)
        {
            scrollRight();
            numScrolls++;
            logger.LogDebug($"numScrolls: {numScrolls}");
            currentParty = macroService.GetText(patterns["party"]["name"], targetPartyName);
            logger.LogDebug($"currentParty: {currentParty}");
            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }

        return numScrolls == 20 ? false : true;
    }

    public void scrollRight()
    {
        var currentX = Math.Floor(macroService.FindPattern(patterns["party"]["slot"]).Point.X);
        var prevX = currentX;
        while (macroService.IsRunning && currentX == 0)
        {
            currentX = Math.Floor(macroService.FindPattern(patterns["party"]["slot"]).Point.X);
            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }
        while (macroService.IsRunning && (currentX == 0 || currentX == prevX))
        {
            logger.LogDebug($"currentX: {currentX}, prevX: {prevX}");
            macroService.ClickPattern(patterns["party"]["scrollRight"]);
            new System.Threading.ManualResetEvent(false).WaitOne(500);
            currentX = Math.Floor(macroService.FindPattern(patterns["party"]["slot"]).Point.X);
        }
    }
}
