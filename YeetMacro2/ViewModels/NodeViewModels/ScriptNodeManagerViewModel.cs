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
    bool _showScriptEditor;

    public ScriptNodeManagerViewModel(
        int rootNodeId,
        INodeService<ScriptNode, ScriptNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        IsList = true;
        PropertyChanged += ScriptNodeManagerViewModel_PropertyChanged;
    }

    private void ScriptNodeManagerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedNode))
        {
            WeakReferenceMessenger.Default.Send(new Lazy<ScriptNode>(SelectedNode));
        }
    }
}
