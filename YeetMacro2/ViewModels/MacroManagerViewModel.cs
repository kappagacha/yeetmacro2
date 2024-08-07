﻿using AutoMapper;
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
using System.Text.RegularExpressions;

namespace YeetMacro2.ViewModels;

public partial class MacroManagerViewModel : ObservableObject
{
    readonly ILogger _logger;
    readonly IRepository<MacroSet> _macroSetRepository;
    readonly IToastService _toastService;
    readonly INodeService<PatternNode, PatternNode> _patternNodeService;
    readonly INodeService<ScriptNode, ScriptNode> _scriptNodeService;
    readonly INodeService<ParentSetting, SettingNode> _settingNodeService;
    readonly INodeService<TodoNode, TodoNode> _todoNodeService;
    readonly NodeManagerViewModelFactory _nodeViewModelManagerFactory;
    readonly IMapper _mapper;
    readonly IHttpService _httpService;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Patterns), nameof(Scripts))]
    MacroSet _selectedMacroSet;
    readonly ConcurrentDictionary<int, PatternNodeManagerViewModel> _nodeRootIdToPatternTree;
    readonly ConcurrentDictionary<int, ScriptNodeManagerViewModel> _nodeRootIdToScriptList;
    readonly ConcurrentDictionary<int, SettingNodeManagerViewModel> _nodeRootIdToSettingTree;
    readonly ConcurrentDictionary<int, DailyNodeManagerViewModel> _nodeRootIdToDailyList;
    readonly ConcurrentDictionary<int, WeeklyNodeManagerViewModel> _nodeRootIdToWeeklyList;
    readonly IScriptService _scriptService;
    [ObservableProperty]
    bool _isExportEnabled, _isOpenAppDirectoryEnabled, _inDebugMode, 
        _showExport, _isBusy, _persistLogs, _showMacroSetDescriptionEditor, _isScriptRunning;
    [ObservableProperty]
    double _resolutionWidth, _resolutionHeight, _defaultLocationX, _defaultLocationY;
    [ObservableProperty]
    string _exportValue, _message;
    readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        Converters = {
                new JsonStringEnumConverter()
        },
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = CombinedPropertiesResolver.Combine(SizePropertiesResolver.Instance, PointPropertiesResolver.Instance)
    };
    readonly string _targetBranch = "main";
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
                var list = _nodeViewModelManagerFactory.Create<ScriptNodeManagerViewModel>(SelectedMacroSet.RootScriptNodeId);
                _nodeRootIdToScriptList.TryAdd(SelectedMacroSet.RootScriptNodeId, list);
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
                var list = _nodeViewModelManagerFactory.Create<DailyNodeManagerViewModel>(SelectedMacroSet.RootDailyNodeId);
                _nodeRootIdToDailyList.TryAdd(SelectedMacroSet.RootDailyNodeId, list);
                list.MacroSet = (MacroSetViewModel)SelectedMacroSet;
            }
            return _nodeRootIdToDailyList[SelectedMacroSet.RootDailyNodeId];
        }
    }

    public WeeklyNodeManagerViewModel Weeklies
    {
        get
        {
            if (SelectedMacroSet == null) return null;
            if (!_nodeRootIdToWeeklyList.ContainsKey(SelectedMacroSet.RootWeeklyNodeId))
            {
                var list = _nodeViewModelManagerFactory.Create<WeeklyNodeManagerViewModel>(SelectedMacroSet.RootWeeklyNodeId);
                _nodeRootIdToWeeklyList.TryAdd(SelectedMacroSet.RootWeeklyNodeId, list);
                list.MacroSet = (MacroSetViewModel)SelectedMacroSet;
            }
            return _nodeRootIdToWeeklyList[SelectedMacroSet.RootWeeklyNodeId];
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
        INodeService<TodoNode, TodoNode> todoNodeService,
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
        _todoNodeService = todoNodeService;
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

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities([.._macroSets]);
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternNodeManagerViewModel>();
        _nodeRootIdToScriptList = new ConcurrentDictionary<int, ScriptNodeManagerViewModel>();
        _nodeRootIdToSettingTree = new ConcurrentDictionary<int, SettingNodeManagerViewModel>();
        _nodeRootIdToDailyList = new ConcurrentDictionary<int, DailyNodeManagerViewModel>();
        _nodeRootIdToWeeklyList = new ConcurrentDictionary<int, WeeklyNodeManagerViewModel>();
        _scriptService = scriptService;

        IsExportEnabled = Preferences.Default.Get(nameof(IsExportEnabled), false);

#if DEBUG
        IsExportEnabled = true;
#endif

#if WINDOWS
        IsOpenAppDirectoryEnabled = true;
#endif

        WeakReferenceMessenger.Default.Register<SettingNode>(this, (r, settingNode) => {
            if (IsBusy ||                                   // updating MacroSet   (MacroManagerViewModel)
                !(Settings?.IsInitialized ?? false) ||      // loading settings     (NodeManagerViewModel)
                settingNode.NodeId == 0)                    // json desirialization (NodeManagerViewModel)
                return;

            Settings?.SaveSetting(settingNode);
        });

        WeakReferenceMessenger.Default.Register<Lazy<ScriptNode>>(this, (r, scriptNode) => {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Settings?.OnScriptNodeSelected(scriptNode.Value);
                await Dailies?.OnScriptNodeSelected(scriptNode.Value);
                await Weeklies?.OnScriptNodeSelected(scriptNode.Value);
            });
        });

        WeakReferenceMessenger.Default.Register<TodoViewModel>(this, (r, todoNode) => {
            if (IsBusy ||                                   // updating MacroSet   (MacroManagerViewModel)
                //!(Dailies?.IsInitialized ?? false) ||       // loading dailies     (NodeManagerViewModel)
                //!(Weeklies?.IsInitialized ?? false) ||      // loading weeklies     (NodeManagerViewModel)
                todoNode.NodeId == 0)                       // json desirialization (NodeManagerViewModel)
                return;
            _todoNodeService.Update(todoNode);
        });

        WeakReferenceMessenger.Default.Register<PatternNodeViewModel>(this, (r, patternNode) => {
            if (IsBusy ||                                   // updating MacroSet   (MacroManagerViewModel)
                Patterns.IsBusy ||                          // updating patterns
                !(Patterns?.IsInitialized ?? false) ||      // loading patterns     (NodeManagerViewModel)
                patternNode.NodeId == 0)                    // json desirialization (NodeManagerViewModel)
                return;

            _patternNodeService.Update(patternNode);
        });

        InDebugMode = Preferences.Default.Get(nameof(InDebugMode), false);
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
        if (source.StartsWith("localAsset:")) macroSetName = source[11..];
        else if (source.StartsWith("online:")) macroSetName = source[7..];

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

        var rootDaily = _mapper.Map<TodoViewModel>(_todoNodeService.GetRoot(0));
        _todoNodeService.ReAttachNodes(rootDaily);
        macroSet.RootDailyNodeId = rootDaily.NodeId;

        var rootWeekly = _mapper.Map<TodoViewModel>(_todoNodeService.GetRoot(0));
        _todoNodeService.ReAttachNodes(rootWeekly);
        macroSet.RootWeeklyNodeId = rootWeekly.NodeId;

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

        IsBusy = true;
        await Patterns.WaitForInitialization();
        await Scripts.WaitForInitialization();
        await Settings.WaitForInitialization();
        await Dailies.WaitForInitialization();
        await Weeklies.WaitForInitialization();
        SelectedMacroSet = null;
        _patternNodeService.Delete(_nodeRootIdToPatternTree[macroSet.RootPatternNodeId].Root);
        _scriptNodeService.Delete(_nodeRootIdToScriptList[macroSet.RootScriptNodeId].Root);
        _settingNodeService.Delete(_nodeRootIdToSettingTree[macroSet.RootSettingNodeId].Root);
        _todoNodeService.Delete(_nodeRootIdToDailyList[macroSet.RootDailyNodeId].Root);
        _todoNodeService.Delete(_nodeRootIdToWeeklyList[macroSet.RootWeeklyNodeId].Root);
        _macroSetRepository.Delete(macroSet);
        _macroSetRepository.Save();
        MacroSets.Remove(macroSet);

        IsBusy = false;
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
        macroSet.DefaultLocation = new Point(DefaultLocationX, DefaultLocationY);
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
        _toastService.Show($"Saved MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task ExecuteScript(ScriptNode scriptNode)
    {
        if (IsScriptRunning) return;

        await Patterns.WaitForInitialization();
        await Settings.WaitForInitialization();
        await Dailies.WaitForInitialization();
        await Weeklies.WaitForInitialization();

        await Task.Run(() =>
        {
            IsScriptRunning = true;
            if (PersistLogs) 
            { 
                _logger.LogInformation("{persistLogs}", true);
                _logger.LogInformation("{macroSet} {script}", SelectedMacroSet?.Name ?? string.Empty, scriptNode.Name);
            };
            Console.WriteLine($"[*****YeetMacro*****] MacroManagerViewModel ExecuteScript");

            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() {  Type = ScriptEventType.Started }));
            var result = _scriptService.RunScript(scriptNode, Scripts, SelectedMacroSet, Patterns, Settings, Dailies, Weeklies);
            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() { Type = ScriptEventType.Finished, Result = result }));
            if (PersistLogs) _logger.LogInformation("{persistLogs}", false);
            IsScriptRunning = false;
        });
    }

    [RelayCommand]
    private async Task ExportMacroSet(MacroSet macroSet)
    {
#if ANDROID
        // https://stackoverflow.com/questions/75880663/maui-on-android-listing-folder-contents-of-an-sd-card-and-writing-in-it
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Android.Content.Intent();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            return;
        }
        else if (!OperatingSystem.IsAndroidVersionAtLeast(30) && await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
        {
            return;
        }
#endif

        IsBusy = true;

        macroSet.MacroSetLastUpdated = DateTimeOffset.Now;
        macroSet.PatternsLastUpdated = DateTimeOffset.Now;
        macroSet.ScriptsLastUpdated = DateTimeOffset.Now;
        macroSet.SettingsLastUpdated = DateTimeOffset.Now;

        await Dailies.WaitForInitialization();
        if (Dailies.Root.Data is not null)
        {
            macroSet.DailyTemplate = Dailies.Root.Data.ToJsonString(JsonSerializerOptions.Default);
            macroSet.DailyTemplateLastUpdated = DateTimeOffset.Now;
        }

        await Weeklies.WaitForInitialization();
        if (Weeklies.Root.Data is not null)
        {
            macroSet.WeeklyTemplate = Weeklies.Root.Data.ToJsonString(JsonSerializerOptions.Default);
            macroSet.WeeklyTemplateLastUpdated = DateTimeOffset.Now;
        }

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
            targetDirectory = Path.Combine(targetDirectory, $"{macroSet.Source[11..]}");
        }
        else
        {
            targetDirectory = Path.Combine(targetDirectory, $"{macroSet.Source[7..]}");
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
            else if (s is SettingNode<PatternNode> patternSetting)
            {
                patternSetting.DefaultValue = patternSetting.Value;
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
            Dictionary<string, string> nameToScript = [];
            if (macroSet.Source.StartsWith("localAsset:"))
            {
                var macroSetName = macroSet.Source[11..];
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
                var macroSetName = macroSet.Source[7..];

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
                Patterns.SelectedNode = null;
                var patterns = PatternNodeManagerViewModel.FromJson(pattternJson);
                ((PatternNodeViewModel)Patterns.Root).ResetDictionary();
                Patterns.Import(patterns);
            }

            if (nameToScript.Count > 0)
            {
                Scripts.SelectedNode = null;
                var scripts = new ScriptNodeManagerViewModel(-1, null, null, null) { Root = new ScriptNode() { Nodes = [] } };
                
                foreach (var scriptKvp in nameToScript)
                {
                    var position = 0;
                    Match positionMatch = PositionRegex().Match(scriptKvp.Value);
                    if (positionMatch.Success)
                    {
                        position = int.Parse(positionMatch.Groups[1].Value);
                    }
                    var script = new ScriptNode()
                    {
                        Name = scriptKvp.Key,
                        Text = scriptKvp.Value,
                        RootId = Scripts.Root.NodeId,
                        ParentId = Scripts.Root.NodeId,
                        IsHidden = scriptKvp.Key.StartsWith('_'),
                        IsFavorite = scriptKvp.Value.Contains("@isFavorite"),
                        Position = position
                    };

                    scripts.Root.Nodes.Add(script);
                }
                Scripts.Import(scripts);
            }

            if (settingJson is not null)
            {
                Settings.SelectedNode = null;
                Settings.IsBusy = true;
                var settings = SettingNodeManagerViewModel.FromJson(settingJson);
                settings.Root = _mapper.Map<ParentSettingViewModel>(settings.Root);
                SettingNodeManagerViewModel.MergeSettings(Settings.Root, settings.Root);
                ((ParentSettingViewModel)Settings.Root).ResetDictionary();
                
                Settings.Import(settings);
                Settings.IsBusy = false;
            }

            if (targetMacroSet.DailyTemplate is not null && (!macroSet.DailyTemplateLastUpdated.HasValue || macroSet.DailyTemplateLastUpdated < targetMacroSet.DailyTemplateLastUpdated))
            {
                await Dailies.WaitForInitialization();
                Dailies.Root.Data = (JsonObject)JsonObject.Parse(targetMacroSet.DailyTemplate, null, default);
                Dailies.Save();
            }

            if (targetMacroSet.WeeklyTemplate is not null && (!macroSet.WeeklyTemplateLastUpdated.HasValue || macroSet.WeeklyTemplateLastUpdated < targetMacroSet.WeeklyTemplateLastUpdated))
            {
                await Weeklies.WaitForInitialization();
                Weeklies.Root.Data = (JsonObject)JsonObject.Parse(targetMacroSet.WeeklyTemplate, null, default);
                Weeklies.Save();
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
                    src.RootWeeklyNodeId = dst.RootWeeklyNodeId;
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
        }
        else
        {
            ResolutionWidth = value.Resolution.Width;
            ResolutionHeight = value.Resolution.Height;
            DefaultLocationX = value.DefaultLocation.X;
            DefaultLocationY = value.DefaultLocation.Y;
            Preferences.Default.Set(nameof(SelectedMacroSet), value.Name);
            WeakReferenceMessenger.Default.Send(value);
        }
        OnPropertyChanged(nameof(Patterns));
        OnPropertyChanged(nameof(Scripts));
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(Dailies));
        OnPropertyChanged(nameof(Weeklies));
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

    [RelayCommand]
    private void ClearDailyTemplateLastUpdated(MacroSet macroSet)
    {
        macroSet.DailyTemplateLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearWeeklyTemplateLastUpdated(MacroSet macroSet)
    {
        macroSet.WeeklyTemplateLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    partial void OnInDebugModeChanged(bool oldValue, bool newValue)
    {
        Preferences.Default.Set(nameof(InDebugMode), InDebugMode);
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(InDebugMode), oldValue, newValue), nameof(MacroManagerViewModel));
    }

    partial void OnIsExportEnabledChanged(bool oldValue, bool newValue)
    {
        Preferences.Default.Set(nameof(IsExportEnabled), IsExportEnabled);
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(IsExportEnabled), oldValue, newValue), nameof(MacroManagerViewModel));
    }

    [GeneratedRegex(@"@position=(-?\d+)")]
    private static partial Regex PositionRegex();
}
