using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services.Scripts.Eversoul;

public partial class EversoulScripts
{
    ILogger logger;
    MacroService macroService;
    PatternNodeViewModel patterns;
    ParentSettingViewModel settings;

    public EversoulScripts(ILogger logger, MacroService macroService, PatternNodeViewModel patterns, ParentSettingViewModel settings)
    {
        this.logger = logger;
        this.macroService = macroService;
        this.patterns = patterns;
        this.settings = settings;

        resolvePath(patterns);
    }

    public void resolvePath(PatternNode node, string path = "")
    {
        if (node.Name == "root")
        {
            node.Path = "";
        }
        else if (string.IsNullOrWhiteSpace(path))
        {
            node.Path = node.Name;
        }
        else
        {
            node.Path = $"{path}.{node.Name}";
        }

        foreach (var child in node.Nodes)
        {
            resolvePath(child, node.Path);
        }
    }
}
