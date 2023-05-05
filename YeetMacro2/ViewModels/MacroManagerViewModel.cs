using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public partial class MacroManagerViewModel : ObservableObject
{
    IRepository<MacroSet> _macroSetRepository;
    IToastService _toastService;
    INodeService<PatternNode, PatternNode> _nodeService;
    NodeViewModelFactory _nodeViewModelFactory;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Patterns), nameof(Scripts))]
    MacroSet _selectedMacroSet;
    ConcurrentDictionary<int, PatternNodeViewModel> _nodeRootIdToPatternTree;
    ConcurrentDictionary<int, ScriptNodeViewModel> _nodeRootIdToScriptTree;
    ConcurrentDictionary<int, SettingNodeViewModel> _nodeRootIdToSettingTree;
    IScriptService _scriptService;
    [ObservableProperty]
    bool _inDebugMode, _showLogView, _showExport;
    [ObservableProperty]
    double _resolutionWidth, _resolutionHeight;
    [ObservableProperty]
    MacroSetSourceType _macroSetSourceType;
    [ObservableProperty]
    string _macroSetSourceLink;
    [ObservableProperty]
    string _exportValue;

    public PatternNodeViewModel Patterns
    {
        get
        {
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(_selectedMacroSet.RootPatternNodeId))
            {
                var tree = _nodeViewModelFactory.Create<PatternNodeViewModel>(_selectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(_selectedMacroSet.RootPatternNodeId, tree);
            }
            return _nodeRootIdToPatternTree[_selectedMacroSet.RootPatternNodeId];
        }
    }

    public ScriptNodeViewModel Scripts
    {
        get
        {
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToScriptTree.ContainsKey(_selectedMacroSet.RootScriptNodeId))
            {
                var tree = _nodeViewModelFactory.Create<ScriptNodeViewModel>(_selectedMacroSet.RootScriptNodeId);
                _nodeRootIdToScriptTree.TryAdd(_selectedMacroSet.RootScriptNodeId, tree);
            }
            return _nodeRootIdToScriptTree[_selectedMacroSet.RootScriptNodeId];
        }
    }

    public SettingNodeViewModel Settings
    {
        get
        {
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToSettingTree.ContainsKey(_selectedMacroSet.RootSettingNodeId))
            {
                var tree = _nodeViewModelFactory.Create<SettingNodeViewModel>(_selectedMacroSet.RootSettingNodeId);
                _nodeRootIdToSettingTree.TryAdd(_selectedMacroSet.RootSettingNodeId, tree);
            }
            return _nodeRootIdToSettingTree[_selectedMacroSet.RootSettingNodeId];
        }
    }

    public ICommand OnScriptExecuted { get; set; }

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository,
        IToastService toastService,
        NodeViewModelFactory nodeViewModelFactory,
        INodeService<PatternNode, PatternNode> nodeService,
        IScriptService scriptService)
    {
        _macroSetRepository = macroSetRepository;
        _toastService = toastService;
        _nodeViewModelFactory = nodeViewModelFactory;
        _nodeService = nodeService;
        var tempMacroSets = _macroSetRepository.Get();
        _macroSets = ProxyViewModel.CreateCollection(tempMacroSets);

        if (Preferences.Default.ContainsKey(nameof(SelectedMacroSet)) && _macroSets.Any(ms => ms.Name == Preferences.Default.Get<string>(nameof(SelectedMacroSet), null)))
        {
            SelectedMacroSet = _macroSets.First(ms => ms.Name == Preferences.Default.Get<string>(nameof(SelectedMacroSet), null));
        } 
        else if (_macroSets.Count > 0 )
        {
            SelectedMacroSet = _macroSets.First();
        }

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities(_macroSets.ToArray());
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternNodeViewModel>();
        _nodeRootIdToScriptTree = new ConcurrentDictionary<int, ScriptNodeViewModel>();
        _nodeRootIdToSettingTree = new ConcurrentDictionary<int, SettingNodeViewModel>();
        _scriptService = scriptService;
    }

    [RelayCommand]
    public async Task AddMacroSet()
    {
        var macroSetName = await Application.Current.MainPage.DisplayPromptAsync("Macro Set", "Enter name...");
        if (string.IsNullOrEmpty(macroSetName)) return;

        var macroSet = ProxyViewModel.Create(new MacroSet() { Name = macroSetName });
        var rootPattern = ProxyViewModel.Create(_nodeService.GetRoot(0));
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
    private void OpenAppDirectory()
    {
        //Only for Windows
        System.Diagnostics.Process.Start("explorer.exe", FileSystem.Current.AppDataDirectory);
    }

    [RelayCommand]
    private void Save(MacroSet macroSet)
    {
        macroSet.Resolution = new Size(ResolutionWidth, ResolutionHeight);
        macroSet.Source.Link = MacroSetSourceLink;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
        _toastService.Show($"Saved MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task ExecuteScript(ScriptNode scriptNode)
    {
        _scriptService.InDebugMode = InDebugMode;
        await Patterns.WaitForInitialization();
        await Settings.WaitForInitialization();
        _scriptService.RunScript(scriptNode.Text, Patterns.ToJson(), Settings.ToJson());

        OnScriptExecuted?.Execute(null);
    }

    [RelayCommand]
    private void ToggleShowLogView()
    {
        ShowLogView = !ShowLogView;
    }

    [RelayCommand]
    private void ToggleInDebugMode()
    {
        InDebugMode = !InDebugMode;
    }

    [RelayCommand]
    private void ExportMacroSet(MacroSet macroSet)
    {
        var json = JsonSerializer.Serialize(macroSet, new JsonSerializerOptions()
        {
            Converters = {
                new JsonStringEnumConverter()
            },
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = SizePropertiesResolver.Instance
        });
        _toastService.Show($"Exported MacroSet: {macroSet.Name}");
        ExportValue = json;
        ShowExport = true;
    }

    [RelayCommand]
    private void CloseExport()
    {
        ShowExport = false;
    }

    partial void OnSelectedMacroSetChanged(MacroSet value)
    {
        ResolutionWidth = value.Resolution.Width;
        ResolutionHeight = value.Resolution.Height;
        MacroSetSourceType = value.Source.Type;
        MacroSetSourceLink = value.Source.Link;
        Preferences.Default.Set(nameof(SelectedMacroSet), value.Name);
    }
}
