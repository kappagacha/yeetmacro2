using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class PatternNodeViewModel : NodeViewModel<PatternNode, PatternNode>
{
    IRepository<Pattern> _patternRepository;
    IScreenService _screenService;
    Resolution _currentResolution;
    [ObservableProperty]
    Pattern _selectedPattern;

    public Resolution CurrentResolution => _currentResolution ?? (_currentResolution = new Resolution()
    {
        Width = DeviceDisplay.MainDisplayInfo.Width,
        Height = DeviceDisplay.MainDisplayInfo.Height
    });

    public PatternNodeViewModel(
        int rootNodeId,
        INodeService<PatternNode, PatternNode> nodeService,
        IInputService inputService,
        IToastService toastService,
        IRepository<Pattern> patternRepository,
        IScreenService screenService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _patternRepository = patternRepository;
        _screenService = screenService;

        PropertyChanged += PatternTreeViewViewModel_PropertyChanged;

        // TODO: init SelectedPattern
        //if (SelectedPattern == null && SelectedNode.Patterns.Count > 0)
        //{
        //    var targetPattern = SelectedNode.Patterns.First();
        //    targetPattern.IsSelected = false;
        //    SelectPattern(targetPattern);
        //}
    }

    private void PatternTreeViewViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedNode) && SelectedNode != null && SelectedNode.Patterns.Count > 0)
        {
            var targetPattern = SelectedNode.Patterns.First();
            targetPattern.IsSelected = false;
            SelectPattern(targetPattern);
        } 
        else if (e.PropertyName == nameof(SelectedNode))
        {
            SelectedPattern = null;
        }
    }

    [RelayCommand]
    private async void AddPattern(PatternNode patternNode)
    {
        if (SelectedNode != null)
        {
            var name = await _inputService.PromptInput("Please enter pattern name: ");
            if (string.IsNullOrWhiteSpace(name))
            {
                _toastService.Show("Canceled add pattern");
                return;
            }

            var newPattern = ProxyViewModel.Create(new Pattern() { Name = name });
            patternNode.Patterns.Add(newPattern);
            _patternRepository.Insert(newPattern);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private void SelectPattern(Pattern pattern)
    {
        if (SelectedPattern != null && SelectedPattern != pattern)
        {
            SelectedPattern.IsSelected = false;
        }

        if (pattern == null)
        {
            SelectedPattern = null;
            return;
        }

        pattern.IsSelected = !pattern.IsSelected;

        if (pattern.IsSelected && SelectedPattern != pattern)
        {
            SelectedPattern = pattern;
        }
        else if (!pattern.IsSelected)
        {
            SelectedPattern = null;
        }
    }

    [RelayCommand]
    private void DeletePattern(Object[] values)
    {
        if (values.Length == 2 && values[0] is Pattern pattern && values[1] is PatternNode patternNode)
        {
            patternNode.Patterns.Remove(pattern);
            _patternRepository.Delete(pattern);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private async void CapturePattern(Object[] values)
    {
        if (values.Length > 1 && values[1] is PatternNode patternNode)
        {
            var pattern = values[0] as Pattern;
            if (pattern == null)
            {
                pattern = ProxyViewModel.Create(new Pattern() { Name = "pattern" });
                patternNode.Patterns.Add(pattern);
                _patternRepository.Insert(pattern);
                _patternRepository.Save();
                SelectPattern(pattern);
            }

            var bounds = await _inputService.DrawUserRectangle();
            pattern.ImageData = await _screenService.GetCurrentImageData(bounds.start, bounds.end);
            pattern.Bounds = ProxyViewModel.Create(new Bounds() { Start = bounds.start, End = bounds.end });
            pattern.Resolution = ProxyViewModel.Create(new Resolution()
            {
                Width = DeviceDisplay.MainDisplayInfo.Width,
                Height = DeviceDisplay.MainDisplayInfo.Height
            });
            _patternRepository.Update(pattern);
            _patternRepository.Save();

            if (values.Length > 2 && values[2] is ICommand selectCommand)
            {
                // Annoying that OnPropertyChanged(nameof(SelectedPattern)) won't work because the value hasn't changed
                selectCommand.Execute(null);
                selectCommand.Execute(pattern);
            }
        }
    }

    [RelayCommand]
    private void SavePattern(Pattern pattern)
    {
        _patternRepository.Update(pattern);
        _patternRepository.Save();
    }

    [RelayCommand]
    private async void SetPatternBounds(Pattern pattern)
    {
        if (pattern == null) return;

        var bounds = await _inputService.DrawUserRectangle();
        if (bounds.start != Point.Zero && bounds.end != Point.Zero)
        {
            pattern.Bounds = new Bounds() { Start = bounds.start, End = bounds.end };
            _patternRepository.Update(pattern);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private async void TestPattern(Pattern pattern)
    {
        if (pattern == null) return;

        _screenService.DrawClear();
        var result = await _screenService.FindPattern(pattern, new FindOptions() { Limit = 10 });
        var points = result.Points;
        _toastService.Show(points != null && points.Length > 0 ? "Match(es) found" : "No match found");

        if (pattern.Bounds != null)
        {
            _screenService.DrawRectangle(pattern.Bounds.Start, pattern.Bounds.End);
        }

        if (points != null)
        {
            foreach (var point in points)
            {
                _screenService.DrawCircle(point);
            }
        }
    }

    // For this to work, Android view needs to not be touchable or do a double click
    [RelayCommand]
    private async void ClickPattern(Pattern pattern)
    {
        if (pattern == null) return;
        await _screenService.ClickPattern(pattern);     //one to change focus
        await Task.Delay(300);
        await _screenService.ClickPattern(pattern);     //one to click
    }

    [RelayCommand]
    private void ApplyColorThreshold(Object[] values)
    {
        if (values.Length != 4) return;
        
        if (values[0] is Pattern pattern && 
            values[1] is string colorThresholdVariancePctString && 
            double.TryParse(colorThresholdVariancePctString, out double colorThresholdVariancePct) && 
            values[2] is string colorThresholdColor &&
            values[3] is ICommand selectCommand)
        {
            pattern.ColorThreshold.VariancePct = colorThresholdVariancePct;
            pattern.ColorThreshold.Color = colorThresholdColor;
            pattern.ColorThreshold.IsActive = true;
            pattern.ColorThreshold.ImageData = _screenService.CalcColorThreshold(pattern, pattern.ColorThreshold);
            _patternRepository.Update(pattern);
            _patternRepository.Save();
            selectCommand.Execute(null);
            selectCommand.Execute(pattern);
        }
    }

    [RelayCommand]
    private async void TestPatternTextMatch(Pattern pattern)
    {
        if (pattern == null) return;
        if (pattern.Bounds != null)
        {
            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Bounds.Start, pattern.Bounds.End);
        }
        var result = await _screenService.GetText(pattern);
        _toastService.Show($"TextMatch: {result}");
    }

    [RelayCommand]
    private async void ApplyPatternTextMatch(Pattern pattern)
    {
        if (pattern == null) return;
        if (pattern.Bounds != null)
        {
            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Bounds.Start, pattern.Bounds.End);
        }
        var result = await _screenService.GetText(pattern);
        _toastService.Show($"TextMatch Apply: {result}");
        pattern.TextMatch.Text = result;

        _patternRepository.Update(pattern);
        _patternRepository.Save();
    }
}