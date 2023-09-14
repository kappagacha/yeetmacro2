using Microsoft.Extensions.Logging;
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
    }
}
