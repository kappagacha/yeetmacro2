using Jint;
using Jint.Runtime;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Services;

public interface IScriptsService
{
    void RunScript(string script);
    void Stop();
}

public class ScriptsService : IScriptsService
{
    CancellationTokenSource _cancellationTokenSource;
    MacroManagerViewModel _macroManagerViewModel;
    LogViewModel _logViewModel;
    IMacroService _macroService;
    IToastService _toastService;
    public ScriptsService(MacroManagerViewModel macroManagerViewModel, LogViewModel logViewModel, IMacroService macroService, IToastService toastService)
    {
        _macroManagerViewModel = macroManagerViewModel;
        _logViewModel = logViewModel;
        _macroService = macroService;
        _toastService = toastService;
    }

    public void RunScript(string script)
    {
        Task.Run(async () =>
        {
            var engine = await CreateEngine();

            try
            {
                engine.Execute($"{script}()", "script.js");
                _toastService.Show("Script started...");
            }
            catch (ExecutionCanceledException)
            {
                _toastService.Show("Script stopped...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _toastService.Show("Error: " + ex.Message);
            }
        });
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task<Engine> CreateEngine()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = new CancellationTokenSource();

        var treeViewViewModel = _macroManagerViewModel.PatternTree;
        var scripts = _macroManagerViewModel.Scripts.Scripts;
        await treeViewViewModel.WaitForInitialization();
        var patterns = treeViewViewModel.Root.BuildDynamicObject();
        
        var engine = new Engine(opt => opt.CancellationToken(_cancellationTokenSource.Token))
                .SetValue("log", new Action<object>(Console.WriteLine))
                //.SetValue("logSomething", new Action<object>((obj) => {
                //    //var elapsed = stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
                //    var message = $"[*****YeetMacro*****] {elapsed} Doing something: " + Guid.NewGuid();
                //    _logViewModel.Message = message;
                //    Console.WriteLine(message);
                //}))
                .SetValue("sleep", new Action<int>((ms) => Thread.Sleep(ms)))
                .SetValue("patterns", patterns)
                .SetValue("macroService", _macroService.BuildDynamicObject(_cancellationTokenSource.Token));

        foreach (var script in scripts)
        {
            var func = script.Text.StartsWith("function") ? script.Text : 
                $@"function {script.Name}() {{
                    {script.Text}
                }}";

            engine.Execute(func);
        }

        return engine;
    }
}
