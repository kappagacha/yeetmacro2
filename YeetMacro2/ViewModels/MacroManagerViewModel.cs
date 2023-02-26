using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public partial class MacroManagerViewModel : ObservableObject
{
    IRepository<MacroSet> _macroSetRepository;
    IToastService _toastService;
    INodeService<PatternNode, PatternNode> _nodeService;
    IRepository<Script> _scriptRespository;
    PatternTreeViewViewModelFactory _patternTreeViewFactory;
    ScriptsViewModelFactory _scriptsViewModelFactory;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PatternTree), nameof(Scripts))]
    MacroSet _selectedMacroSet;
    ConcurrentDictionary<int, PatternTreeViewViewModel> _nodeRootIdToPatternTree;
    ConcurrentDictionary<int, ScriptsViewModel> _macroSetIdToScripts;

    public PatternTreeViewViewModel PatternTree
    {
        get
        {
            // Lazy load PattenTree
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(_selectedMacroSet.RootPatternNodeId))
            {
                var patternTree = _patternTreeViewFactory.Create(_selectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(_selectedMacroSet.RootPatternNodeId, patternTree);
            }
            return _nodeRootIdToPatternTree[_selectedMacroSet.RootPatternNodeId];
        }
    }

    public ScriptsViewModel Scripts
    {
        get
        {
            // Lazy load Scripts
            if (_selectedMacroSet == null) return null;
            if (!_macroSetIdToScripts.ContainsKey(_selectedMacroSet.MacroSetId))
            {
                var scripts = _scriptsViewModelFactory.Create(_selectedMacroSet.MacroSetId);
                _macroSetIdToScripts.TryAdd(_selectedMacroSet.MacroSetId, scripts);
            }
            return _macroSetIdToScripts[_selectedMacroSet.MacroSetId];
        }
    }

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository,
        IToastService toastService,
        PatternTreeViewViewModelFactory patternTreeViewFactory,
        ScriptsViewModelFactory scriptsViewModelFactory,
        INodeService<PatternNode, PatternNode> nodeService,
        IRepository<Script> scriptRepository)
    {
        _macroSetRepository = macroSetRepository;
        _toastService = toastService;
        _patternTreeViewFactory = patternTreeViewFactory;
        _scriptsViewModelFactory = scriptsViewModelFactory;
        _nodeService = nodeService;
        _macroSets = ProxyViewModel.CreateCollection(_macroSetRepository.Get());
        _scriptRespository = scriptRepository;
        if (_macroSets.Count > 0 )
        {
            SelectedMacroSet = _macroSets.First();
        }

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities(_macroSets.ToArray());
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternTreeViewViewModel>();
        _macroSetIdToScripts = new ConcurrentDictionary<int, ScriptsViewModel>();
    }

    [RelayCommand]
    public async Task AddMacroSet()
    {
        var macroSetName = await Application.Current.MainPage.DisplayPromptAsync("Macro Set", "Enter name...");
        if (string.IsNullOrEmpty(macroSetName)) return;

        var macroSet = ProxyViewModel.Create(new MacroSet() { Name = macroSetName });
        var rootPattern = ProxyViewModel.Create(_nodeService.GetRoot(0));
        rootPattern.Children = ProxyViewModel.CreateCollection(new ObservableCollection<PatternNode>());
        rootPattern.Patterns = ProxyViewModel.CreateCollection(new ObservableCollection<Pattern>());
        rootPattern.UserPatterns = ProxyViewModel.CreateCollection(new ObservableCollection<UserPattern>());
        _nodeService.ReAttachNodes(rootPattern);
        macroSet.RootPattern = rootPattern;
        macroSet.RootPatternNodeId = rootPattern.NodeId;
        _macroSets.Add(macroSet);
        _macroSetRepository.Insert(macroSet);
        _macroSetRepository.Save();
        SelectedMacroSet = macroSet;
        _toastService.Show($"Added MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    public async Task DeleteMacroSet(MacroSet macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        _nodeService.Delete(_nodeRootIdToPatternTree[macroSet.RootPatternNodeId].Root);
        _macroSetRepository.Delete(macroSet);
        _macroSetRepository.Save();
        _macroSets.Remove(macroSet);
        _toastService.Show($"Deleted MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task ExportScripts()
    {
        if (Scripts == null) return;
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;

        var patternTreeJson = JsonSerializer.Serialize(Scripts.Scripts, new JsonSerializerOptions { WriteIndented = true });
#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        var targetDirctory = DeviceInfo.Current.Platform == DevicePlatform.Android ? 
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{_selectedMacroSet.Name}_scripts.json");
        File.WriteAllText(targetFile, patternTreeJson);
#elif WINDOWS
        var targetDirctory = FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{_selectedMacroSet.Name}_scripts.json");
        File.WriteAllText(targetFile, patternTreeJson);
#endif
        _toastService.Show($"Exported Scripts: {_selectedMacroSet.Name}");
    }

    [RelayCommand]
    private async Task ImportScripts()
    {
        if (Scripts == null) return;
        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceNames = currentAssembly.GetManifestResourceNames().Where(rs => rs.StartsWith("YeetMacro2.Resources.MacroSets"));
        var regex = new Regex(@"YeetMacro2\.Resources\.MacroSets\.(?<macroSet>.+?)\.");

        // https://stackoverflow.com/questions/9436381/c-sharp-regex-string-extraction
        var macroSetGroups = resourceNames.GroupBy(rn => regex.Match(rn).Groups["macroSet"].Value);
        var selectedMacroSet = await Application.Current.MainPage.DisplayActionSheet("Import Scripts", "Cancel", null, macroSetGroups.Select(g => g.Key).ToArray());
        if (selectedMacroSet == null || selectedMacroSet == "Cancel") return;

        using (var stream = currentAssembly.GetManifestResourceStream($"YeetMacro2.Resources.MacroSets.{selectedMacroSet}.{selectedMacroSet.Replace("_", " ")}_scripts.json"))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            var newScripts = JsonSerializer.Deserialize<IEnumerable<Script>>(json);
            var currentScripts = Scripts.Scripts.ToList();
            foreach (var currentChild in currentScripts)
            {
                Scripts.Scripts.Remove(currentChild);
                _scriptRespository.Delete(currentChild);
            }

            foreach (var newChild in newScripts)
            {
                newChild.MacroSetId = SelectedMacroSet.MacroSetId;
                var proxy = ProxyViewModel.Create(newChild);
                Scripts.Scripts.Add(proxy);
                _scriptRespository.Insert(proxy);
            }
            _scriptRespository.Save();
        }
        _toastService.Show($"Imported Patterns: {_selectedMacroSet.Name}");
    }

    [RelayCommand]
    private void OpenAppDirectory()
    {
        //Only for Windows
        System.Diagnostics.Process.Start("explorer.exe", FileSystem.Current.AppDataDirectory);
    }
}
