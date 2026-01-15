using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;

public interface INodeTagService
{
    NodeTag Get(int tagId);
    IEnumerable<NodeTag> GetTagsForMacroSet(int macroSetId);
    void Insert(NodeTag tag);
    void Update(NodeTag tag);
    void Delete(int tagId);
    void Save();
}

public class NodeTagService(IRepository<NodeTag> tagRepository) : INodeTagService
{
    readonly IRepository<NodeTag> _tagRepository = tagRepository;

    public NodeTag Get(int tagId)
    {
        return _tagRepository.Get(t => t.TagId == tagId).FirstOrDefault();
    }

    public IEnumerable<NodeTag> GetTagsForMacroSet(int macroSetId)
    {
        return _tagRepository.Get(t => t.MacroSetId == macroSetId).OrderBy(t => t.Position);
    }

    public void Insert(NodeTag tag)
    {
        tag.TagId = 0;
        _tagRepository.Insert(tag);
        _tagRepository.Save();
    }

    public void Update(NodeTag tag)
    {
        _tagRepository.Update(tag);
    }

    public void Delete(int tagId)
    {
        _tagRepository.Delete(tagId);
        _tagRepository.Save();
    }

    public void Save()
    {
        _tagRepository.Save();
    }
}
