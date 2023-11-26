using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using YeetMacro2.ViewModels.NodeViewModels;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace YeetMacro2.ViewModels;

public partial class MacroManagerViewModel : ObservableObject
{
    ILogger _logger;
    IRepository<MacroSet> _macroSetRepository;
    IToastService _toastService;
    INodeService<PatternNode, PatternNode> _patternNodeService;
    INodeService<ScriptNode, ScriptNode> _scriptNodeService;
    INodeService<ParentSetting, SettingNode> _settingNodeService;
    INodeService<DailyNode, DailyNode> _dailyNodeService;
    NodeManagerViewModelFactory _nodeViewModelManagerFactory;
    IMapper _mapper;
    IHttpService _httpService;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Patterns), nameof(Scripts))]
    MacroSet _selectedMacroSet;
    ConcurrentDictionary<int, PatternNodeManagerViewModel> _nodeRootIdToPatternTree;
    ConcurrentDictionary<int, ScriptNodeManagerViewModel> _nodeRootIdToScriptList;
    ConcurrentDictionary<int, SettingNodeManagerViewModel> _nodeRootIdToSettingTree;
    ConcurrentDictionary<int, DailyNodeManagerViewModel> _nodeRootIdToDailyList;
    IScriptService _scriptService;
    [ObservableProperty]
    bool _isExportEnabled, _isOpenAppDirectoryEnabled, _inDebugMode, _showStatusPanel, 
        _showExport, _showSettings, _isBusy, _persistLogs, _showMacroSetDescriptionEditor;
    [ObservableProperty]
    double _resolutionWidth, _resolutionHeight;
    [ObservableProperty]
    string _exportValue, _message;
    JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        Converters = {
                new JsonStringEnumConverter()
            },
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = SizePropertiesResolver.Instance
    };
    string _targetBranch = "main";
    public PatternNodeManagerViewModel Patterns
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(SelectedMacroSet.RootPatternNodeId))
            {
                var tree = _nodeViewModelManagerFactory.Create<PatternNodeManagerViewModel>(SelectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(SelectedMacroSet.RootPatternNodeId, tree);
            }
            return _nodeRootIdToPatternTree[SelectedMacroSet.RootPatternNodeId];
        }
    }

    public ScriptNodeManagerViewModel Scripts
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToScriptList.ContainsKey(SelectedMacroSet.RootScriptNodeId))
            {
                var tree = _nodeViewModelManagerFactory.Create<ScriptNodeManagerViewModel>(SelectedMacroSet.RootScriptNodeId);
                _nodeRootIdToScriptList.TryAdd(SelectedMacroSet.RootScriptNodeId, tree);
            }
            return _nodeRootIdToScriptList[SelectedMacroSet.RootScriptNodeId];
        }
    }

    public SettingNodeManagerViewModel Settings
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToSettingTree.ContainsKey(SelectedMacroSet.RootSettingNodeId))
            {
                var tree = _nodeViewModelManagerFactory.Create<SettingNodeManagerViewModel>(SelectedMacroSet.RootSettingNodeId);
                _nodeRootIdToSettingTree.TryAdd(SelectedMacroSet.RootSettingNodeId, tree);
            }
            return _nodeRootIdToSettingTree[SelectedMacroSet.RootSettingNodeId];
        }
    }

    public DailyNodeManagerViewModel Dailies
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToDailyList.ContainsKey(SelectedMacroSet.RootDailyNodeId))
            {
                var tree = _nodeViewModelManagerFactory.Create<DailyNodeManagerViewModel>(SelectedMacroSet.RootDailyNodeId);
                _nodeRootIdToDailyList.TryAdd(SelectedMacroSet.RootDailyNodeId, tree);
            }
            return _nodeRootIdToDailyList[SelectedMacroSet.RootDailyNodeId];
        }
    }

    public string AppVersion { get { return AppInfo.Current.VersionString; } }
    public MacroManagerViewModel(ILogger<MacroManagerViewModel> logger,
        IRepository<MacroSet> macroSetRepository,
        IToastService toastService,
        NodeManagerViewModelFactory nodeViewModelFactory,
        INodeService<PatternNode, PatternNode> patternNodeService,
        INodeService<ScriptNode, ScriptNode> scriptNodeService,
        INodeService<ParentSetting, SettingNode> settingNodeService,
        INodeService<DailyNode, DailyNode> dailyNodeService,
        IScriptService scriptService,
        IMapper mapper,
        IHttpService httpService)
    {
        _logger = logger;
        _macroSetRepository = macroSetRepository;
        _toastService = toastService;
        _nodeViewModelManagerFactory = nodeViewModelFactory;
        _patternNodeService = patternNodeService;
        _scriptNodeService = scriptNodeService;
        _settingNodeService = settingNodeService;
        _dailyNodeService = dailyNodeService;
        _mapper = mapper;
        _httpService = httpService;
        // manually instantiating in ServiceRegistrationHelper.AppInitializer to pre initialize MacroSets
        if (_macroSetRepository == null) return;  

        var tempMacroSets = _macroSetRepository.Get();
        MacroSets = new ObservableCollection<MacroSet>(tempMacroSets.Select(ms => (MacroSet)_mapper.Map<MacroSetViewModel>(ms)));

        if (Preferences.Default.ContainsKey(nameof(SelectedMacroSet)) && MacroSets.Any(ms => ms.Name == Preferences.Default.Get<string>(nameof(SelectedMacroSet), null)))
        {
            SelectedMacroSet = MacroSets.First(ms => ms.Name == Preferences.Default.Get<string>(nameof(SelectedMacroSet), null));
        } 
        else if (MacroSets.Count > 0 )
        {
            SelectedMacroSet = MacroSets.First();
        }

        InDebugMode = Preferences.Default.Get(nameof(InDebugMode), false);
        ShowStatusPanel = Preferences.Default.Get(nameof(ShowStatusPanel), false);

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities(_macroSets.ToArray());
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternNodeManagerViewModel>();
        _nodeRootIdToScriptList = new ConcurrentDictionary<int, ScriptNodeManagerViewModel>();
        _nodeRootIdToSettingTree = new ConcurrentDictionary<int, SettingNodeManagerViewModel>();
        _nodeRootIdToDailyList = new ConcurrentDictionary<int, DailyNodeManagerViewModel>();
        _scriptService = scriptService;

#if DEBUG
        IsExportEnabled = true;
#endif

#if WINDOWS
        IsOpenAppDirectoryEnabled = true;
#endif
    }

    [RelayCommand]
    public async Task AddLocalMacroSet()
    {
        var localMacroSets = ServiceHelper.ListAssets("MacroSets");
        var sources = localMacroSets.Select(ms => $"localAsset:{ms}").ToArray();
        await AddMacroSet(sources);
    }

    [RelayCommand]
    public async Task AddOnlineMacroSet()
    {
        var macroSetsUrl = $"https://github.com/kappagacha/yeetmacro2/tree-commit-info/{_targetBranch}/YeetMacro2/Resources/Raw/MacroSets";
        var strMacroSets = await _httpService.GetAsync(macroSetsUrl, new Dictionary<string, string>() { { "Accept", "application/json" } });
        var jsonMacroSets = JsonSerializer.Deserialize<JsonObject>(strMacroSets);
        var sources = jsonMacroSets.Select(ms => $"online:{ms.Key}").ToArray();
        await AddMacroSet(sources);
    }

    public async Task AddMacroSet(string[] sources)
    {
        var source = await Application.Current.MainPage.DisplayActionSheet("Source", "cancel", "ok", sources);
        if (string.IsNullOrEmpty(source) || source == "cancel") return;
        IsBusy = true;
        string macroSetName = source;
        if (source.StartsWith("localAsset:")) macroSetName = source.Substring(11);
        else if (source.StartsWith("online:")) macroSetName = source.Substring(7);

        var macroSet = new MacroSetViewModel() { Name = macroSetName, Source = source };

        var rootPattern = _mapper.Map<PatternNodeViewModel>(_patternNodeService.GetRoot(0));
        _patternNodeService.ReAttachNodes(rootPattern);
        macroSet.RootPatternNodeId = rootPattern.NodeId;

        var rootScript = _mapper.Map<ScriptNodeViewModel>(_scriptNodeService.GetRoot(0));
        _scriptNodeService.ReAttachNodes(rootScript);
        macroSet.RootScriptNodeId = rootScript.NodeId;

        var rootSetting = _mapper.Map<ParentSettingViewModel>(_settingNodeService.GetRoot(0));
        _settingNodeService.ReAttachNodes(rootSetting);
        macroSet.RootSettingNodeId = rootSetting.NodeId;

        var rootDaily = _mapper.Map<DailyNodeViewModel>(_dailyNodeService.GetRoot(0));
        _dailyNodeService.ReAttachNodes(rootDaily);
        macroSet.RootDailyNodeId = rootDaily.NodeId;

        MacroSets.Add(macroSet);
        _macroSetRepository.Insert(macroSet);
        _macroSetRepository.Save();
        SelectedMacroSet = macroSet;
        await UpdateMacroSet(macroSet);
        IsBusy = false;
        _toastService.Show($"Added MacroSet: {macroSet.Name}");
    } 

    [RelayCommand]
    public async Task DeleteMacroSet(MacroSet macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        await Patterns.WaitForInitialization();
        await Scripts.WaitForInitialization();
        await Settings.WaitForInitialization();
        _patternNodeService.Delete(_nodeRootIdToPatternTree[macroSet.RootPatternNodeId].Root);
        _scriptNodeService.Delete(_nodeRootIdToScriptList[macroSet.RootScriptNodeId].Root);
        _settingNodeService.Delete(_nodeRootIdToSettingTree[macroSet.RootSettingNodeId].Root);
        _dailyNodeService.Delete(_nodeRootIdToDailyList[macroSet.RootDailyNodeId].Root);
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
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
        _toastService.Show($"Saved MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task ExecuteScript(ScriptNode scriptNode)
    {
        if (IsBusy) return;

        await Patterns.WaitForInitialization();
        await Settings.WaitForInitialization();

        await Task.Run(() =>
        {
            IsBusy = true;
            if (PersistLogs) 
            { 
                _logger.LogInformation("{persistLogs}", true);
                _logger.LogInformation("{macroSet} {script}", SelectedMacroSet?.Name ?? string.Empty, scriptNode.Name);
            };
            Console.WriteLine($"[*****YeetMacro*****] MacroManagerViewModel ExecuteScript");

            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() {  Type = ScriptEventType.Started }));
            var result = _scriptService.RunScript(scriptNode, Scripts, SelectedMacroSet, Patterns, Settings);
            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() { Type = ScriptEventType.Finished, Result = result }));
            if (PersistLogs) _logger.LogInformation("{persistLogs}", false);
            IsBusy = false;
        });
    }

    [RelayCommand]
    private void ToggleShowStatusPanel()
    {
        ShowStatusPanel = !ShowStatusPanel;
        Preferences.Default.Set(nameof(ShowStatusPanel), ShowStatusPanel);
    }

    [RelayCommand]
    private void ToggleInDebugMode()
    {
        InDebugMode = !InDebugMode;
        Preferences.Default.Set(nameof(InDebugMode), InDebugMode);
    }

    [RelayCommand]
    private void ToggleShowSettings()
    {
        ShowSettings = !ShowSettings;
    }

    [RelayCommand]
    private void ToggleShowMacroSetDescriptionEditor()
    {
        ShowMacroSetDescriptionEditor = !ShowMacroSetDescriptionEditor;
    }

    [RelayCommand]
    private async Task ExportMacroSet(MacroSet macroSet)
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;
        IsBusy = true;

        macroSet.MacroSetLastUpdated = DateTimeOffset.Now;
        macroSet.PatternsLastUpdated = DateTimeOffset.Now;
        macroSet.ScriptsLastUpdated = DateTimeOffset.Now;
        macroSet.SettingsLastUpdated = DateTimeOffset.Now;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();

        var macroSetJson = JsonSerializer.Serialize(macroSet, _jsonSerializerOptions);
        var targetDirectory = "";
