using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YeetMacro2.ViewModels.NodeViewModels;

public partial class ScriptNodeManagerViewModel : NodeManagerViewModel<ScriptNodeViewModel, ScriptNode, ScriptNode>
{
    [ObservableProperty]
    bool _showEditor;

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
            WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<ScriptNode>(this, nameof(SelectedNode), null, SelectedNode), nameof(ScriptNodeManagerViewModel));
        }
    }

    [RelayCommand]
    public void ToggleShowEditor()
    {
        ShowEditor = !ShowEditor;
    }
}
