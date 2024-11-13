using System.Collections.Concurrent;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Views;

public partial class ScriptNodeView : ContentView
{
    public static readonly BindableProperty ShowExecuteButtonProperty =
        BindableProperty.Create(nameof(ShowExecuteButton), typeof(bool), typeof(ScriptNodeView), false);
    public bool ShowExecuteButton
    {
        get { return (bool)GetValue(ShowExecuteButtonProperty); }
        set { SetValue(ShowExecuteButtonProperty, value); }
    }

    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(ScriptNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    static ConcurrentDictionary<ParentSetting, View> _settingSubViewModelToView = new ConcurrentDictionary<ParentSetting, View>();
    static ConcurrentDictionary<TodoJsonParentViewModel, View> _todoSubViewModelToView = new ConcurrentDictionary<TodoJsonParentViewModel, View>();

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        var scriptNodeView = bindable as ScriptNodeView;
        var settingsPropertyChanged = new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingNodeManagerViewModel.CurrentSubViewModel) &&
                scriptNodeView.MacroSet.Settings.CurrentSubViewModel is ParentSetting settingsSubViewModel)
            {
                if (!_settingSubViewModelToView.ContainsKey(settingsSubViewModel))
                {
                    var settingNodeView = new SettingNodeView() { MacroSet = scriptNodeView.MacroSet, SubView = settingsSubViewModel };
                    _settingSubViewModelToView.TryAdd(settingsSubViewModel, settingNodeView);
                }

                scriptNodeView.settingsContentPresenter.Content = _settingSubViewModelToView[settingsSubViewModel];
            }
        });

        var dailiesPropertyChanged = new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DailyNodeManagerViewModel.CurrentSubViewModel) &&
                 scriptNodeView.MacroSet.Dailies.CurrentSubViewModel is TodoJsonParentViewModel todoSubViewModel)
            {
                if (!_todoSubViewModelToView.ContainsKey(todoSubViewModel))
                {
                    var todoNodeView = new TodoNodeView() { Todos = scriptNodeView.MacroSet.Dailies, SubView = todoSubViewModel, IsVisible = todoSubViewModel.Children.Count > 0 };
                    _todoSubViewModelToView.TryAdd(todoSubViewModel, todoNodeView);
                }

                scriptNodeView.dailiesContentPresenter.Content = _todoSubViewModelToView[todoSubViewModel];
            }
        });

        var weekliesPropertyChanged = new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WeeklyNodeManagerViewModel.CurrentSubViewModel) &&
                scriptNodeView.MacroSet.Weeklies.CurrentSubViewModel is TodoJsonParentViewModel todoSubViewModel)
            {
                if (!_todoSubViewModelToView.ContainsKey(todoSubViewModel))
                {
                    var todoNodeView = new TodoNodeView() { Todos = scriptNodeView.MacroSet.Weeklies, SubView = todoSubViewModel, IsVisible = todoSubViewModel.Children.Count > 0 };
                    _todoSubViewModelToView.TryAdd(todoSubViewModel, todoNodeView);
                }

                scriptNodeView.weekliesContentPresenter.Content = _todoSubViewModelToView[todoSubViewModel];
            }
        });

        if (newValue is MacroSetViewModel macroSet)
        {
            scriptNodeView.BindingContext = macroSet;
            macroSet.Settings.PropertyChanged += settingsPropertyChanged;
            macroSet.Dailies.PropertyChanged += dailiesPropertyChanged;
            macroSet.Weeklies.PropertyChanged += weekliesPropertyChanged;
            settingsPropertyChanged.Invoke(null, new PropertyChangedEventArgs(nameof(SettingNodeManagerViewModel.CurrentSubViewModel)));
            dailiesPropertyChanged.Invoke(null, new PropertyChangedEventArgs(nameof(DailyNodeManagerViewModel.CurrentSubViewModel)));
            weekliesPropertyChanged.Invoke(null, new PropertyChangedEventArgs(nameof(WeeklyNodeManagerViewModel.CurrentSubViewModel)));
        }

        if (oldValue is MacroSetViewModel oldMacroSet)
        {
            oldMacroSet.Settings.PropertyChanged -= settingsPropertyChanged;
            oldMacroSet.Dailies.PropertyChanged -= dailiesPropertyChanged;
            oldMacroSet.Weeklies.PropertyChanged -= weekliesPropertyChanged;
        }
    }

    public ScriptNodeView()
    {
        InitializeComponent();
    }

    private void ScriptEditor_SelectAll(object sender, EventArgs e)
    {
        if (scriptEditor.Text == null) return;
        scriptEditor.Focus();
        scriptEditor.CursorPosition = 0;
        scriptEditor.SelectionLength = scriptEditor.Text.Length;
    }
}