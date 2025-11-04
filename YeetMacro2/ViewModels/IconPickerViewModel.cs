using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Reflection;
using UraniumUI.Icons.FontAwesome;
using UraniumUI.Icons.MaterialSymbols;

namespace YeetMacro2.ViewModels;

public partial class IconPickerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private ObservableCollection<IconItem> _filteredIcons = new();

    [ObservableProperty]
    private IconItem _selectedIcon;

    private List<IconData> _allIcons = new();
    private TaskCompletionSource<bool> _taskCompletionSource;
    private CancellationTokenSource _searchDebounceTokenSource;

    public string SelectedGlyph { get; private set; }
    public string SelectedFontFamilyResult { get; private set; }
    public bool WasConfirmed { get; private set; }

    public Task<bool> ShowAsync()
    {
        _taskCompletionSource = new TaskCompletionSource<bool>();
        return _taskCompletionSource.Task;
    }

    public IconPickerViewModel()
    {
        InitializeIconDatabase();
        LoadIcons();
    }

    partial void OnSearchTextChanged(string value)
    {
        // Cancel previous search debounce
        _searchDebounceTokenSource?.Cancel();
        _searchDebounceTokenSource = new CancellationTokenSource();
        var token = _searchDebounceTokenSource.Token;

        // Debounce search by 300ms
        Task.Run(async () =>
        {
            await Task.Delay(300, token);
            if (!token.IsCancellationRequested)
            {
                MainThread.BeginInvokeOnMainThread(() => LoadIcons());
            }
        }, token);
    }

    private void InitializeIconDatabase()
    {
        _allIcons = new List<IconData>();

        // FontAwesome Solid icons
        AddIconsFromType("FASolid", typeof(Solid));

        // FontAwesome Regular icons
        AddIconsFromType("FARegular", typeof(Regular));

        // Material Outlined icons
        AddIconsFromType("MaterialOutlined", typeof(MaterialOutlined));

        // Material Sharp icons
        AddIconsFromType("MaterialSharp", typeof(MaterialSharp));
    }

    private void AddIconsFromType(string fontFamily, Type iconType)
    {
        var fields = iconType.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(string))
            {
                var glyphValue = field.GetValue(null) as string;
                if (!string.IsNullOrEmpty(glyphValue))
                {
                    _allIcons.Add(new IconData
                    {
                        FontFamily = fontFamily,
                        Glyph = glyphValue,
                        FieldName = field.Name
                    });
                }
            }
        }
    }

    private void LoadIcons()
    {
        FilteredIcons.Clear();

        var icons = _allIcons.AsEnumerable();

        // Search by field name instead of glyph value
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            icons = icons.Where(icon => icon.FieldName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var iconData in icons.Take(100)) // Limit to 100 icons for performance
        {
            FilteredIcons.Add(new IconItem
            {
                FontFamily = iconData.FontFamily,
                Glyph = iconData.Glyph,
                FieldName = iconData.FieldName,
                SelectCommand = new RelayCommand(() => OnIconSelected(iconData))
            });
        }
    }

    private void OnIconSelected(IconData iconData)
    {
        foreach (var icon in FilteredIcons)
        {
            icon.IsSelected = icon.Glyph == iconData.Glyph && icon.FontFamily == iconData.FontFamily;
        }
        SelectedGlyph = iconData.Glyph;
        SelectedFontFamilyResult = iconData.FontFamily;
    }

    [RelayCommand]
    private async Task Confirm()
    {
        if (string.IsNullOrEmpty(SelectedGlyph))
        {
            return;
        }

        WasConfirmed = true;
        await Shell.Current.Navigation.PopModalAsync();
        _taskCompletionSource?.TrySetResult(true);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        WasConfirmed = false;
        await Shell.Current.Navigation.PopModalAsync();
        _taskCompletionSource?.TrySetResult(false);
    }
}

public class IconData
{
    public string FontFamily { get; set; }
    public string Glyph { get; set; }
    public string FieldName { get; set; }
}

public partial class IconItem : ObservableObject
{
    [ObservableProperty]
    private string _fontFamily;

    [ObservableProperty]
    private string _glyph;

    [ObservableProperty]
    private string _fieldName;

    [ObservableProperty]
    private bool _isSelected;

    public RelayCommand SelectCommand { get; set; }
}
