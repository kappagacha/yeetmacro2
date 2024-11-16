using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
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
    ICollection<MacroSetViewModel> _macroSets;
    [ObservableProperty]
    MacroSetViewModel _selectedMacroSet;
    readonly IScriptService _scriptService;
    [ObservableProperty]
    bool _isExportEnabled, _isOpenAppDirectoryEnabled, _inDebugMode, 
        _showExport, _isBusy, _showMacroSetDescriptionEditor;
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
        MacroSets = new ObservableCollection<MacroSetViewModel>(tempMacroSets.Select(ms => _mapper.Map<MacroSetViewModel>(ms)));

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
                !(SelectedMacroSet.Settings?.IsInitialized ?? false) ||      // loading settings     (NodeManagerViewModel)
                settingNode.NodeId == 0)                    // json desirialization (NodeManagerViewModel)
                return;

            SelectedMacroSet.Settings?.SaveSetting(settingNode);
        });

        WeakReferenceMessenger.Default.Register<Lazy<ScriptNode>>(this, (r, scriptNode) => {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await SelectedMacroSet.Settings?.OnScriptNodeSelected(scriptNode.Value);
                await SelectedMacroSet.Dailies?.OnScriptNodeSelected(scriptNode.Value);
                await SelectedMacroSet.Weeklies?.OnScriptNodeSelected(scriptNode.Value);
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
            if (IsBusy ||                                           // updating MacroSet   (MacroManagerViewModel)
                SelectedMacroSet.Patterns.IsBusy ||                 // updating patterns
                !(SelectedMacroSet.Patterns.IsInitialized) ||       // loading patterns     (NodeManagerViewModel)
                patternNode.NodeId == 0)                            // json desirialization (NodeManagerViewModel)
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

        var macroSet = new MacroSetViewModel(_nodeViewModelManagerFactory, _scriptService) { Name = macroSetName, Source = source };

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
    public async Task DeleteMacroSet(MacroSetViewModel macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        IsBusy = true;
        await macroSet.WaitForInitialization();
        SelectedMacroSet = null;
        _patternNodeService.Delete(macroSet.Patterns.Root);
        _settingNodeService.Delete(macroSet.Settings.Root);
        _scriptNodeService.Delete(macroSet.Scripts.Root);
        _todoNodeService.Delete(macroSet.Dailies.Root);
        _todoNodeService.Delete(macroSet.Weeklies.Root);
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
    private void Save(MacroSetViewModel macroSet)
    {
        macroSet.Resolution = new Size(ResolutionWidth, ResolutionHeight);
        macroSet.DefaultLocation = new Point(DefaultLocationX, DefaultLocationY);
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
        _toastService.Show($"Saved MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task ExportMacroSet(MacroSetViewModel macroSet)
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

        await macroSet.Dailies.WaitForInitialization();
        if (macroSet.Dailies.Root.Data is not null)
        {
            macroSet.DailyTemplate = macroSet.Dailies.Root.Data;
            macroSet.DailyTemplateLastUpdated = DateTimeOffset.Now;
        }

        await macroSet.Weeklies.WaitForInitialization();
        if (macroSet.Weeklies.Root.Data is not null)
        {
            macroSet.WeeklyTemplate = macroSet.Weeklies.Root.Data;
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
        
        await macroSet.Patterns.WaitForInitialization();
        await macroSet.Settings.WaitForInitialization();
        File.WriteAllText(Path.Combine(targetDirectory, $"macroSet.json"), macroSetJson);
        File.WriteAllText(Path.Combine(targetDirectory, $"patterns.json"), SelectedMacroSet.Patterns.ToJson());
        macroSet.Settings.Traverse(macroSet.Settings.Root, s =>
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
        File.WriteAllText(Path.Combine(targetDirectory, $"settings.json"), macroSet.Settings.ToJson());

        ExportValue = macroSetJson;
        ShowExport = true;
        IsBusy = false;
        _toastService.Show($"Exported MacroSet: {macroSet.Name}");
    }

    [RelayCommand]
    private async Task UpdateMacroSet(MacroSetViewModel macroSet)
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

            await macroSet.WaitForInitialization();

            if (pattternJson is not null)
            {
                macroSet.Patterns.SelectedNode = null;
                var patterns = PatternNodeManagerViewModel.FromJson(pattternJson);
                ((PatternNodeViewModel)macroSet.Patterns.Root).ResetDictionary();
                macroSet.Patterns.Import(patterns);
            }

            if (nameToScript.Count > 0)
            {
                macroSet.Scripts.SelectedNode = null;
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
                        RootId = macroSet.Scripts.Root.NodeId,
                        ParentId = macroSet.Scripts.Root.NodeId,
                        IsHidden = scriptKvp.Key.StartsWith('_'),
                        IsFavorite = scriptKvp.Value.Contains("@isFavorite"),
                        Position = position
                    };

                    scripts.Root.Nodes.Add(script);
                }
                ScriptNodeManagerViewModel.MergeSettings(macroSet.Scripts.Root, scripts.Root);
                macroSet.Scripts.Import(scripts);
            }

            if (settingJson is not null)
            {
                macroSet.Settings.SelectedNode = null;
                macroSet.Settings.IsBusy = true;
                var settings = SettingNodeManagerViewModel.FromJson(settingJson);
                settings.Root = _mapper.Map<ParentSettingViewModel>(settings.Root);
                SettingNodeManagerViewModel.MergeSettings(macroSet.Settings.Root, settings.Root);
                ((ParentSettingViewModel)macroSet.Settings.Root).ResetDictionary();

                macroSet.Settings.Import(settings);
                macroSet.Settings.IsBusy = false;
            }

            if (targetMacroSet.DailyTemplate is not null && (!macroSet.DailyTemplateLastUpdated.HasValue || macroSet.DailyTemplateLastUpdated < targetMacroSet.DailyTemplateLastUpdated))
            {
                macroSet.Dailies.Root.Data = targetMacroSet.DailyTemplate;
                macroSet.Dailies.Save();
            }

            if (targetMacroSet.WeeklyTemplate is not null && (!macroSet.WeeklyTemplateLastUpdated.HasValue || macroSet.WeeklyTemplateLastUpdated < targetMacroSet.WeeklyTemplateLastUpdated))
            {
                macroSet.Weeklies.Root.Data = targetMacroSet.WeeklyTemplate;
                macroSet.Weeklies.Save();
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
#if !WINDOWS 
            Vibration.Default.Vibrate(100);
#endif
        }
    }

    [RelayCommand]
    private void CloseExport()
    {
        ShowExport = false;
    }

    partial void OnSelectedMacroSetChanged(MacroSetViewModel value)
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
    }

    [RelayCommand]
    private void ClearMacroSetLastUpdated(MacroSetViewModel macroSet)
    {
        macroSet.MacroSetLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearPatternsLastUpdated(MacroSetViewModel macroSet)
    {
        macroSet.PatternsLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearScriptsLastUpdated(MacroSetViewModel macroSet)
    {
        macroSet.ScriptsLastUpdated = null;
        _macroSetRepository.Update(macroSet);
        _macroSetRepository.Save();
    }

    [RelayCommand]
    private void ClearSettingsLastUpdated(MacroSetViewModel macroSet)
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
