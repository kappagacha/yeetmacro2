using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    bool _isProjectionServiceEnabled;
    [ObservableProperty]
    bool _isAccessibilityEnabled;
    [ObservableProperty]
    bool _isAppearing;
    //private IAccessibilityService _accessibilityService;
    private IWindowManagerService _windowManagerService;
    //private INodeService<PatternNode, PatternNode> _nodeService;

    public HomeViewModel() { }

    //public HomeViewModel(IAccessibilityService accessibilityService, IWindowManagerService windowManagerService,
    //    INodeService<PatternNode, PatternNode> nodeService)
    public HomeViewModel(IWindowManagerService windowManagerService)
    {
        //Title = "Home";
        
        //_accessibilityService = accessibilityService;
        _windowManagerService = windowManagerService;
        //_nodeService = nodeService;
    }

    [RelayCommand]
    private void SavePatternsToJson()
    {
        //var patternsViewModel = App.GetService<PatternTreeViewViewModel>();
        //var str = JsonSerializer.Serialize(patternsViewModel.Root, new JsonSerializerOptions { WriteIndented = true });
        //var picturesPath = "/storage/emulated/0/Pictures";
        //var toPath = Path.Combine(picturesPath, "patterns.json");
        //File.WriteAllText(toPath, str);
    }

    [RelayCommand]
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

    [RelayCommand]
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

    [RelayCommand]
    private void DeleteDb()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
        File.Delete(dbPath);
    }

    [RelayCommand]
    public void ToggleProjectionService()
    {
        _windowManagerService.Show(WindowView.ActionView);
        //if (IsProjectionServiceEnabled)
        //{
        //    _windowManagerService.StartProjectionService();
        //}
        //else
        //{
        //    _windowManagerService.StopProjectionService();
        //}
    }

    [RelayCommand]
    public void ToggleAccessibilityPermissions()
    {
        if (IsAppearing) return;

        //if (IsAccessibilityEnabled)
        //{
        //    _windowManagerService.RequestAccessibilityPermissions();
        //}
        //else
        //{
        //    _windowManagerService.RevokeAccessibilityPermissions();
        //}
    }

    [RelayCommand]
    public void OnAppear()
    {
        //_isAppearing = true;
        //IsProjectionServiceEnabled = _windowManagerService.ProjectionServiceEnabled;
        //IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        //_isAppearing = false;

        //IsProjectionServiceEnabled = true;
        //_windowManagerService.Show(WindowView.PatternsTreeView);
    }
}
