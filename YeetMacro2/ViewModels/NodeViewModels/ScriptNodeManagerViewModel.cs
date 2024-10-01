using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YeetMacro2.ViewModels.NodeViewModels;

public partial class ScriptNodeManagerViewModel : NodeManagerViewModel<ScriptNodeViewModel, ScriptNode, ScriptNode>
{
    [ObservableProperty]
    bool _showScriptEditor, _showHiddenScripts;

    public ScriptNodeManagerViewModel(
        int rootNodeId,
        INodeService<ScriptNode, ScriptNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        IsList = true;
        PropertyChanged += ScriptNodeManagerViewModel_PropertyChanged;

        ShowHiddenScripts = Preferences.Default.Get(nameof(ShowHiddenScripts), false);
    }

    private void ScriptNodeManagerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedNode))
        {
            WeakReferenceMessenger.Default.Send(new Lazy<ScriptNode>(SelectedNode));
        }
    }

    partial void OnShowHiddenScriptsChanged(bool value)
    {
        Preferences.Default.Set(nameof(ShowHiddenScripts), ShowHiddenScripts);
    }

    public static void MergeSettings(ScriptNode source, ScriptNode dest)
    {
        dest.IsHidden = source.IsHidden;
        dest.IsFavorite = source.IsFavorite;

        foreach (var childSource in source.Nodes)
        {
            // Not supporting duplicate names
            var childDest = dest.Nodes.FirstOrDefault(sn => sn.Name == childSource.Name);
            if (childDest is not null)
            {
                MergeSettings(childSource, childDest);
            }
        }
    }
}
