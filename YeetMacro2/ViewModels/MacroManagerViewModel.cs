using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
    INodeService<PatternNode, PatternNode> _patternNodeService;
    INodeService<ScriptNode, ScriptNode> _scriptNodeService;
    INodeService<ParentSetting, SettingNode> _settingNodeService;
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
    JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        Converters = {
                new JsonStringEnumConverter()
            },
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = SizePropertiesResolver.Instance
    };

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
    public ICommand OnScriptFinished { get; set; }

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository,
        IToastService toastService,
        NodeViewModelFactory nodeViewModelFactory,
        INodeService<PatternNode, PatternNode> patternNodeService,
        INodeService<ScriptNode, ScriptNode> scriptNodeService,
        INodeService<ParentSetting, SettingNode> settingNodeService,
        IScriptService scriptService)
    {
        _macroSetRepository = macroSetRepository;
        _toastService = toastService;
        _nodeViewModelFactory = nodeViewModelFactory;
        _patternNodeService = patternNodeService;
        _scriptNodeService = scriptNodeService;
        _settingNodeService = settingNodeService;

        // manually instantiating in ServiceRegistrationHelper.AppInitializer to pre initialize MacroSets
        if (_macroSetRepository == null) return;  

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
        var rootPattern = ProxyViewModel.Create(_patternNodeService.GetRoot(0));
        _patternNodeService.ReAttachNodes(rootPattern);
        macroSet.RootPatternNodeId = rootPattern.NodeId;

        var rootScript = ProxyViewModel.Create(_scriptNodeService.GetRoot(0));
        _scriptNodeService.ReAttachNodes(rootScript);
        macroSet.RootScriptNodeId = rootScript.NodeId;

        var rootSetting = ProxyViewModel.Create(_settingNodeService.GetRoot(0));
        _settingNodeService.ReAttachNodes(rootSetting);
        macroSet.RootSettingNodeId = rootSetting.NodeId;

        MacroSets.Add(macroSet);
        _macroSetRepository.Insert(macroSet);
        _macroSetRepository.Save();
        SelectedMacroSet = macroSet;
        _toastService.Show($"Added MacroSet: {macroSet.Name}");
    }

    //public async Task AddLocalAssetMacroSet(string uri)
    //{
    //    var macroSetJson = await ServiceHelper.GetAssetContent(Path.Combine("MacroSets", uri, "macroSet.json"));
    //    var macroSet = ProxyViewModel.Create(JsonSerializer.Deserialize<MacroSet>(macroSetJson, _jsonSerializerOptions));

    //}


    [RelayCommand]
    public async Task DeleteMacroSet(MacroSet macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        await Patterns.WaitForInitialization();
        await Scripts.WaitForInitialization();
        await Settings.WaitForInitialization();
        _patternNodeService.Delete(_nodeRootIdToPatternTree[macroSet.RootPatternNodeId].Root);
        _scriptNodeService.Delete(_nodeRootIdToScriptTree[macroSet.RootScriptNodeId].Root);
        _settingNodeService.Delete(_nodeRootIdToSettingTree[macroSet.RootSettingNodeId].Root);
        _macroSetRepository.Delete(macroSet);
        _macroSetRepository.Save();
        MacroSets.Remove(macroSet);
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
        macroSet.Source.Uri = MacroSetSourceLink;
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
        _scriptService.RunScript(scriptNode.Text, Patterns.ToJson(), Settings.ToJson(), () =>
        {
            OnScriptFinished?.Execute(null);
        });

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
        var json = JsonSerializer.Serialize(macroSet, _jsonSerializerOptions);
        _toastService.Show($"Exported MacroSet: {macroSet.Name}");
        ExportValue = json;
        ShowExport = true;
    }

    [RelayCommand]
    private async void CalculateHash(MacroSet macroSet)
    {
        var macroSetJson = JsonSerializer.Serialize(macroSet, _jsonSerializerOptions);
        await Patterns.WaitForInitialization();
        var patternsJson = Patterns.ToJson();
        await Scripts.WaitForInitialization();
        var scriptsJson = Scripts.ToJson();
        await Settings.WaitForInitialization();
        var settingsJson = Settings.ToJson();

        var hash = new MacroSetHash()
        {
            MacroSet = ContentHasher.Create(macroSetJson),
            Patterns = ContentHasher.Create(patternsJson),
            Scripts = ContentHasher.Create(scriptsJson),
            Settings = ContentHasher.Create(settingsJson),
        };
        // https://stackoverflow.com/questions/65620060/equivalent-of-jobject-in-system-text-json
        //var hashJson = new JsonObject()
        //{
        //    ["macroSet"] = ContentHasher.Create(macroSetJson),
        //    ["patterns"] = ContentHasher.Create(patternsJson),
        //    ["scripts"] = ContentHasher.Create(scriptsJson),
        //    ["settings"] = ContentHasher.Create(settingsJson)
        //};

        _toastService.Show($"Hash calculated for MacroSet: {macroSet.Name}");
        //ExportValue = hashJson.ToJsonString(_jsonSerializerOptions);
        ExportValue = JsonSerializer.Serialize(hash, _jsonSerializerOptions);
        ShowExport = true;
    }

    [RelayCommand]
    private void CloseExport()
    {
        ShowExport = false;
    }

    partial void OnSelectedMacroSetChanged(MacroSet value)
    {
        if (value == null)
        {
            Preferences.Default.Remove(nameof(SelectedMacroSet));
            return;
        }
        ResolutionWidth = value.Resolution.Width;
        ResolutionHeight = value.Resolution.Height;
        if (value.Source != null)
        {
            MacroSetSourceType = value.Source.Type;
            MacroSetSourceLink = value.Source.Uri;
        }
        Preferences.Default.Set(nameof(SelectedMacroSet), value.Name);
    }
}
