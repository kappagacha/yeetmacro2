﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using YeetMacro2.Data.Serialization;
using System.Text.Json.Nodes;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class DailyNodeManagerViewModel(
    int rootNodeId,
    IRepository<TodoNode> todoRepository,
    INodeService<TodoNode, TodoNode> nodeService,
    IInputService inputService,
    IToastService toastService) : TodoNodeManagerViewModel(rootNodeId, todoRepository, nodeService, inputService, toastService)
{
    protected override void CustomInit()
    {
        // This invokes ScriptNodeManagerViewModel SelectedNode PropertyChangedMessage
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<string>(this, nameof(CustomInit), null, null), nameof(DailyNodeManagerViewModel));
    }

    protected override Task AddNode()
    {
        var newNode = new TodoViewModel()
        {
            Date = ResolveTargetDate(0),
            Data = Root.Data
        };
        base.AddNode(newNode);
        return Task.CompletedTask;
    }

    public void UpdateCurrentDailyTemplate()
    {
        var targetDate = ResolveTargetDate(0);
        var todo = ResolveTodo(targetDate);

        var currentJson = JsonObject.Parse(todo.Data);
        var currentTemplate = JsonObject.Parse(Root.Data);
        var newJsonString = currentJson.Merge(currentTemplate).ToString();
        SaveTodo([todo, newJsonString]);
    }

    public TodoJsonParentViewModel GetCurrentDaily(int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var todo = ResolveTodo(targetDate);

        return todo.JsonViewModel;
    }

    public override DateOnly ResolveTargetDate(int offset)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow).AddDays(offset);
        var currentMacroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;
        if (utcNow.Hour < currentMacroSet.DailyResetUtcHour)
        {
            targetDate = targetDate.AddDays(-1);
        }
        return targetDate;
    }
}