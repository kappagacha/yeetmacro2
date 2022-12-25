using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using YeetMacro2.Services;
using Jint;

namespace YeetMacro2.ViewModels;

public enum ActionState
{
    Stopped,
    Running
}

public partial class ActionViewModel : ObservableObject, IMovable
{
    [ObservableProperty]
    ActionState _state;
    public bool IsMoving { get; set; }

    IWindowManagerService _windowManagerService;
    IToastService _toastService;
    IMediaProjectionService _mediaProjectionService;
    IAccessibilityService _accessibilityService;
    IMacroService _macroService;
    CancellationTokenSource _cancellationTokenSource;
    public ActionViewModel()
    {
    }

    public ActionViewModel(IWindowManagerService windowManagerService, IToastService toastService, IMediaProjectionService mediaProjectionService,
        IAccessibilityService accessibilityService, IMacroService macroService)
    {
        _windowManagerService = windowManagerService;
        _toastService = toastService;
        _mediaProjectionService = mediaProjectionService;
        _accessibilityService = accessibilityService;
        _macroService = macroService;
    }

    [RelayCommand]
    public void Execute(object o)
    {
        switch (State)
        {
            case ActionState.Stopped:
                RunScript();
                break;
            case ActionState.Running:
                StopScript();
                break;
        }
    }

    private void RunScript()
    {
        Task.Run(async () =>
        {
            Console.WriteLine("[*****YeetMacro*****] RunScript");
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            var logViewModel = ServiceHelper.GetService<LogViewModel>();
            var treeViewViewModel = ServiceHelper.GetService<PatternTreeViewViewModel>();
            State = ActionState.Running;
            var patternsModel = ServiceHelper.GetService<PatternTreeViewViewModel>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await treeViewViewModel.WaitForInitialization();
            var macroService = _macroService.BuildDynamicObject();
            var patterns = patternsModel.Root.BuildDynamicObject();

            Engine engine = new Engine(opt => opt.CancellationToken(_cancellationTokenSource.Token))
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("logSomething", new Action<object>((obj) => {
                    var elapsed = stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
                    var message = $"[*****YeetMacro*****] {elapsed} Doing something: " + Guid.NewGuid();
                    logViewModel.Message = message;
                    Console.WriteLine(message);
                }))
                .SetValue("sleep", new Action<int>((ms) => Thread.Sleep(ms)))
                .SetValue("patterns", patterns)
                .SetValue("macroService", macroService);

            var script = @"
                    while (true) {
                        logSomething();
                        macroService.ClickPattern(patterns.test);
                        sleep(1000);
                    }
                ";

            try
            {
                engine.Execute(script, "script.js");
                _toastService.Show("Script started...");
            }
            catch (Exception ex)
            {
                _toastService.Show("Script stopped...");
            }
        });
    }

    private void StopScript()
    {
        Console.WriteLine("[*****YeetMacro*****] StopScript");
        _cancellationTokenSource.Cancel();
        State = ActionState.Stopped;
    }

    [RelayCommand]
    public void OpenMenu(object o)
    {
        if (!IsMoving)
        {
            _windowManagerService.Show(WindowView.ActionMenuView);
        }
    }
}
