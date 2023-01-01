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
    IRepository<PatternBase> _patternRepository;
    INodeService<PatternNode, PatternNode> _nodeService;
    PatternTreeViewViewModelFactory _patternTreeViewFacctory;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PatternTree))]
    MacroSet _selectedMacroSet;
    ConcurrentDictionary<int, PatternTreeViewViewModel> _nodeRootIdToPatternTree;

    public PatternTreeViewViewModel PatternTree
    {
        get
        {
            // Lazy load PattenTree
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(_selectedMacroSet.RootPatternNodeId))
            {
                var patternTree = _patternTreeViewFacctory.Create(_selectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(_selectedMacroSet.RootPatternNodeId, patternTree);
            }
            return _nodeRootIdToPatternTree[_selectedMacroSet.RootPatternNodeId];
        }
    }

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository,
        IToastService toastService,
        PatternTreeViewViewModelFactory patternTreeViewFactory,
        INodeService<PatternNode, PatternNode> nodeService,
        IRepository<PatternBase> patternRepository)
    {
        _macroSetRepository = macroSetRepository;
        _toastService = toastService;
        _patternTreeViewFacctory = patternTreeViewFactory;
        _nodeService = nodeService;
        _patternRepository = patternRepository;
        _macroSets = ProxyViewModel.CreateCollection(_macroSetRepository.Get());
        if (_macroSets.Count > 0 )
        {
            SelectedMacroSet = _macroSets.First();
        }

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities(_macroSets.ToArray());
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternTreeViewViewModel>();
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
    }

    [RelayCommand]
    public async Task DeleteMacroSet(MacroSet macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        _nodeService.Delete(macroSet.RootPattern);
        _macroSetRepository.Delete(macroSet);
        _macroSetRepository.Save();
        _macroSets.Remove(macroSet);
        _toastService.Show($"Deleted MacroSet: {macroSet.Name})");
    }

    [RelayCommand]
    private void ExportPatterns()
    {
        if (PatternTree == null) return;
        var patternTreeJson = JsonSerializer.Serialize(PatternTree.Root, new JsonSerializerOptions { WriteIndented = true });
        // https://stackoverflow.com/questions/74047234/net-maui-writing-file-in-android-to-folder-that-then-can-be-accessed-in-windows
        // var x = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);

        var targetDirctory = DeviceInfo.Current.Platform == DevicePlatform.Android ? "/storage/emulated/0/Pictures" : FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{_selectedMacroSet.Name}_patterns.json");
        File.WriteAllText(targetFile, patternTreeJson);
        _toastService.Show($"Exported Patterns: {_selectedMacroSet.Name})");
    }

    [RelayCommand]
    private async Task ImportPatterns()
    {
        if (PatternTree == null) return;
        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceNames = currentAssembly.GetManifestResourceNames().Where(rs => rs.StartsWith("YeetMacro2.Resources.MacroSets"));
        var regex = new Regex(@"YeetMacro2\.Resources\.MacroSets\.(?<macroSet>.+?)\.");

        // https://stackoverflow.com/questions/9436381/c-sharp-regex-string-extraction
        var macroSetGroups = resourceNames.GroupBy(rn => regex.Match(rn).Groups["macroSet"].Value);
        var selectedMacroSet = await Application.Current.MainPage.DisplayActionSheet("Import Patterns", "Cancel", null, macroSetGroups.Select(g => g.Key).ToArray());
        if (selectedMacroSet == null || selectedMacroSet == "Cancel") return;

        using (var stream = currentAssembly.GetManifestResourceStream($"YeetMacro2.Resources.MacroSets.{selectedMacroSet}.{selectedMacroSet.Replace("_", " ")}_patterns.json"))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            var rootTemp = ProxyViewModel.Create(JsonSerializer.Deserialize<PatternNode>(json));
            foreach (var currentChild in PatternTree.Root.Children.ToList())
            {
                _nodeService.Delete(currentChild);
            }
            
            foreach (var newChild in rootTemp.Children.ToList())
            {
                PatternTree.Root.Children.Add(newChild);
                _nodeService.Insert(newChild);
            }
        }
        _toastService.Show($"Imported Patterns: {_selectedMacroSet.Name})");
    }

    [RelayCommand]
    private void OpenAppDirectory()
    {
        //Only for Windows
        System.Diagnostics.Process.Start("explorer.exe", FileSystem.Current.AppDataDirectory);
    }
}
