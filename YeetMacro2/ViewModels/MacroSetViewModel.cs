using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Messaging;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.ViewModels;

[ObservableObject]
public partial class MacroSetViewModel : MacroSet
{
    readonly NodeManagerViewModelFactory _nodeViewModelManagerFactory;
    readonly IScriptService _scriptService;
    PatternNodeManagerViewModel _patterns;
    ScriptNodeManagerViewModel _scripts;
    SettingNodeManagerViewModel _settings;
    DailyNodeManagerViewModel _dailies;
    WeeklyNodeManagerViewModel _weeklies;
    
    [ObservableProperty]
    [property: JsonIgnore]  // https://stackoverflow.com/questions/74599937/is-there-any-other-way-of-ignoring-a-property-during-json-serialization-instead
    bool _isScriptRunning, _isBusy;

    public MacroSetViewModel(NodeManagerViewModelFactory nodeViewModelManagerFactory, IScriptService scriptService)
    {
        _nodeViewModelManagerFactory = nodeViewModelManagerFactory;
        _scriptService = scriptService;
    }

    [JsonIgnore]
    public PatternNodeManagerViewModel Patterns
    {
        get
        {
            if (_patterns == null)
            {
                _patterns = _nodeViewModelManagerFactory.Create<PatternNodeManagerViewModel>(RootPatternNodeId);
            }

            return _patterns;
        }
    }

    [JsonIgnore]
    public SettingNodeManagerViewModel Settings
    {
        get
        {
            if (_settings == null)
            {
                _settings = _nodeViewModelManagerFactory.Create<SettingNodeManagerViewModel>(RootSettingNodeId);
            }

            return _settings;
        }
    }

    [JsonIgnore]
    public ScriptNodeManagerViewModel Scripts
    {
        get
        {
            if (_scripts == null)
            {
                _scripts = _nodeViewModelManagerFactory.Create<ScriptNodeManagerViewModel>(RootScriptNodeId);
            }

            return _scripts;
        }
    }

    [JsonIgnore]
    public DailyNodeManagerViewModel Dailies
    {
        get
        {
            if (_dailies == null)
            {
                _dailies = _nodeViewModelManagerFactory.Create<DailyNodeManagerViewModel>(RootDailyNodeId);
            }

            return _dailies;
        }
    }

    [JsonIgnore]
    public WeeklyNodeManagerViewModel Weeklies
    {
        get
        {
            if (_weeklies == null)
            {
                _weeklies = _nodeViewModelManagerFactory.Create<WeeklyNodeManagerViewModel>(RootWeeklyNodeId);
            }

            return _weeklies;
        }
    }

    public override string Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            OnPropertyChanged();
        }
    }

    public override Size Resolution
    {
        get => base.Resolution;
        set
        {
            base.Resolution = value;
            OnPropertyChanged();
        }
    }

    public override bool SupportsGreaterWidth
    {
        get => base.SupportsGreaterWidth;
        set
        {
            base.SupportsGreaterWidth = value;
            OnPropertyChanged();
        }
    }

    public override bool SupportsGreaterHeight
    {
        get => base.SupportsGreaterHeight;
        set
        {
            base.SupportsGreaterHeight = value;
            OnPropertyChanged();
        }
    }

    public override string Package
    {
        get => base.Package;
        set
        {
            base.Package = value;
            OnPropertyChanged();
        }
    }

    public override string Source
    {
        get => base.Source;
        set
        {
            base.Source = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? MacroSetLastUpdated
    {
        get => base.MacroSetLastUpdated;
        set
        {
            base.MacroSetLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? PatternsLastUpdated
    {
        get => base.PatternsLastUpdated;
        set
        {
            base.PatternsLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? ScriptsLastUpdated
    {
        get => base.ScriptsLastUpdated;
        set
        {
            base.ScriptsLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? SettingsLastUpdated
    {
        get => base.SettingsLastUpdated;
        set
        {
            base.SettingsLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? DailyTemplateLastUpdated
    {
        get => base.DailyTemplateLastUpdated;
        set
        {
            base.DailyTemplateLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override DateTimeOffset? WeeklyTemplateLastUpdated
    {
        get => base.WeeklyTemplateLastUpdated;
        set
        {
            base.WeeklyTemplateLastUpdated = value;
            OnPropertyChanged();
        }
    }

    public override string Description
    {
        get => base.Description;
        set
        {
            base.Description = value;
            OnPropertyChanged();
        }
    }

    public override int DailyResetUtcHour
    {
        get => base.DailyResetUtcHour;
        set
        {
            base.DailyResetUtcHour = value;
            OnPropertyChanged();
        }
    }

    public override DayOfWeek WeeklyStartDay
    {
        get => base.WeeklyStartDay;
        set
        {
            base.WeeklyStartDay = value;
            OnPropertyChanged();
        }
    }

    public override bool IgnoreCutoutInOffsetCalculation
    {
        get => base.IgnoreCutoutInOffsetCalculation;
        set
        {
            base.IgnoreCutoutInOffsetCalculation = value;
            OnPropertyChanged();
        }
    }

    public override bool UsePatternsSnapshot
    {
        get => base.UsePatternsSnapshot;
        set
        {
            var hasChanged = base.UsePatternsSnapshot != value;
            base.UsePatternsSnapshot = value;
            OnPropertyChanged();

            if (IsBusy || !hasChanged) return;

            if (base.UsePatternsSnapshot && _patterns is not null)
            {
                _patterns.TakeSnapshot();
            }
            else if (_patterns is not null && !_patterns.IsInitialized)
            {
                _patterns.ForceInit();
            }
        }
    }

    public async Task WaitForInitialization()
    {
        if (!UsePatternsSnapshot) await Patterns.WaitForInitialization();
        await Scripts.WaitForInitialization();
        await Settings.WaitForInitialization();
        await Dailies.WaitForInitialization();
        await Weeklies.WaitForInitialization();
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task ExecuteScript(ScriptNodeViewModel scriptNode)
    {
        if (IsScriptRunning) return;

        await WaitForInitialization();

        await Task.Run(() =>
        {
            Console.WriteLine($"[*****YeetMacro*****] MacroManagerViewModel ExecuteScript");
            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() { Type = ScriptEventType.Started }));
            var result = _scriptService.RunScript(scriptNode, this);
            WeakReferenceMessenger.Default.Send(new ScriptEventMessage(new ScriptEvent() { Type = ScriptEventType.Finished, Result = result }));
            IsScriptRunning = false;
        });
    }
}