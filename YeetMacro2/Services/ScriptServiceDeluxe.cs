using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.Services.Scripts.Eversoul;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.ViewModels;
using YeetMacro2.Services.Scripts.KonosubaFD;
using YeetMacro2.Data.Services;
using AutoMapper;

namespace YeetMacro2.Services;
public class ScriptServiceDeluxe : IScriptService
{
    ILogger _logger;
    public IMapper _mapper;
    IToastService _toastService;
    public bool InDebugMode { get; set; }
    MacroService _macroService;
    public ScriptServiceDeluxe(ILogger<ScriptServiceDeluxe> logger, IMapper mapper, IToastService toastService, MacroService macroService)
    {
        _logger = logger;
        _mapper = mapper;
        _toastService = toastService;
        _macroService = macroService;
    }

    public void RunScript(ScriptNode targetScript, ScriptNodeManagerViewModel scriptNodeManger, MacroSet macroSet, PatternNodeManagerViewModel patternNodeManager, SettingNodeManagerViewModel settingNodeManager, Action<string> onScriptFinished)
    {
        if (_macroService.IsRunning) return;

        string result = String.Empty;
        _macroService.IsRunning = true;
        _macroService.InDebugMode = InDebugMode;
        try
        {
            object myScripts = null;

            var patterns = (PatternNodeViewModel)patternNodeManager.Root;
            var settings = (ParentSettingViewModel)settingNodeManager.Root;
            if (macroSet.Name == "Eversoul")
            {
                myScripts = new EversoulScripts(_logger, _macroService, patterns, settings);
            }
            else if (macroSet.Name == "Konosuba FD")
            {
                myScripts = new KonosubaFDScripts(_logger, _macroService, patterns, settings);
            }
            else
            {
                throw new Exception($"Did not expect macroSetName: {macroSet.Name}");
            }

            result = (string)ReflectionHelper.MethodInfoCollection[myScripts.GetType()][targetScript.Name].Invoke(myScripts, new object[0]);

            _toastService.Show(_macroService.IsRunning ? "Script finished..." : "Script stopped...");
        }
        catch (Exception ex)
        {
            _toastService.Show("Error: " + ex.Message);
            _logger.LogError(ex, $"Script Error: {ex.Message}");
        }
        finally
        {
            onScriptFinished?.Invoke(result);
            _macroService.IsRunning = false;
            _macroService.Reset();
        }
    }

    public void Stop()
    {
        _macroService.IsRunning = false;
    }
}