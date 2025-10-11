using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using static System.Net.Mime.MediaTypeNames;

namespace YeetMacro2.ViewModels.NodeViewModels;

public partial class PatternNodeManagerViewModel : NodeManagerViewModel<PatternNodeViewModel, PatternNode, PatternNode>
{
    readonly IRepository<Pattern> _patternRepository;
    readonly IScreenService _screenService;
    [ObservableProperty]
    Pattern _selectedPattern;
    static PatternNodeManagerViewModel()
    {
    }

    public PatternNodeManagerViewModel(
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

    protected override void Init(Action callback = null)
    {
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        var macroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;

        if (!macroSet.UsePatternsSnapshot)
        {
            base.Init();
        }
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }

    public void ForceInit()
    {
        base.Init(() =>
        {
            if (this.Root.Nodes.Count != 0) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var macroSetManager = ServiceHelper.GetService<MacroManagerViewModel>();
                await macroSetManager.UpdateMacroSetPatterns(macroSetManager.SelectedMacroSet);
            });
        });
    }

    private string GetPatternsSnapshotFile()
    {
        var macroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;
        var folder = FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(folder, $"{macroSet.Name}_patterns.json");
        return targetFile;
    }

    public void TakeSnapshot()
    {
        TakeSnapshot(base.ToJson());
    }

    public void TakeSnapshot(string json)
    {
        var targetFile = GetPatternsSnapshotFile();
        var macroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;
        File.WriteAllText(targetFile, json);
        _toastService.Show($"Exported {macroSet.Name} patterns on {targetFile}");
    }

    public override string ToJson()
    {
        var macroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;
        if (macroSet.UsePatternsSnapshot)
        {
            return File.ReadAllText(GetPatternsSnapshotFile());
        }

        return base.ToJson();
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

    public static Pattern ClonePattern(Pattern pattern)
    {
        var json = JsonSerializer.Serialize(pattern);
        return JsonSerializer.Deserialize<Pattern>(json);
    }

    public static Pattern GetScaled(IScreenService screenService, Pattern pattern, double scale)
    {
        var scaledPattern = ClonePattern(pattern);
        if (scaledPattern.ImageData is not null)
        {
            scaledPattern.ImageData = screenService.ScaleImageData(scaledPattern.ImageData, scale);
        }

        if (scaledPattern.ColorThreshold.ImageData is not null)
        {
            scaledPattern.ColorThreshold.ImageData = screenService.ScaleImageData(scaledPattern.ColorThreshold.ImageData, scale);
        }

        return scaledPattern;
    }

    [RelayCommand]
    private async Task AddPattern(PatternNode patternNode)
    {
        if (patternNode == null) return;

        var name = await _inputService.PromptInput("Please enter pattern name: ");
        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show("Canceled add pattern");
            return;
        }

        var newPattern = new PatternViewModel() { Name = name, PatternNodeId = patternNode.NodeId, ColorThreshold = new ColorThresholdPropertiesViewModel(), TextMatch = new TextMatchPropertiesViewModel() };
        patternNode.Patterns.Add(newPattern);
        _patternRepository.Insert(newPattern);
        _patternRepository.Save();
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
    private void DeletePattern(object[] values)
    {
        if (values.Length == 2 && values[0] is Pattern pattern && values[1] is PatternNode patternNode)
        {
            //_patternRepository.AttachEntities(pattern);     // When called form SettingPattern, pattern is not attached to the repository
            patternNode.Patterns.Remove(pattern);
            _patternRepository.Delete(pattern.PatternId);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private async Task CapturePattern(object[] values)
    {
        if (values.Length > 1 && values[1] is PatternNode patternNode)
        {
            if (values[0] is not Pattern pattern)
            {
                pattern = new PatternViewModel() { Name = "pattern", PatternNodeId = patternNode.NodeId, ColorThreshold = new ColorThresholdPropertiesViewModel(), TextMatch = new TextMatchPropertiesViewModel() };
                patternNode.Patterns.Add(pattern);
                _patternRepository.Insert(pattern);
                _patternRepository.Save();
            }

            var rect = await _inputService.DrawUserRectangle();
            pattern.ImageData = _screenService.GetCurrentImageData(rect);

            var topLeft = DisplayHelper.TopLeft;
            if (!topLeft.IsEmpty)   // If top left has value, then assuming it's a capture from physical device
            {
                rect = rect.Offset(-topLeft.X, -topLeft.Y);
                pattern.OffsetCalcType = OffsetCalcType.DockLeft;
            }

            pattern.RawBounds = rect;
            pattern.Resolution = new Size(DisplayHelper.DisplayInfo.Width, DisplayHelper.DisplayInfo.Height);
            pattern.ColorThreshold.IsActive = false;

            _patternRepository.Update(pattern, p => p.ColorThreshold);
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
    private void SavePattern(object[] values)
    {
        if (values.Length > 1 && values[0] is Pattern pattern && values[1] is PatternNode patternNode)
        {
            _patternRepository.Update(pattern, p => p.ColorThreshold);
            _patternRepository.Save();
            _nodeService.Update(patternNode);
            _nodeService.Save();
            _toastService.Show($"PatternNode saved: {patternNode.Name}");
        }
    }

    [RelayCommand]
    private async Task SetPatternBounds(Pattern pattern)
    {
        if (pattern == null) return;

        var rect = await _inputService.DrawUserRectangle();
        if (rect.IsEmpty) return;

        var topLeft = DisplayHelper.TopLeft;
        if (!topLeft.IsEmpty)   // If top left has value, then assuming it's a capture from physical device
        {
            rect = rect.Offset(-topLeft.X, -topLeft.Y);
        }

        pattern.RawBounds = rect;
        _patternRepository.Update(pattern);
        _patternRepository.Save();
    }

    [RelayCommand]
    private void TestPattern(object[] values)
    {
        if (values.Length != 5) return;

        if (values[0] is Pattern pattern &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset &&
            values[3] is bool doTestCalc &&
            values[4] is string inputScale)
        {
            if (double.TryParse(inputScale, out double scale) && scale != 1.0)
            {
                pattern = GetScaled(_screenService, pattern, scale);
            }

            // Usage of Task.Run and MainThread.BeginInvokeOnMainThread is needed because of AndroidOcrService
            Task.Run(() =>
            {
                var opts = new FindOptions() { Limit = 10 };
                if (doTestCalc) opts.Offset = pattern.Offset;
                if (int.TryParse(strXOffset, out int xOffset)) opts.Offset = opts.Offset.Offset(xOffset, 0);
                if (int.TryParse(strYOffset, out int yOffset)) opts.Offset = opts.Offset.Offset(0, yOffset);

                _screenService.DrawClear();
                var result = _screenService.FindPattern(pattern, opts);
                var points = result.Points;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _toastService.Show(points != null && points.Length > 0 ? "Match(es) found" : "No match found");

                    if (pattern.RawBounds != Rect.Zero)
                    {
                        _screenService.DrawRectangle(pattern.Bounds.Offset(opts.Offset));
                    }

                    if (points != null)
                    {
                        foreach (var point in points)
                        {
                            _screenService.DrawCircle(point);
                        }
                    }
                });
            });
        }
    }

    // For this to work, Android view needs to not be touchable or do a double click
    [RelayCommand]
    private async Task ClickPattern(object[] values)
    {
        if (values.Length != 4) return;

        if (values[0] is Pattern pattern &&
            pattern != null &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset &&
            values[3] is bool doTestCalc)
        {
            var opts = new FindOptions() { Limit = 10 };
            if (int.TryParse(strXOffset, out int xOffset)) opts.Offset = opts.Offset.Offset(xOffset, 0);
            if (int.TryParse(strYOffset, out int yOffset)) opts.Offset = opts.Offset.Offset(0, yOffset);
            if (doTestCalc) opts.Offset = pattern.Offset;

            _screenService.ClickPattern(pattern, opts);     //one to change focus
            await Task.Delay(300);
            _screenService.ClickPattern(pattern, opts);     //one to click
        }
    }

    [RelayCommand]
    private void ApplyColorThreshold(object[] values)
    {
        if (values.Length != 4) return;

        if (values[0] is Pattern pattern &&
            values[1] is int colorThresholdVariancePct &&
            values[2] is string colorThresholdColor &&
            values[3] is ICommand selectCommand)
        {
            pattern.ColorThreshold.VariancePct = colorThresholdVariancePct;
            pattern.ColorThreshold.Color = colorThresholdColor;
            pattern.ColorThreshold.IsActive = true;
            pattern.ColorThreshold.ImageData = _screenService.CalcColorThreshold(pattern, pattern.ColorThreshold);
            _patternRepository.Update(pattern, p => p.ColorThreshold);
            _patternRepository.Save();
            selectCommand.Execute(null);
            selectCommand.Execute(pattern);
        }
    }

    [RelayCommand]
    private async Task TestPatternTextMatch(object[] values)
    {
        if (values.Length != 4) return;

        if (values[0] is Pattern pattern &&
            pattern != null &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset &&
            values[3] is bool doTestCalc)
        {
            var opts = new TextFindOptions();
            if (int.TryParse(strXOffset, out int xOffset)) opts.Offset = opts.Offset.Offset(xOffset, 0);
            if (int.TryParse(strYOffset, out int yOffset)) opts.Offset = opts.Offset.Offset(0, yOffset);
            if (doTestCalc) opts.Offset = pattern.Offset;

            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Bounds.Offset(opts.Offset));
            var result = await _screenService.FindTextAsync(pattern, opts);
            _toastService.Show($"TextMatch: {result}");
            Console.WriteLine($"TextMatch: {result}");
        }
    }

    [RelayCommand]
    private async Task ApplyPatternTextMatch(object[] values)
    {
        if (values.Length != 4) return;

        if (values[0] is Pattern pattern &&
            pattern != null &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset &&
            values[3] is bool doTestCalc)
        {
            var opts = new TextFindOptions();
            if (int.TryParse(strXOffset, out int xOffset)) opts.Offset = opts.Offset.Offset(xOffset, 0);
            if (int.TryParse(strYOffset, out int yOffset)) opts.Offset = opts.Offset.Offset(0, yOffset);
            if (doTestCalc) opts.Offset = pattern.Offset;

            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Bounds.Offset(opts.Offset));
            var result = await _screenService.FindTextAsync(pattern, opts);
            _toastService.Show($"TextMatch Apply: {result}");
            pattern.TextMatch.Text = result;

            _patternRepository.Update(pattern, p => p.TextMatch);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private void ApplyPatternOffset(object[] values)
    {
        if (values.Length != 3) return;

        if (values[0] is Pattern pattern &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset)
        {
            var offset = Point.Zero;
            if (int.TryParse(strXOffset, out int xOffset)) offset = offset.Offset(xOffset, 0);
            if (int.TryParse(strYOffset, out int yOffset)) offset = offset.Offset(0, yOffset);

            if (offset.IsEmpty) return;

            pattern.RawBounds = pattern.RawBounds.Offset(offset);
            _patternRepository.Update(pattern);
            _patternRepository.Save();
            _toastService.Show($"Offset applied to pattern");
        }
    }

    [RelayCommand]
    private void TestSwipe(Pattern pattern)
    {
        if (pattern == null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _screenService.DrawClear();
            _screenService.DoClick(pattern.Bounds.Center);  //one to change focus
            await Task.Delay(300);
            _screenService.SwipePattern(pattern);                //one to swipe
            await Task.Delay(300);
            _screenService.DrawRectangle(pattern.Bounds.Offset(pattern.Offset));
        });
    }

    [RelayCommand]
    private void NormalizePattern(Pattern pattern)
    {
        if (pattern == null) return;

        var targetResolution = new Size(1920, 1080);

        // If already at target resolution, nothing to do
        if (pattern.Resolution == targetResolution)
        {
            _toastService.Show("Pattern is already normalized to 1920x1080");
            return;
        }

        var currentResolution = pattern.Resolution;
        var widthDiff = currentResolution.Width - targetResolution.Width;
        var heightDiff = currentResolution.Height - targetResolution.Height;

        Rect newRawBounds;

        switch (pattern.OffsetCalcType)
        {
            case OffsetCalcType.DockLeft:
                // DockLeft: RawBounds stay the same (anchored to top-left)
                newRawBounds = pattern.RawBounds;
                break;

            case OffsetCalcType.DockRight:
                // DockRight: Move RawBounds to maintain the same space on the right
                // If current resolution is wider than target, move left (subtract widthDiff)
                // If current resolution is narrower than target, move right (add widthDiff)
                newRawBounds = new Rect(
                    pattern.RawBounds.X - widthDiff,
                    pattern.RawBounds.Y,
                    pattern.RawBounds.Width,
                    pattern.RawBounds.Height
                );
                break;

            case OffsetCalcType.Center:
            case OffsetCalcType.Default:
                // Center: Offset by half of the width difference
                // Similar to PatternNode.cs line 126: xOffset = deltaX / 2
                // When converting FROM current TO target, we need to adjust RawBounds.X by half the difference
                newRawBounds = new Rect(
                    pattern.RawBounds.X - (widthDiff / 2.0),
                    pattern.RawBounds.Y,
                    pattern.RawBounds.Width,
                    pattern.RawBounds.Height
                );
                break;

            case OffsetCalcType.None:
            default:
                // For None or other types, keep bounds the same
                newRawBounds = pattern.RawBounds;
                break;
        }

        // Update the pattern
        pattern.RawBounds = newRawBounds;
        pattern.Resolution = targetResolution;

        _patternRepository.Update(pattern);
        _patternRepository.Save();

        _toastService.Show($"Pattern normalized to 1920x1080 (OffsetCalcType: {pattern.OffsetCalcType})");
    }
}