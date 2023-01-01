using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using YeetMacro2.Services;
using Jint;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.ViewModels;

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
            var treeViewViewModel = ServiceHelper.GetService<MacroManagerViewModel>().PatternTree;
            State = ActionState.Running;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await treeViewViewModel.WaitForInitialization();
            var macroService = _macroService.BuildDynamicObject();
            var patterns = treeViewViewModel.Root.BuildDynamicObject();

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
var loopPatterns = [patterns.titles.home];
while (true) {
	logSomething();
	var result = macroService.FindPattern(loopPatterns);
	if (result.IsSuccess) {
		log('[*****YeetMacro*****]' + result.Path);
		switch(result.Path) {
			case 'titles.home':
				log('[*****YeetMacro*****] Click Start');
				macroService.ClickPattern(patterns.tabs.quest);
				log('[*****YeetMacro*****] Click End');
				sleep(1000);
			break;
			case 'titles.quest':
				log('[*****YeetMacro*****] Click Start');
				macroService.ClickPattern(patterns.quest.events);
				log('[*****YeetMacro*****] Click End');
				sleep(1000);
			break;
			case 'titles.events':
				log('[*****YeetMacro*****] Click Start');
				macroService.ClickPattern(patterns.events.quest);
				log('[*****YeetMacro*****] Click End');
				sleep(1000);
			break;
		}
	}	
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
