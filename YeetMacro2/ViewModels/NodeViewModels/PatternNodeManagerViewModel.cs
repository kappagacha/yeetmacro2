using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public partial class PatternNodeManagerViewModel : NodeManagerViewModel<PatternNodeViewModel, PatternNode, PatternNode>
{
    ILogger<PatternNodeManagerViewModel> _logger;
    IRepository<Pattern> _patternRepository;
    IScreenService _screenService;
    [ObservableProperty]
    Pattern _selectedPattern;
    static PatternNodeManagerViewModel()
    {
    }

    public PatternNodeManagerViewModel(
        int rootNodeId,
        ILogger<PatternNodeManagerViewModel> logger,
        INodeService<PatternNode, PatternNode> nodeService,
        IInputService inputService,
        IToastService toastService,
        IRepository<Pattern> patternRepository,
        IScreenService screenService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _logger = logger;
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

    protected override void CustomInit()
    {
        foreach (var patternNode in _nodeService.GetDescendants<PatternNode>(Root).ToList())
        {
            _patternRepository.AttachEntities(patternNode.Patterns.ToArray());
        }
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
        if (SelectedNode != null)
        {
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
            patternNode.Patterns.Remove(pattern);
            //_patternRepository.Delete(pattern);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private async Task CapturePattern(object[] values)
    {
        if (values.Length > 1 && values[1] is PatternNode patternNode)
        {
            var pattern = values[0] as Pattern;
            if (pattern == null)
            {
                pattern = new PatternViewModel() { Name = "pattern", PatternNodeId = patternNode.NodeId, ColorThreshold = new ColorThresholdPropertiesViewModel(), TextMatch = new TextMatchPropertiesViewModel() };
                patternNode.Patterns.Add(pattern);
                _patternRepository.Insert(pattern);
                _patternRepository.Save();
            }

            var rect = await _inputService.DrawUserRectangle();
            pattern.ImageData = _screenService.GetCurrentImageData(rect);

            pattern.Rect = rect;
            pattern.Resolution = new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

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
    private void SavePattern(object[] values)
    {
        if (values.Length > 1 && values[0] is Pattern pattern && values[1] is PatternNode patternNode)
        {
            _patternRepository.Update(pattern);
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
        if (rect != Rect.Zero)
        {
            pattern.Rect = rect;
            _patternRepository.Update(pattern);
            _patternRepository.Save();
        }
    }

    public static Point CalcOffset(Pattern pattern, Size currentResolution)
    {
        if (currentResolution == pattern.Resolution) return Point.Zero;

        var xOffset = 0;
        var yOffset = 0;
        switch (pattern.OffsetCalcType)
        {
            case OffsetCalcType.Default:
            case OffsetCalcType.Center:
                {   // horizontal center handling
                    var deltaX = currentResolution.Width - pattern.Resolution.Width;
                    xOffset = (int)(deltaX / 2);
                }
                break;
            case OffsetCalcType.DockRight:
                {   // horizontal dock right handling (dock left does not need handling)
                    var right = pattern.Resolution.Width - pattern.Rect.X;
                    var targetX = currentResolution.Width - right;
                    xOffset = (int)(targetX - pattern.Rect.X);
                }
                break;
            case OffsetCalcType.HorizontalStretchOffset:
                {
                    // HorizontalStretchMultiplier = targetXOffset / deltaX
                    var deltaX = currentResolution.Width - pattern.Resolution.Width;
                    xOffset = (int)(deltaX * pattern.HorizontalStretchMultiplier);
                }
                break;
            case OffsetCalcType.VerticalStretchOffset:
                {
                    // HorizontalStretchMultiplier = targetYOffset / deltaY
                    var deltaY = currentResolution.Height - pattern.Resolution.Height;
                    yOffset = (int)(deltaY * pattern.VerticalStretchMultiplier);
                }
                break;
        }
        return new Point(xOffset, yOffset);
    }

    [RelayCommand]
    private void TestPattern(object[] values)
    {
        if (values.Length != 5) return;

        if (values[0] is Pattern pattern &&
            pattern != null &&
            values[1] is string strXOffset &&
            values[2] is string strYOffset &&
            values[3] is bool doTestCalc &&
            values[4] is string inputScale)
        {
            if (double.TryParse(inputScale, out double scale) && scale != 1.0)
            {
                pattern = GetScaled(_screenService, pattern, scale);
            }

            var opts = new FindOptions() { Limit = 10 };
            if (int.TryParse(strXOffset, out int xOffset)) opts.Offset = opts.Offset.Offset(xOffset, 0);
            if (int.TryParse(strYOffset, out int yOffset)) opts.Offset = opts.Offset.Offset(0, yOffset);
            if (doTestCalc) opts.Offset = CalcOffset(pattern, _screenService.CalcResolution);

            _screenService.DrawClear();
            var result = _screenService.FindPattern(pattern, opts);
            var points = result.Points;
            _toastService.Show(points != null && points.Length > 0 ? "Match(es) found" : "No match found");

            if (pattern.Rect != Rect.Zero)
            {
                _screenService.DrawRectangle(pattern.Rect.Offset(opts.Offset));
            }

            if (points != null)
            {
                foreach (var point in points)
                {
                    _screenService.DrawCircle(point);
                }
            }
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
            if (doTestCalc) opts.Offset = CalcOffset(pattern, _screenService.CalcResolution);

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
    private void TestPatternTextMatch(object[] values)
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
            if (doTestCalc) opts.Offset = CalcOffset(pattern, _screenService.CalcResolution);

            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Rect.Offset(opts.Offset));
            var result = _screenService.GetText(pattern, opts);
            _toastService.Show($"TextMatch: {result}");
            Console.WriteLine($"TextMatch: {result}");
        }
    }

    [RelayCommand]
    private void ApplyPatternTextMatch(object[] values)
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
            if (doTestCalc) opts.Offset = CalcOffset(pattern, _screenService.CalcResolution);

            _screenService.DrawClear();
            _screenService.DrawRectangle(pattern.Rect.Offset(opts.Offset));
            var result = _screenService.GetText(pattern, opts);
            _toastService.Show($"TextMatch Apply: {result}");
            pattern.TextMatch.Text = result;

            _patternRepository.Update(pattern);
            _patternRepository.Save();
        }
    }
}