#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        targetDirectory = DeviceInfo.Current.Platform == DevicePlatform.Android ?
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            FileSystem.Current.AppDataDirectory;
#elif WINDOWS
        targetDirectory = FileSystem.Current.AppDataDirectory;
#endif

        if (macroSet.Source.StartsWith("localAsset:"))
        {
            targetDirectory = Path.Combine(targetDirectory, $"{macroSet.Source.Substring(11)}");
        }
        else
        {
            targetDirectory = Path.Combine(targetDirectory, $"{macroSet.Source.Substring(7)}");
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        await Patterns.WaitForInitialization();
        await Settings.WaitForInitialization();
        File.WriteAllText(Path.Combine(targetDirectory, $"macroSet.json"), macroSetJson);
        File.WriteAllText(Path.Combine(targetDirectory, $"patterns.json"), Patterns.ToJson());
        Settings.Traverse(Settings.Root, s =>
        {
            if (s is SettingNode<String> stringSetting)
            {
                stringSetting.DefaultValue = stringSetting.Value;
            }
            else if (s is SettingNode<Boolean> boolSetting)
            {
                boolSetting.DefaultValue = boolSetting.Value;
            }
            else if (s is SettingNode<int> integerSetting)
            {
                integerSetting.DefaultValue = integerSetting.Value;
            }
        });
        File.WriteAllText(Path.Combine(targetDirectory, $"settings.json"), Settings.ToJson());

        ExportValue = macroSetJson;
        ShowExport = true;
        IsBusy = false;
        _toastService.Show($"Exported MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task UpdateMacroSet(MacroSet macroSet)
    {
        IsBusy = true;

        try
        {
            MacroSet targetMacroSet;
            string macroSetJson = null, pattternJson = null, settingJson = null;
            Dictionary<string, string> nameToScript = new Dictionary<string, string>();
            if (macroSet.Source.StartsWith("localAsset:"))
            {
                var macroSetName = macroSet.Source.Substring(11);
                var localMacroSets = ServiceHelper.ListAssets("MacroSets").ToList();
                if (!localMacroSets.Contains(macroSetName))
                {
                    _toastService.Show($"Did not find local MacroSet: {macroSet.Source}");
                    return;
                }

                macroSetJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetName, "macroSet.json"));
                targetMacroSet = JsonSerializer.Deserialize<MacroSet>(macroSetJson, _jsonSerializerOptions);
                if (!macroSet.PatternsLastUpdated.HasValue || macroSet.PatternsLastUpdated < targetMacroSet.PatternsLastUpdated)
                {
                    pattternJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetName, "patterns.json"));
                }

                if (!macroSet.ScriptsLastUpdated.HasValue || macroSet.ScriptsLastUpdated < targetMacroSet.ScriptsLastUpdated)
                {
                    var scriptList = ServiceHelper.ListAssets(Path.Combine("MacroSets", macroSetName, "scripts"));
                    foreach (var scriptFile in scriptList)
                    {
                        var scriptText = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetName, "scripts", scriptFile));
                        nameToScript.Add(Path.GetFileNameWithoutExtension(scriptFile), scriptText);
                    }
                }

                if (!macroSet.SettingsLastUpdated.HasValue || macroSet.SettingsLastUpdated < targetMacroSet.SettingsLastUpdated)
                {
                    settingJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", macroSetName, "settings.json"));
                }
            }
            else // online from public github
            {
                var macroSetName = macroSet.Source.Substring(7);

                var commitInfoUrl = $"https://github.com/kappagacha/yeetmacro2/tree-commit-info/{_targetBranch}/YeetMacro2/Resources/Raw/MacroSets/{macroSetName}";
                var rawUrl = $"https://raw.githubusercontent.com/kappagacha/yeetmacro2/{_targetBranch}/YeetMacro2/Resources/Raw/MacroSets/{macroSetName}";
                macroSetJson = await _httpService.GetAsync(Path.Combine(rawUrl, "macroSet.json"), new Dictionary<string, string>() { { "Accept", "application/json" } });
                targetMacroSet = JsonSerializer.Deserialize<MacroSet>(macroSetJson, _jsonSerializerOptions);
                if (!macroSet.PatternsLastUpdated.HasValue || macroSet.PatternsLastUpdated < targetMacroSet.PatternsLastUpdated)
                {
                    pattternJson = await _httpService.GetAsync(Path.Combine(rawUrl, "patterns.json"), new Dictionary<string, string>() { { "Accept", "application/json" } });
                }

                if (!macroSet.ScriptsLastUpdated.HasValue || macroSet.ScriptsLastUpdated < targetMacroSet.ScriptsLastUpdated)
                {
                    var strScripts = await _httpService.GetAsync(Path.Combine(commitInfoUrl, "scripts"), new Dictionary<string, string>() { { "Accept", "application/json" } });
                    var jsonScripts = JsonSerializer.Deserialize<JsonObject>(strScripts);
                    foreach (var script in jsonScripts)
                    {
#if ANDROID
                        if (script.Key.StartsWith('_') && !ServiceHelper.GetService<YeetMacro2.Platforms.Android.ViewModels.AndriodHomeViewModel>().InDeveloperMode) continue;
#endif
                        var scriptText = await _httpService.GetAsync(Path.Combine(rawUrl, "scripts", script.Key), new Dictionary<string, string>() { { "Accept", "application/json" } });
                        nameToScript.Add(Path.GetFileNameWithoutExtension(script.Key), scriptText);
                    }
                }

                if (!macroSet.SettingsLastUpdated.HasValue || macroSet.SettingsLastUpdated < targetMacroSet.SettingsLastUpdated)
                {
                    settingJson = await _httpService.GetAsync(Path.Combine(rawUrl, "settings.json"), new Dictionary<string, string>() { { "Accept", "application/json" } });
                }
            }

            await Patterns.WaitForInitialization();
            await Scripts.WaitForInitialization();
            await Settings.WaitForInitialization();

            if (pattternJson is not null)
            {
                var patterns = PatternNodeManagerViewModel.FromJson(pattternJson);
                Patterns.Import(patterns);
            }

            if (nameToScript.Count > 0)
            {
                var scripts = new ScriptNodeManagerViewModel(-1, null, null, null) { Root = new ScriptNode() { Nodes = new List<ScriptNode>() } };
                foreach (var scriptKvp in nameToScript)
                {
                    var script = new ScriptNode()
                    {
                        Name = scriptKvp.Key,
                        Text = scriptKvp.Value,
                        RootId = Scripts.Root.NodeId,
                        ParentId = Scripts.Root.NodeId
                    };

                    scripts.Root.Nodes.Add(script);
                }
                Scripts.Import(scripts);
            }

            if (settingJson is not null)
            {
                var settings = SettingNodeManagerViewModel.FromJson(settingJson);
                Settings.SelectedNode = null;
                MergeSettings(Settings.Root, settings.Root);
                Settings.Import(settings);
            }

            _mapper.Map(targetMacroSet, macroSet, opt =>
            {
                opt.BeforeMap((src, dst) =>     // prevent db id from getting wiped out
                {
                    src.MacroSetId = dst.MacroSetId;
                    src.RootScriptNodeId = dst.RootScriptNodeId;
                    src.RootPatternNodeId = dst.RootPatternNodeId;
                    src.RootSettingNodeId = dst.RootSettingNodeId;
                    src.RootDailyNodeId = dst.RootDailyNodeId;
                    src.Source = dst.Source;
                });
            });

            _macroSetRepository.Update(macroSet);
            _macroSetRepository.Save();
            OnSelectedMacroSetChanged(macroSet);
            _toastService.Show($"Updated MacroSet: {macroSet.Name}");
        }
        catch (Exception ex)
        {
            _toastService.Show($"Error Updating MacroSet: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
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
        Preferences.Default.Set(nameof(SelectedMacroSet), value.Name);
        OnPropertyChanged(nameof(Patterns));
        OnPropertyChanged(nameof(Scripts));
        OnPropertyChanged(nameof(Settings));
    }

    private void MergeSettings(SettingNode source, SettingNode dest)
    {
        if (source is ParentSetting parentSource && dest is ParentSetting parentDest)
        {
            foreach (var childSource in parentSource.Nodes)
            {
                // Not supporting duplicate names
                var childDest = parentDest.Nodes.FirstOrDefault(sn => sn.Name == childSource.Name);
                if (childDest is not null)
                {
                    MergeSettings(childSource, childDest);
                }
            }
        } 
        else
        {
            switch (source.SettingType)
            {
                case SettingType.Boolean when dest.SettingType == SettingType.Boolean:
                    ((SettingNode<bool>)dest).Value = ((SettingNode<bool>)source).Value;
                    break;
                case SettingType.String when dest.SettingType == SettingType.String:
                    ((SettingNode<string>)dest).Value = ((SettingNode<string>)source).Value;
                    break;
                case SettingType.Pattern when dest.SettingType == SettingType.Pattern:
                    ((SettingNode<PatternNode>)dest).Value = ((SettingNode<PatternNode>)source).Value;
                    break;
            }
        }
    }

    [RelayCommand]
    private void ClearMacroSetLastUpdated(MacroSet macroSet)
    {
        macroSet.MacroSetLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearPatternsLastUpdated(MacroSet macroSet)
    {
        macroSet.PatternsLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearScriptsLastUpdated(MacroSet macroSet)
    {
        macroSet.ScriptsLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearSettingsLastUpdated(MacroSet macroSet)
    {
        macroSet.SettingsLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    partial void OnInDebugModeChanged(bool oldValue, bool newValue)
    {
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(InDebugMode), oldValue, newValue), nameof(MacroManagerViewModel));
    }

    partial void OnShowStatusPanelChanged(bool oldValue, bool newValue)
    {
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(ShowStatusPanel), oldValue, newValue), nameof(MacroManagerViewModel));
    }
}
