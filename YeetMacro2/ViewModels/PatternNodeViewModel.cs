using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class PatternNodeViewModel : NodeViewModel<PatternNode, PatternNode>
{
    IRepository<Pattern> _patternRepository;
    IScreenService _screenService;
    IMacroService _macroService;
    Resolution _currentResolution;
    [ObservableProperty]
    ImageSource _selectedImageSource;
    [ObservableProperty]
    Pattern _selectedPattern;

    //public PatternBase SelectedPattern
    //{
    //    get { return _selectedPattern; }
    //    set
    //    {
    //        SetProperty(ref _selectedPattern, value);
    //        if (_selectedPattern != null && _selectedPattern.ImageData != null)
    //        {
    //            SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(_selectedPattern.ImageData));
    //        }
    //    }
    //}
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
        IScreenService screenService,
        IMacroService macroService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _patternRepository = patternRepository;
        _screenService = screenService;
        _macroService = macroService;

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
    }

    [RelayCommand]
    private async void AddPattern(object o)
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
            SelectedNode.Patterns.Add(newPattern);
            _patternRepository.Insert(newPattern);
            _patternRepository.Save();
        }
    }

    [RelayCommand]
    private void SelectPattern(Pattern pattern)
    {
        pattern.IsSelected = !pattern.IsSelected;

        if (SelectedPattern != null && SelectedPattern != pattern)
        {
            SelectedPattern.IsSelected = false;
        }

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
    private void DeletePattern(Pattern pattern)
    {
        SelectedNode.Patterns.Remove((Pattern)pattern);
        _patternRepository.Delete(pattern);
        _patternRepository.Save();
    }

    [RelayCommand]
    private async void CapturePattern(Pattern pattern)
    {
        if (pattern == null)
        {
            pattern = ResolveSelectedPattern();
        }

        if (pattern == null)
        {
            return;
        }

        var bounds = await _inputService.DrawUserRectangle();
        var strokeThickness = 3;
        var imageStream = await _screenService.GetCurrentImageStream(
            (int)bounds.X + strokeThickness - 1,
            (int)bounds.Y + strokeThickness - 1,
            (int)bounds.W - strokeThickness + 1,
            (int)bounds.H - strokeThickness - 1);

        pattern.ImageData = imageStream.ToArray();

        //Console.WriteLine(pattern.ImageData);
        pattern.Bounds = ProxyViewModel.Create(new Bounds()
        {
            X = bounds.X,
            Y = bounds.Y,
            W = bounds.W,
            H = bounds.H
        });
        pattern.Resolution = ProxyViewModel.Create(new Resolution()
        {
            Width = DeviceDisplay.MainDisplayInfo.Width,
            Height = DeviceDisplay.MainDisplayInfo.Height
        });
        _patternRepository.Update(pattern);
        _patternRepository.Save();

        SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(_selectedPattern.ImageData));
        //SelectedImageSource = ImageSource.FromStream(() => imageStream);

        ////await Task.Delay(250);
        ////pattern.IsSelected = false;
        ////SelectPattern(pattern);
    }

    [RelayCommand]
    private void SavePattern(Pattern pattern)
    {
        if (pattern == null)
        {
            pattern = ResolveSelectedPattern();
        }

        if (pattern == null)
        {
            return;
        }

        _patternRepository.Update(pattern);
        _patternRepository.Save();
    }

    [RelayCommand]
    private async void SetPatternBounds(object obj)
    {
        ResolveSelectedPattern();
        if (_selectedPattern == null) return;

        var bounds = await _inputService.DrawUserRectangle();
        if (bounds != null)
        {
            _selectedPattern.Bounds = bounds;
            _patternRepository.Update(_selectedPattern);
            _patternRepository.Save();
        }
    }

    private Pattern ResolveSelectedPattern()
    {
        if (SelectedNode.IsMultiPattern && SelectedPattern == null)
        {
            _toastService.Show("No selected pattern.");
            return null;
        }
        else if (SelectedNode.IsMultiPattern)
        {
            _toastService.Show("Case 2");
            return SelectedPattern;
        }
        else if (!SelectedNode.IsMultiPattern && SelectedNode.Patterns.Count > 0)
        {
            _toastService.Show("Case 3");
            var targetPattern = SelectedNode.Patterns.First();
            targetPattern.IsSelected = false;
            SelectPattern(targetPattern);
            return SelectedPattern;
        }
        else if (!SelectedNode.IsMultiPattern)
        {
            _toastService.Show("Case 4");
            var newPattern = ProxyViewModel.Create(new Pattern() { Name = "pattern" });
            SelectedNode.Patterns.Add(newPattern);
            _patternRepository.Insert(newPattern);
            _patternRepository.Save();
            SelectPattern(newPattern);
            return newPattern;
        }
        else
        {
            _toastService.Show("Case 5");
            return null;
        }
    }

    [RelayCommand]
    private async void TestPattern(object o)
    {
        ResolveSelectedPattern();
        if (_selectedPattern == null) return;

        _screenService.DrawClear();
        var result = await _macroService.FindPattern(_selectedPattern, new FindOptions() { Limit = 10 });
        var points = result.Points;
        _toastService.Show(points != null && points.Length > 0 ? "Match(es) found" : "No match found");

        if (_selectedPattern.Bounds != null)
        {
            var calcBounds = _screenService.TransformBounds(_selectedPattern.Bounds, _selectedPattern.Resolution);
            _screenService.DrawRectangle((int)calcBounds.X, (int)calcBounds.Y, (int)calcBounds.W, (int)calcBounds.H);
        }

        if (points != null)
        {
            foreach (var point in points)
            {
                _screenService.DrawCircle((int)point.X, (int)point.Y);
            }
        }
    }

    // For this to work, Android view needs to not be touchable or do a double click
    [RelayCommand]
    private async void ClickPattern(object o)
    {
        ResolveSelectedPattern();
        if (_selectedPattern == null) return;
        await _macroService.ClickPattern(_selectedPattern);     //one to change focus
        await Task.Delay(300);
        await _macroService.ClickPattern(_selectedPattern);     //one to click
    }

    [RelayCommand]
    private void ApplyColorThreshold(string colorThrehold)
    {
        //if (string.IsNullOrWhiteSpace(color)) return;


    }
}