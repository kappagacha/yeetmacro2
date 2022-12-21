using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class PatternTreeViewViewModel : TreeViewViewModel<PatternNode, PatternNode>
{
    IRepository<PatternBase> _patternRepository;
    IMediaProjectionService _projectionService;
    IMacroService _macroService;
    Resolution _currentResolution;

    //public ICommand InitPatternsCommand { get; }
    public ICommand AddPatternCommand { get; }
    public ICommand SelectPatternCommand { get; set; }
    public ICommand DeletePatternCommand { get; set; }
    public ICommand CapturePatternCommand { get; set; }
    public ICommand SetPatternBoundsCommand { get; set; }
    public ICommand TestPatternCommand { get; set; }
    public ICommand TestBoundsCommand { get; set; }
    public ICommand ClickPatternCommand { get; set; }
    PatternBase _selectedPattern;
    ImageSource _selectedImageSource;
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
    public ImageSource SelectedImageSource
    {
        get { return _selectedImageSource; }
        set { SetProperty(ref _selectedImageSource, value); }
    }
    public Resolution CurrentResolution => _currentResolution ?? (_currentResolution = new Resolution()
    {
        Width = DeviceDisplay.MainDisplayInfo.Width,
        Height = DeviceDisplay.MainDisplayInfo.Height
    });

    public PatternTreeViewViewModel() : base()
    {
    }

    public PatternTreeViewViewModel(
        INodeService<PatternNode, PatternNode> nodeService,
        IWindowManagerService windowManagerService,
        IToastService toastService,
        IRepository<PatternBase> patternRepository,
        IMediaProjectionService projectionService,
        IMacroService macroService)
            : base(nodeService, windowManagerService, toastService)
    {
        // InitPatternsCommand = new Command(InitPatterns);
        AddPatternCommand = new Command(AddPattern);
        SelectPatternCommand = new Command<PatternBase>(SelectPattern);
        DeletePatternCommand = new Command<PatternBase>(DeletePattern);
        CapturePatternCommand = new Command<PatternBase>(CapturePattern);
        TestPatternCommand = new Command(TestPattern);
        ClickPatternCommand = new Command(ClickPattern);
        SetPatternBoundsCommand = new Command(SetPatternBounds);
        _windowManagerService = windowManagerService;
        _patternRepository = patternRepository;
        _projectionService = projectionService;
        _macroService = macroService;

        PropertyChanged += PatternTreeViewViewModel_PropertyChanged;

        //InitPatternsCommand.Execute(null);
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
        newNode.Nodes = ProxyViewModel.CreateCollection(new ObservableCollection<PatternNode>());
        newNode.Patterns = ProxyViewModel.CreateCollection(new ObservableCollection<Pattern>());
        newNode.UserPatterns = ProxyViewModel.CreateCollection(new ObservableCollection<UserPattern>());
    }

    [RelayCommand]
    public void InitPatterns(object o)
    {
        Task.Run(() =>
        {
            Root = ProxyViewModel.Create(_nodeService.GetRoot());
            _patternRepository.DetachAllEntities();
            Root.Nodes = ProxyViewModel.CreateCollection(Root.Nodes, pn => new { pn.Nodes, pn.Patterns, pn.UserPatterns });
            _nodeService.ReAttachNodes(Root);
            if (SelectedNode == null && Root.Nodes.Count > 0)
            {
                SelectedNode = Root.Nodes.First();

                if (SelectedPattern == null && SelectedNode.Patterns.Count > 0)
                {
                    var targetPattern = SelectedNode.Patterns.First();
                    targetPattern.IsSelected = false;
                    SelectPattern(targetPattern);
                }
            }
        });
    }

    private async void AddPattern(object o)
    {
        if (SelectedNode != null)
        {
            var name = await _windowManagerService.PromptInput("Please enter pattern name: ");
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

    private void DeletePattern(PatternBase pattern)
    {
        SelectedNode.Patterns.Remove((Pattern)pattern);
        _patternRepository.Delete(pattern);
        _patternRepository.Save();
    }

    private async void CapturePattern(PatternBase pattern)
    {
        //if (pattern == null)
        //{
        //    pattern = ResolveSelectedPattern();
        //}

        //if (pattern == null)
        //{
        //    return;
        //}

        //var bounds = await _windowManagerService.DrawUserRectangle();
        //var strokeThickness = 3;
        //var imageStream = await _projectionService.GetCurrentImageStream(
        //    (int)bounds.X + strokeThickness - 1,
        //    (int)bounds.Y + strokeThickness - 1,
        //    (int)bounds.Width - strokeThickness + 1,
        //    (int)bounds.Height - strokeThickness - 1);

        //pattern.ImageData = imageStream.ToArray();
        //pattern.Bounds = ProxyViewModel.Create(new Bounds()
        //{
        //    X = bounds.X,
        //    Y = bounds.Y,
        //    Width = bounds.Width,
        //    Height = bounds.Height
        //});
        //pattern.Resolution = ProxyViewModel.Create(new Resolution()
        //{
        //    Width = DeviceDisplay.MainDisplayInfo.Width,
        //    Height = DeviceDisplay.MainDisplayInfo.Height
        //});
        //_patternRepository.Update(pattern);
        //_patternRepository.Save();

        //SelectedImageSource = ImageSource.FromStream(() => imageStream);



        ////await Task.Delay(250);
        ////pattern.IsSelected = false;
        ////SelectPattern(pattern);
    }

    private async void SetPatternBounds(object obj)
    {
        //ResolveSelectedPattern();
        //if (_selectedPattern == null) return;

        //var bounds = await _windowManagerService.DrawUserRectangle();
        //if (bounds != null)
        //{
        //    _selectedPattern.Bounds = bounds;
        //    _patternRepository.Update(_selectedPattern);
        //    _patternRepository.Save();
        //}
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

    private async void TestPattern(object o)
    {
        //_windowManagerService.DrawClear();
        //ResolveSelectedPattern();
        //if (_selectedPattern == null) return;
        //var result = await _macroService.FindPattern(_selectedPattern);
        //var points = result.Points;
        //_toastService.MakeText(points != null && points.Length > 0 ? "Match(es) found" : "No match found");

        //if (_selectedPattern.Bounds != null)
        //{
        //    var calcBounds = _windowManagerService.TransformBounds(_selectedPattern.Bounds, _selectedPattern.Resolution);
        //    _windowManagerService.DrawRectangle((int)calcBounds.X, (int)calcBounds.Y, (int)calcBounds.Width, (int)calcBounds.Height);
        //}

        //if (points != null)
        //{
        //    foreach (var point in points)
        //    {
        //        _windowManagerService.DrawCircle((int)point.X, (int)point.Y);
        //    }
        //}
    }

    // For this to work, Android view needs to not be touchable or do a double click
    private async void ClickPattern(object o)
    {
        ResolveSelectedPattern();
        if (_selectedPattern == null) return;
        await _macroService.ClickPattern(_selectedPattern);     //one to change focus
        await Task.Delay(300);
        await _macroService.ClickPattern(_selectedPattern);     //one to click
    }
}