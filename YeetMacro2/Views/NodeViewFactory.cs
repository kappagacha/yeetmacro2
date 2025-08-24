using Microsoft.Maui.Controls;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Views;

public static class NodeViewFactory
{
    public static ContentView CreatePatternNodeView()
    {
        return new GenericNodeView<PatternNode, PatternNodeManagerViewModel>();
    }

    public static ContentView CreateSettingNodeView()
    {
        return new GenericNodeView<ParentSetting, SettingNodeManagerViewModel>();
    }

    public static ContentView CreateTodoNodeView()
    {
        return new GenericNodeView<TodoNode, TodoNodeManagerViewModel>();
    }

    public static ContentView CreateScriptNodeView()
    {
        return new GenericNodeView<ScriptNode, ScriptNodeManagerViewModel>();
    }

    public static ContentView CreateForNodeManager(NodeManagerViewModel nodeManager)
    {
        return nodeManager switch
        {
            PatternNodeManagerViewModel => CreatePatternNodeView(),
            SettingNodeManagerViewModel => CreateSettingNodeView(),
            TodoNodeManagerViewModel => CreateTodoNodeView(),
            ScriptNodeManagerViewModel => CreateScriptNodeView(),
            _ => throw new NotSupportedException($"NodeManager type {nodeManager.GetType()} is not supported")
        };
    }
}

// Non-generic wrapper for XAML usage
public class PatternCoreNodeView : GenericNodeView<PatternNode, PatternNodeManagerViewModel>
{
    public PatternCoreNodeView() : base()
    {
    }
}

// This can replace the existing SettingNodeView if desired
public class SettingCoreNodeView : GenericNodeView<ParentSetting, SettingNodeManagerViewModel>
{
    public SettingCoreNodeView() : base()
    {
    }
}

public class TodoCoreNodeView : GenericNodeView<TodoNode, TodoNodeManagerViewModel>
{
    public TodoCoreNodeView() : base()
    {
    }
}

public class ScriptCoreNodeView : GenericNodeView<ScriptNode, ScriptNodeManagerViewModel>
{
    public ScriptCoreNodeView() : base()
    {
    }
}