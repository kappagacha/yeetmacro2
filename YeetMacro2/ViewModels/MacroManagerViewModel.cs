using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;

namespace YeetMacro2.ViewModels;
public partial class MacroManagerViewModel : ObservableObject
{
    IRepository<MacroSet> _macroSetRepository;
    IRepository<PatternBase> _patternRepository;
    INodeService<PatternNode, PatternNode> _nodeService;
    PatternTreeViewViewModelFactory _patternTreeViewFacctory;
    [ObservableProperty]
    ICollection<MacroSet> _macroSets;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PatternTree))]
    MacroSet _selectedMacroSet;
    ConcurrentDictionary<int, PatternTreeViewViewModel> _nodeRootIdToPatternTree;

    public PatternTreeViewViewModel PatternTree
    {
        get
        {
            // Lazy load PattenTree
            if (_selectedMacroSet == null) return null;
            if (!_nodeRootIdToPatternTree.ContainsKey(_selectedMacroSet.RootPatternNodeId))
            {
                var patternTree = _patternTreeViewFacctory.Create(_selectedMacroSet.RootPatternNodeId);
                _nodeRootIdToPatternTree.TryAdd(_selectedMacroSet.RootPatternNodeId, patternTree);
            }
            return _nodeRootIdToPatternTree[_selectedMacroSet.RootPatternNodeId];
        }
    }

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository,
        PatternTreeViewViewModelFactory patternTreeViewFactory,
        INodeService<PatternNode, PatternNode> nodeService,
        IRepository<PatternBase> patternRepository)
    {
        _macroSetRepository = macroSetRepository;
        _patternTreeViewFacctory = patternTreeViewFactory;
        _nodeService = nodeService;
        _patternRepository = patternRepository;
        _macroSets = ProxyViewModel.CreateCollection(_macroSetRepository.Get());
        if (_macroSets.Count > 0 )
        {
            SelectedMacroSet = _macroSets.First();
        }

        _macroSetRepository.DetachAllEntities();
        _macroSetRepository.AttachEntities(_macroSets.ToArray());
        _nodeRootIdToPatternTree = new ConcurrentDictionary<int, PatternTreeViewViewModel>();
    }

    [RelayCommand]
    public async Task AddMacroSet()
    {
        var macroSetName = await Application.Current.MainPage.DisplayPromptAsync("Macro Set", "Enter name...");
        if (string.IsNullOrEmpty(macroSetName)) return;

        var macroSet = ProxyViewModel.Create(new MacroSet() { Name = macroSetName });
        var rootPattern = ProxyViewModel.Create(_nodeService.GetRoot(0));
        rootPattern.Children = ProxyViewModel.CreateCollection(new ObservableCollection<PatternNode>());
        rootPattern.Patterns = ProxyViewModel.CreateCollection(new ObservableCollection<Pattern>());
        rootPattern.UserPatterns = ProxyViewModel.CreateCollection(new ObservableCollection<UserPattern>());
        _nodeService.ReAttachNodes(rootPattern);
        macroSet.RootPattern = rootPattern;
        macroSet.RootPatternNodeId = rootPattern.NodeId;
        _macroSets.Add(macroSet);
        _macroSetRepository.Insert(macroSet);
        _macroSetRepository.Save();
        SelectedMacroSet = macroSet;
    }

    [RelayCommand]
    public async Task DeleteMacroSet(MacroSet macroSet)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Macro Set", "Are you sure?", "Ok", "Cancel")) return;

        _nodeService.Delete(macroSet.RootPattern);
        _macroSetRepository.Delete(macroSet);
        _macroSetRepository.Save();
        _macroSets.Remove(macroSet);
    }
}
