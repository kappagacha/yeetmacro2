using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class TagManagerViewModel : ObservableObject
{
    private readonly int _macroSetId;
    private readonly INodeTagService _nodeTagService;
    private readonly IInputService _inputService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private NodeTag _selectedTag;

    public MacroSetViewModel MacroSet { get; }

    public TagManagerViewModel(
        int macroSetId,
        MacroSetViewModel macroSet,
        INodeTagService nodeTagService,
        IInputService inputService,
        IToastService toastService)
    {
        _macroSetId = macroSetId;
        MacroSet = macroSet;
        _nodeTagService = nodeTagService;
        _inputService = inputService;
        _toastService = toastService;
    }

    public void ImportTags(IEnumerable<NodeTag> tagsToImport)
    {
        // Delete existing tags from database
        var existingTags = _nodeTagService.GetTagsForMacroSet(_macroSetId).ToList();
        foreach (var tag in existingTags)
        {
            _nodeTagService.Delete(tag.TagId);
        }

        // Ensure Tags is an ObservableCollection
        ObservableCollection<NodeTag> tags;
        if (MacroSet.Tags == null || MacroSet.Tags is not ObservableCollection<NodeTag>)
        {
            tags = new ObservableCollection<NodeTag>();
            MacroSet.Tags = tags;
        }
        else
        {
            tags = MacroSet.Tags as ObservableCollection<NodeTag>;
        }

        // Clear and repopulate the collection
        tags.Clear();

        foreach (var tag in tagsToImport.OrderBy(t => t.Position))
        {
            var newTag = new NodeTag
            {
                MacroSetId = _macroSetId,
                Name = tag.Name,
                FontFamily = tag.FontFamily,
                Glyph = tag.Glyph,
                Position = tag.Position
            };
            _nodeTagService.Insert(newTag);
            tags.Add(newTag);
        }
        _nodeTagService.Save();
    }

    [RelayCommand]
    private async Task AddTag()
    {
        // Get tag name
        var name = await _inputService.PromptInput("Enter tag name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        // Show icon picker
        var iconPicker = new IconPickerViewModel();
        var iconPickerPage = new Pages.IconPickerPage(iconPicker);

        // Start the ShowAsync task before pushing the modal
        var showTask = iconPicker.ShowAsync();
        await Shell.Current.Navigation.PushModalAsync(iconPickerPage);

        // Wait for the user to close the modal
        var confirmed = await showTask;

        // Check if user confirmed selection
        if (!confirmed || string.IsNullOrEmpty(iconPicker.SelectedGlyph))
        {
            return;
        }

        var tags = MacroSet.Tags as ObservableCollection<NodeTag>;
        var newTag = new NodeTag
        {
            MacroSetId = _macroSetId,
            Name = name,
            FontFamily = iconPicker.SelectedFontFamilyResult,
            Glyph = iconPicker.SelectedGlyph,
            Position = tags.Count
        };

        _nodeTagService.Insert(newTag);
        _nodeTagService.Save();
        tags.Add(newTag);
        _toastService.Show($"Added tag: {name}");
    }

    [RelayCommand]
    private async Task DeleteTag(NodeTag tag)
    {
        if (tag == null) return;

        var confirm = await _inputService.SelectOption(
            $"Delete tag {tag.Name}?",
            new[] { "Yes", "No" });

        if (confirm != "Yes") return;

        _nodeTagService.Delete(tag.TagId);
        _nodeTagService.Save();

        var tags = MacroSet.Tags as ObservableCollection<NodeTag>;
        tags.Remove(tag);
        _toastService.Show($"Deleted tag: {tag.Name}");
    }

    [RelayCommand]
    private void MoveTagUp(NodeTag tag)
    {
        if (tag == null || tag.Position == 0) return;

        var tags = MacroSet.Tags as ObservableCollection<NodeTag>;
        var index = tags.IndexOf(tag);
        if (index <= 0) return;

        var tagAbove = tags[index - 1];
        tagAbove.Position++;
        tag.Position--;

        _nodeTagService.Update(tagAbove);
        _nodeTagService.Update(tag);
        _nodeTagService.Save();

        tags.Move(index, index - 1);
        _toastService.Show("Moved tag up");
    }

    [RelayCommand]
    private void MoveTagDown(NodeTag tag)
    {
        if (tag == null) return;

        var tags = MacroSet.Tags as ObservableCollection<NodeTag>;
        var index = tags.IndexOf(tag);
        if (index < 0 || index >= tags.Count - 1) return;

        var tagBelow = tags[index + 1];
        tagBelow.Position--;
        tag.Position++;

        _nodeTagService.Update(tagBelow);
        _nodeTagService.Update(tag);
        _nodeTagService.Save();

        tags.Move(index, index + 1);
        _toastService.Show("Moved tag down");
    }
}
