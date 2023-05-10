using AutoMapper;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Text.Json;
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
    bool _inDebugMode, _showLogView, _showExport, _showSettings;
    [ObservableProperty]
    double _resolutionWidth, _resolutionHeight;
    [ObservableProperty]
    MacroSetSourceType _macroSetSourceType;
    [ObservableProperty]
    string _macroSetSourceUri;
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
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(SelectedMacroSet.RootPatternNodeId))
            {
                var tree = _nodeViewModelFactory.Create<PatternNodeViewModel>(SelectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(SelectedMacroSet.RootPatternNodeId, tree);
            }
            return _nodeRootIdToPatternTree[SelectedMacroSet.RootPatternNodeId];
        }
    }

    public ScriptNodeViewModel Scripts
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToScriptTree.ContainsKey(SelectedMacroSet.RootScriptNodeId))
            {
                var tree = _nodeViewModelFactory.Create<ScriptNodeViewModel>(SelectedMacroSet.RootScriptNodeId);
                _nodeRootIdToScriptTree.TryAdd(SelectedMacroSet.RootScriptNodeId, tree);
            }
            return _nodeRootIdToScriptTree[SelectedMacroSet.RootScriptNodeId];
        }
    }

    public SettingNodeViewModel Settings
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToSettingTree.ContainsKey(SelectedMacroSet.RootSettingNodeId))
            {
                var tree = _nodeViewModelFactory.Create<SettingNodeViewModel>(SelectedMacroSet.RootSettingNodeId);
                _nodeRootIdToSettingTree.TryAdd(SelectedMacroSet.RootSettingNodeId, tree);
            }
            return _nodeRootIdToSettingTree[SelectedMacroSet.RootSettingNodeId];
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

        var macroSet = ProxyViewModel.Create(new MacroSet() { Name = macroSetName, Source = new MacroSetSource() });
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
        macroSet.Source.Uri = MacroSetSourceUri;
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
    private void ToggleShowSettings()
    {
        ShowSettings = !ShowSettings;
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
    private async Task UpdateMacroSet(MacroSet macroSet)
    {
        var localMacroSets = ServiceHelper.ListAssets("MacroSets").ToList();
        if (!localMacroSets.Contains(macroSet.Source.Uri))
        {
            _toastService.Show($"Did not find local MacroSet: {macroSet.Name}");
        }

        var hash = await GetHash();
        var macroSetFolder = macroSet.Source.Uri;
        var hashJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetFolder, "hash.json"));
        var localHash = JsonSerializer.Deserialize<MacroSetHash>(hashJson, _jsonSerializerOptions);

        if (hash.MacroSet != localHash.MacroSet)
        {
            var json = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetFolder, "macroSet.json"));
            var localMacroSet = JsonSerializer.Deserialize<MacroSet>(json, _jsonSerializerOptions);

            macroSet.Resolution = localMacroSet.Resolution;
            _macroSetRepository.Update(macroSet);
            _macroSetRepository.Save();
        }

        if (hash.Patterns != localHash.Patterns)
        {
            var pattternJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetFolder, "patterns.json"));
            var patterns = PatternNodeViewModel.FromJson(pattternJson);
            Patterns.Import(patterns);
        }

        if (hash.Scripts != localHash.Scripts)
        {
            var scriptList = ServiceHelper.ListAssets(Path.Combine("MacroSets", macroSetFolder, "scripts"));
            var scripts = new ScriptNodeViewModel(-1, null, null, null) { Root = new ScriptNode() { Nodes = new List<ScriptNode>() } };
            foreach (var scriptFile in scriptList)
            {
                var scriptText = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetFolder, "scripts", scriptFile));
                var script = new ScriptNode()
                {
                    Name = Path.GetFileNameWithoutExtension(scriptFile),
                    Text = scriptText,
                    RootId = Scripts.Root.NodeId,
                    ParentId = Scripts.Root.NodeId
                };

                scripts.Root.Nodes.Add(script);
            }

            Scripts.Import(scripts);
        }

        if (hash.Settings != localHash.Settings)
        {
            var json = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetFolder, "settings.json"));
            var settings = SettingNodeViewModel.FromJson(json);
            Settings.Import(settings);
        }

        _toastService.Show($"Updated MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async void CalculateHash(MacroSet macroSet)
    {
        var hash = await GetHash();

        _toastService.Show($"Hash calculated for MacroSet: {macroSet.Name}");
        ExportValue = JsonSerializer.Serialize(hash, _jsonSerializerOptions);
        ShowExport = true;
    }

    [RelayCommand]
    private void CloseExport()
    {
        ShowExport = false;
    }

    private async Task<MacroSetHash> GetHash()
    {
        var macroSetJson = JsonSerializer.Serialize(SelectedMacroSet, _jsonSerializerOptions);
        await Patterns.WaitForInitialization();
        var patternsJson = Patterns.ToJson();
        await Scripts.WaitForInitialization();
        var scriptsJson = Scripts.ToJson();
        await Settings.WaitForInitialization();
        var settingsJson = Settings.ToJson();

        return new MacroSetHash()
        {
            MacroSet = ContentHasher.Create(macroSetJson),
            Patterns = ContentHasher.Create(patternsJson),
            Scripts = ContentHasher.Create(scriptsJson),
            Settings = ContentHasher.Create(settingsJson),
        };
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
            MacroSetSourceUri = value.Source.Uri;
        }
        Preferences.Default.Set(nameof(SelectedMacroSet), value.Name);
    }
}
