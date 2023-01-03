using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

// https://stackoverflow.com/questions/53884417/net-core-di-ways-of-passing-parameters-to-constructor
public class PatternTreeViewViewModelFactory
{
    IServiceProvider _serviceProvider;
    public PatternTreeViewViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider= serviceProvider;
    }

    public PatternTreeViewViewModel Create(int rootId)
    {
        return ActivatorUtilities.CreateInstance<PatternTreeViewViewModel>(_serviceProvider, rootId);
    }
}

public partial class PatternTreeViewViewModel : TreeViewViewModel<PatternNode, PatternNode>
{
    IRepository<PatternBase> _patternRepository;
    IScreenService _screenService;
    IMacroService _macroService;
    Resolution _currentResolution;
    PatternBase _selectedPattern;
    [ObservableProperty]
    ImageSource _selectedImageSource;
    [ObservableProperty]
    bool _isInitialized;
    TaskCompletionSource _initializeCompleted;

    public PatternBase SelectedPattern
    {
        get { return _selectedPattern; }
        set
        {
            SetProperty(ref _selectedPattern, value);
            if (_selectedPattern != null && _selectedPattern.ImageData != null)
            {
                SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(_selectedPattern.ImageData));
            }
        }
    }
    public Resolution CurrentResolution => _currentResolution ?? (_currentResolution = new Resolution()
    {
        Width = DeviceDisplay.MainDisplayInfo.Width,
        Height = DeviceDisplay.MainDisplayInfo.Height
    });

    public PatternTreeViewViewModel(
        int rootNodeId,
        INodeService<PatternNode, PatternNode> nodeService,
        IInputService inputService,
        IToastService toastService,
        IRepository<PatternBase> patternRepository,
        IScreenService screenService,
        IMacroService macroService)
            : base(nodeService, inputService, toastService)
    {
        _patternRepository = patternRepository;
        _screenService = screenService;
        _macroService = macroService;

        PropertyChanged += PatternTreeViewViewModel_PropertyChanged;

        _initializeCompleted = new TaskCompletionSource();
        InitPatterns(rootNodeId);
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

    protected override void OnBeforeAddNode(PatternNode newNode)
    {
        newNode.Children = ProxyViewModel.CreateCollection(new ObservableCollection<PatternNode>());
        newNode.Patterns = ProxyViewModel.CreateCollection(new ObservableCollection<Pattern>());
        newNode.UserPatterns = ProxyViewModel.CreateCollection(new ObservableCollection<UserPattern>());
    }

    private void InitPatterns(int rootNodeId)
    {
        Task.Run(() =>
        {
            var root = ProxyViewModel.Create(_nodeService.GetRoot(rootNodeId));
            //_patternRepository.DetachAllEntities();
            root.Children = ProxyViewModel.CreateCollection(root.Children, pn => new { pn.Children, pn.Patterns, pn.UserPatterns });
            _nodeService.ReAttachNodes(root);
            var firstChild = root.Children.FirstOrDefault();
            if (SelectedNode == null && firstChild != null)
            {
                root.IsExpanded = true;
                firstChild.IsSelected = false;
                SelectNode(firstChild);

                if (SelectedPattern == null && SelectedNode.Patterns.Count > 0)
                {
                    var targetPattern = SelectedNode.Patterns.First();
                    targetPattern.IsSelected = false;
                    SelectPattern(targetPattern);
                }
            }
            Root = root;
            IsInitialized = true;
            _initializeCompleted.SetResult();
        });
    }

    public async Task WaitForInitialization()
    {
        await _initializeCompleted.Task;
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
    private void SelectPattern(PatternBase pattern)
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
    private void DeletePattern(PatternBase pattern)
    {
        SelectedNode.Patterns.Remove((Pattern)pattern);
        _patternRepository.Delete(pattern);
        _patternRepository.Save();
    }

    [RelayCommand]
    private async void CapturePattern(PatternBase pattern)
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

    private PatternBase ResolveSelectedPattern()
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
        var result = await _macroService.FindPattern(_selectedPattern);
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
}