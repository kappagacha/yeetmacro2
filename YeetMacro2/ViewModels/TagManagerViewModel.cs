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
    private readonly ICollection<NodeTag> _macroSetTags;

    [ObservableProperty]
    private NodeTag _selectedTag;

    public ObservableCollection<NodeTag> Tags { get; }

    public TagManagerViewModel(
        int macroSetId,
        ICollection<NodeTag> macroSetTags,
        INodeTagService nodeTagService,
        IInputService inputService,
        IToastService toastService)
    {
        _macroSetId = macroSetId;
        _macroSetTags = macroSetTags;
        _nodeTagService = nodeTagService;
        _inputService = inputService;
        _toastService = toastService;

        // Load tags from database
        var tagsFromDb = _nodeTagService.GetTagsForMacroSet(macroSetId).ToList();

        // Clear and repopulate the MacroSet.Tags collection
        _macroSetTags.Clear();
        foreach (var tag in tagsFromDb)
        {
            _macroSetTags.Add(tag);
        }

        // Create ObservableCollection that wraps the MacroSet.Tags collection
        Tags = new ObservableCollection<NodeTag>(_macroSetTags);

        // Keep the collections in sync
        Tags.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (NodeTag item in e.NewItems)
                {
                    if (!_macroSetTags.Contains(item))
                        _macroSetTags.Add(item);
                }
            }
            if (e.OldItems != null)
            {
                foreach (NodeTag item in e.OldItems)
                {
                    _macroSetTags.Remove(item);
                }
            }
        };
    }

    public void ImportTags(IEnumerable<NodeTag> importedTags)
    {
        // Clear existing tags from database
        var existingTags = _nodeTagService.GetTagsForMacroSet(_macroSetId).ToList();
        foreach (var tag in existingTags)
        {
            _nodeTagService.Delete(tag.TagId);
        }

        // Clear existing tags from collections
        Tags.Clear();

        // Add imported tags
        foreach (var tag in importedTags.OrderBy(t => t.Position))
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
            Tags.Add(newTag);
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

        var newTag = new NodeTag
        {
            MacroSetId = _macroSetId,
            Name = name,
            FontFamily = iconPicker.SelectedFontFamilyResult,
            Glyph = iconPicker.SelectedGlyph,
            Position = Tags.Count
        };

        _nodeTagService.Insert(newTag);
        Tags.Add(newTag);
        _toastService.Show($"Added tag: {name}");
    }

    [RelayCommand]
    private async Task DeleteTag(NodeTag tag)
    {
        if (tag == null) return;

        var confirm = await _inputService.SelectOption(
            $"Delete tag {tag.FontFamily}-{tag.Glyph}?",
            new[] { "Yes", "No" });

        if (confirm != "Yes") return;

        _nodeTagService.Delete(tag.TagId);
        Tags.Remove(tag);
        _toastService.Show($"Deleted tag: {tag.FontFamily}-{tag.Glyph}");
    }

    [RelayCommand]
    private void MoveTagUp(NodeTag tag)
    {
        if (tag == null || tag.Position == 0) return;

        var index = Tags.IndexOf(tag);
        if (index <= 0) return;

        var tagAbove = Tags[index - 1];
        tagAbove.Position++;
        tag.Position--;

        _nodeTagService.Update(tagAbove);
        _nodeTagService.Update(tag);

        Tags.Move(index, index - 1);
        _toastService.Show("Moved tag up");
    }

    [RelayCommand]
    private void MoveTagDown(NodeTag tag)
    {
        if (tag == null) return;

        var index = Tags.IndexOf(tag);
        if (index < 0 || index >= Tags.Count - 1) return;

        var tagBelow = Tags[index + 1];
        tagBelow.Position--;
        tag.Position++;

        _nodeTagService.Update(tagBelow);
        _nodeTagService.Update(tag);

        Tags.Move(index, index + 1);
        _toastService.Show("Moved tag down");
    }
}
