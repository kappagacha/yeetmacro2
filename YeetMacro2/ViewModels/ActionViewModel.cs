using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;
using YeetMacro2.Services;

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
    //IMacroService _macroService;
    //IBackgroundWorker _backgroundWorker;
    CancellationTokenSource _cancellationTokenSource;
    public ActionViewModel()
    {
    }

    //public ActionViewModel(IWindowManagerService windowManagerService, IAccessibilityService accessibilityService,
    //    IMediaProjectionService mediaProjectionService, IToastService toastService, IMacroService macroService,
    //    IBackgroundWorker backgroundWorker)
    public ActionViewModel(IWindowManagerService windowManagerService, IToastService toastService, IMediaProjectionService mediaProjectionService,
        IAccessibilityService accessibilityService)
    {
        _windowManagerService = windowManagerService;
        _toastService = toastService;
        _mediaProjectionService = mediaProjectionService;
        _accessibilityService = accessibilityService;
        //_macroService = macroService;
        //_backgroundWorker = backgroundWorker;
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

        //var title = patterns.title;
        //var home = patterns.title.home;
        //var result = macroService.FindPattern(patterns.title.home);
        //var yy = result;
        //macroService.FindPattern(patterns.home);
        //var result = macroService.FinePattern(patterns.home);
        //Console.WriteLine(result.IsSuccess);
        //Console.WriteLine(result.Location);

        //while (true)
        //{
        //    var result = macroService.FindPattern(patterns.title.home);
        //    if (result.IsSuccess)
        //    {
        //        macroService.ClickPattern(patterns.tab.home);
        //    }
        //}

        //run asynchronously then cancel with click
        //https://stackoverflow.com/questions/58934233/c-sharp-how-to-stop-a-async-task-on-button-click

        //foreground service?
        //https://stackoverflow.com/questions/58107522/how-to-create-a-never-ending-background-service-in-xamarin-forms

        //try out different background execution

        //var topLeft = _windowManagerService.GetTopLeft();
        //Console.WriteLine($"{topLeft.x}x {topLeft.y}");
        //_toastService.MakeText($"{topLeft.x}x {topLeft.y}");
        //_windowManagerService.DrawRectangle(0, 0, 1920, 1080);
        //_windowManagerService.DrawRectangle(120, 0, 1920, 1080);
        //_windowManagerService.DrawRectangle(210, 0, 1920, 1080);
        //_windowManagerService.DrawRectangle(330, 10, 1920, 1080);
        //_windowManagerService.DrawRectangle(270, 10, 1920, 1080);

        //dynamic test = new ExpandoObject();
        //test.myFunc = new Func<int, int, int>((x, y) => x* y);

        //var something = test.myFunc(10, 10);

        //var x = 1;
        // TODO: make MacroService
        // FindPattern
        // DoClick

        //var engine = new Jint.Engine()
        //    .SetValue("log", new Action<object>(Console.WriteLine))
        //    .SetValue("root", root);

        //engine.Execute(@"
        //  function hello() {
        //    log(root.home.node.name);

        //  };

        //  hello();
        //");

        //var vm = App.GetService<PatternTreeViewViewModel>();
        //vm.ClickPatternCommand.Execute(null);
        //_accessibilityService.DoClick(100, 100);

        //var name = "hello.jpeg";
        //var picturesPath = "/storage/emulated/0/Pictures";
        //var filePath = System.IO.Path.Combine(picturesPath, name);

        //var imageStream = await _projectionService.GetCurrentImageStream();
        //byte[] bArray = new byte[imageStream.Length];
        //using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
        //{
        //    using (imageStream)
        //    {
        //        imageStream.Read(bArray, 0, (int)imageStream.Length);
        //    }
        //    int length = bArray.Length;
        //    fs.Write(bArray, 0, length);
        //}
    }

    private void RunScript()
    {
        Console.WriteLine("[*****YeetMacro*****] RunScript");
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = new CancellationTokenSource();
        var logViewModel = ServiceHelper.GetService<LogViewModel>();

        var work = new Action(async () =>
        {
            State = ActionState.Running;
            //var patternsModel = App.GetService<PatternTreeViewViewModel>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var elapsed = stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
                    var message = $"[*****YeetMacro*****] {elapsed} Doing something: " + Guid.NewGuid();
                    var imageStream = await _mediaProjectionService.GetCurrentImageStream();
                    _accessibilityService.DoClick(100, 50);
                    logViewModel.Message = message;
                    Console.WriteLine(message);
                    //_toastService.Show(message);

                    //_macroService.FindPattern(patternsModel.Root.Nodes.First().Nodes.First().Patterns.First());
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {

                }
            }

            //var macroService = _macroService.BuildDynamicObject();
            //var patterns = patternsModel.Root.BuildDynamicObject();

            //Engine engine = new Engine(opt => opt.CancellationToken(_cancellationTokenSource.Token))
            //    .SetValue("log", new Action<object>(Console.WriteLine))
            //    .SetValue("logSomething", new Action<object>((obj) => Console.WriteLine("[*****YeetMacro*****] Doing something: " + Guid.NewGuid())))
            //    .SetValue("sleep", new Action<int>((ms) => Thread.Sleep(ms)))
            //    .SetValue("patterns", patterns)
            //    .SetValue("macroService", macroService);


            ////https://medium.com/nerd-for-tech/background-work-in-xamarin-forms-part-1-xamarin-android-63f629e73f9
            //var script = @"
            //        var loopPatterns = [patterns.title.home, patterns.title.quest, patterns.title.events];
            //        while (true) {
            //            logSomething();
            //            var result = macroService.FindPattern(loopPatterns);
            //            if (result.IsSuccess)
            //            {
            //                log('[*****YeetMacro*****]' + result.Path);
            //                switch(result.Path) {
            //                    case 'title.home':
            //                        log('[*****YeetMacro*****] Click Start');
            //                        macroService.ClickPattern(patterns.tabs.quest);
            //                        log('[*****YeetMacro*****] Click End');
            //                        sleep(1000);
            //                    break;
            //                    case 'title.quest':
            //                        log('[*****YeetMacro*****] Click Start');
            //                        macroService.ClickPattern(patterns.quest.events);
            //                        log('[*****YeetMacro*****] Click End');
            //                        sleep(1000);
            //                    break;
            //                    case 'title.events':
            //                        log('[*****YeetMacro*****] Click Start');
            //                        macroService.ClickPattern(patterns.events.quest);
            //                        log('[*****YeetMacro*****] Click End');
            //                        sleep(1000);
            //                    break;
            //                }
            //            }
            //            sleep(200);
            //        }
            //    ";

            //engine.Execute(script, "script.js");

            //var loopPatterns = new[] { patterns.title.home, patterns.title.quest, patterns.title.events };
            //while (true)
            //{
            //    var result = macroService.FindPattern(loopPatterns);
            //    if (result.IsSuccess)
            //    {
            //        Console.WriteLine(result.Path);
            //        switch (result.Path)
            //        {
            //            case "title.home":
            //                //Console.WriteLine(patterns.tabs.quest);
            //                //macroService.ClickPattern(patterns.tabs.quest);
            //                Thread.Sleep(1000);
            //                break;
            //            case "title.quest":
            //                //Console.WriteLine(patterns.quest.events);
            //                //macroService.ClickPattern(patterns.quest.events);
            //                Thread.Sleep(1000);
            //                break;
            //            case "title.events":
            //                //Console.WriteLine(patterns.events.quest);
            //                //macroService.ClickPattern(patterns.events.quest);
            //                Thread.Sleep(1000);
            //                break;
            //        }
            //    }
            //    Thread.Sleep(400);
            //}
        });
        //_backgroundWorker.StartWorker(work);


        //try
        //{
        //    var t = new Thread(() =>
        //    {
        //        Thread.CurrentThread.IsBackground = true;
        //        work();
        //    });
        //    _cancellationTokenSource.Token.Register(t.Abort);
        //    t.Start();
        //}
        //catch (Exception ex)
        //{

        //}

        try
        {
            Task.Factory.StartNew(work, _cancellationTokenSource.Token);
            //Parallel.ForEach(new List<int>() { 0 }, work);
        }
        catch (Exception ex)
        {

        }

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
        //if (await Permissions.RequestAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
        //{
        //    var imageStream = await _mediaProjectionService.GetCurrentImageStream();

        //    var name = "hello.jpeg";
        //    var picturesPath = "/storage/emulated/0/Pictures";
        //    var filePath = System.IO.Path.Combine(picturesPath, name);
        //    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
        //    {
        //        imageStream.CopyTo(fs);
        //    }
        //}
            

        if (!IsMoving)
        {
            _windowManagerService.Show(WindowView.ActionMenuView);
        }
    }
}
