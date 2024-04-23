using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels;

[ObservableObject]
public partial class MacroSetViewModel : MacroSet
{
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

    public override string Description
    {
        get => base.Description;
        set
        {
            base.Description = value;
            OnPropertyChanged();
        }
    }

    public override string DailyTemplate
    {
        get => base.DailyTemplate;
        set
        {
            base.DailyTemplate = value;
            OnPropertyChanged();
        }
    }

    public override string WeeklyTemplate
    {
        get => base.WeeklyTemplate;
        set
        {
            base.WeeklyTemplate = value;
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

    public bool IsLeaf
    {
        get => true;
    }
}