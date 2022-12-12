using System.Windows.Input;

namespace YeetMacro2.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private bool _isProjectionServiceEnabled, _isAccessibilityEnabled, _isAppearing;
    //private IAccessibilityService _accessibilityService;
    //private IWindowManagerService _windowManagerService;
    //private INodeService<PatternNode, PatternNode> _nodeService;
    public ICommand ToggleProjectionServiceCommand { get; }
    public ICommand ToggleAccessibilityPermissionsCommand { get; }
    public ICommand OnAppearCommand { get; }
    public ICommand CopyDbCommand { get; }
    public ICommand DeleteDbCommand { get; }
    public ICommand SavePatternsToJsonCommand { get; }
    public ICommand LoadPatternsFromJsonCommand { get; }

    public bool IsProjectionServiceEnabled
    {
        get => _isProjectionServiceEnabled;
        set => SetProperty(ref _isProjectionServiceEnabled, value);
    }
    public bool IsAccessibilityEnabled
    {
        get => _isAccessibilityEnabled;
        set => SetProperty(ref _isAccessibilityEnabled, value);
    }

    //public HomeViewModel() { }

    //public HomeViewModel(IAccessibilityService accessibilityService, IWindowManagerService windowManagerService,
    //    INodeService<PatternNode, PatternNode> nodeService)
    public HomeViewModel()
    {
        Title = "Home";

        ToggleProjectionServiceCommand = new Command(ToggleProjectionService);
        ToggleAccessibilityPermissionsCommand = new Command(ToggleAccessibilityPermissions);
        OnAppearCommand = new Command(OnAppear);
        CopyDbCommand = new Command(CopyDb);
        DeleteDbCommand = new Command(DeleteDb);
        SavePatternsToJsonCommand = new Command(SavePatternsToJson);
        LoadPatternsFromJsonCommand = new Command(LoadPatternsFromJson);
        //_accessibilityService = accessibilityService;
        //_windowManagerService = windowManagerService;
        //_nodeService = nodeService;
    }

    private void SavePatternsToJson()
    {
        //var patternsViewModel = App.GetService<PatternTreeViewViewModel>();
        //var str = JsonSerializer.Serialize(patternsViewModel.Root, new JsonSerializerOptions { WriteIndented = true });
        //var picturesPath = "/storage/emulated/0/Pictures";
        //var toPath = Path.Combine(picturesPath, "patterns.json");
        //File.WriteAllText(toPath, str);
    }

    private void LoadPatternsFromJson()
    {
        //var currentAssembly = Assembly.GetExecutingAssembly();
        //using (var stream = currentAssembly.GetManifestResourceStream("XamarinApp.Testing.patterns.json"))
        //using (var reader = new StreamReader(stream))
        //{
        //    var json = reader.ReadToEnd();
        //    var root = ProxyViewModel.Create(JsonSerializer.Deserialize<PatternNode>(json));
        //    root.Nodes = ProxyViewModel.CreateCollection(root.Nodes, pn => new { pn.Nodes, pn.Patterns, pn.UserPatterns });
        //    var patternsViewModel = App.GetService<PatternTreeViewViewModel>();
        //    patternsViewModel.Root = root;
        //    var persistedRoot = _nodeService.GetRoot();
        //    foreach (var node in root.Nodes)
        //    {
        //        _nodeService.Insert(node);
        //    }
        //}
    }

    private async void CopyDb()
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
        {
            var fromPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
            var picturesPath = "/storage/emulated/0/Pictures";
            var toPath = Path.Combine(picturesPath, "yeetmacro.db3");

            File.Copy(fromPath, toPath, true);
        }
    }

    private void DeleteDb()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
        File.Delete(dbPath);
    }

    public void ToggleProjectionService()
    {
        //if (IsProjectionServiceEnabled)
        //{
        //    _windowManagerService.StartProjectionService();
        //}
        //else
        //{
        //    _windowManagerService.StopProjectionService();
        //}
    }

    public void ToggleAccessibilityPermissions()
    {
        if (_isAppearing) return;

        //if (IsAccessibilityEnabled)
        //{
        //    _windowManagerService.RequestAccessibilityPermissions();
        //}
        //else
        //{
        //    _windowManagerService.RevokeAccessibilityPermissions();
        //}
    }

    private void OnAppear()
    {
        //_isAppearing = true;
        //IsProjectionServiceEnabled = _windowManagerService.ProjectionServiceEnabled;
        //IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        //_isAppearing = false;

        //IsProjectionServiceEnabled = true;
        //_windowManagerService.Show(WindowView.PatternsTreeView);
    }
}
