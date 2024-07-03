using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;
public interface INodeService<TParent, TChild>
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
{
    void Delete(TChild node);
    TChild Get(int id);
    TParent GetRoot(int id);
    void Insert(TChild node);
    bool IsDescendant(TParent ancestor, TChild potentialDescendant);
    void ReAttachNodes(TParent root);
    void Update(TChild node);
    void Save();
    IEnumerable<TTarget> GetDescendants<TTarget>(TChild root) where TTarget : TChild;
}

public class NodeService<TParent, TChild>(IRepository<TChild> nodeRepository, IRepository<NodeClosure> closureRepository) : INodeService<TParent, TChild>
    where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
    where TChild : Node
{
    readonly IRepository<TChild> _nodeRepository = nodeRepository;
    readonly IRepository<NodeClosure> _closureRepository = closureRepository;

    public TParent GetRoot(int id)
    {
        var rootNode = _nodeRepository.Get(n => n.NodeId == id).FirstOrDefault();
        var root = (TParent)rootNode;
        if (root == null)
        {
            root = new TParent() { Name = "root" };
            Insert(root);
            root.RootId = root.NodeId;
            _nodeRepository.Update(root);
            _nodeRepository.Save();
        }

        var closures = _closureRepository.Get(c => c.NodeRootId == root.RootId);
        LoadNode(root, closures);

        return root;
    }

    private void LoadNode(TChild node, IEnumerable<NodeClosure> closures)
    {
        if (node is TParent parent)
        {
            var directDescendants = closures.Where(c => c.AncestorId == node.NodeId && c.Depth == 1);
            parent.Nodes = directDescendants.Select(c => c.Descendant).Cast<TChild>().ToList();

            foreach (var child in parent.Nodes)
            {
                LoadNode(child, closures);
            }
        }
    }

    public TChild Get(int id)
    {
        return _nodeRepository.Get(n => n.NodeId == id).FirstOrDefault();
    }

    public bool IsDescendant(TParent ancestor, TChild potentialDescendant)
    {
        return _closureRepository.Get(c => c.AncestorId == ancestor.NodeId && c.DescendantId == potentialDescendant.NodeId).Any();
    }

    public void Insert(TChild node)
    {
        if (node is TParent parent)
        {
            parent.NodeId = 0;
            _nodeRepository.Insert(parent);
            _nodeRepository.Save();
            Resolve(node);

            var children = parent.Nodes.ToList();
            foreach (var child in children)
            {
                child.ParentId = parent.NodeId;
                child.RootId = parent.RootId;
                Insert(child);
            }
            _nodeRepository.Save();
        }
        else
        {
            node.NodeId = 0;
            _nodeRepository.Insert(node);
            _nodeRepository.Save();
            Resolve(node);
        }
    }

    public void Update(TChild node)
    {
        _nodeRepository.Update(node);
        _nodeRepository.Save();
        Resolve(node);
    }

    public void Delete(TChild node)
    {
        if (node is TParent parent)
        {
            var children = parent.Nodes.ToList();
            foreach (var child in children)
            {
                Delete(child);
            }
        }
        DeleteClosures(node);
        _nodeRepository.Delete(node);
        _nodeRepository.Save();
    }

    //https://dirtsimple.org/2010/11/simplest-way-to-do-tree-based-queries.html
    private void Resolve(TChild node)
    {
        DeleteClosures(node);

        var selfClosure = new NodeClosure()
        {
            NodeRootId = node.RootId,
            Name = $"{node.Name} -> {node.Name}",
            AncestorId = node.NodeId,
            AncestorName = node.Name,
            DescendantId = node.NodeId,
            DescendantName = node.Name,
            Depth = 0
        };
        _closureRepository.Insert(selfClosure);

        var ancestorClosures = _closureRepository.Get(c => c.DescendantId == node.ParentId);
        foreach (var closure in ancestorClosures)
        {
            var ancestorClosure = new NodeClosure()
            {
                NodeRootId = node.RootId,
                Name = $"{closure.Name} -> {node.Name}",
                AncestorId = closure.AncestorId,
                AncestorName = closure.AncestorName,
                DescendantId = node.NodeId,
                DescendantName = node.Name,
                Depth = closure.Depth + 1
            };
            _closureRepository.Insert(ancestorClosure);
        }

        _closureRepository.Save();
    }

    private void DeleteClosures(TChild node)
    {
        var existingColosures = _closureRepository.Get(c => c.DescendantId == node.NodeId);
        foreach (var closure in existingColosures)
        {
            _closureRepository.Delete(closure);
        }
        _closureRepository.Save();
    }
    public void ReAttachNodes(TParent root)
    {
        var originalRoot = Get(root.NodeId);
        var originalDescendants = GetDescendants<TChild>(originalRoot).ToArray();
        _nodeRepository.DetachEntities(originalDescendants);
        var newDescendants = GetDescendants<TChild>(root).ToArray();
        _nodeRepository.AttachEntities(newDescendants);
    }

    public IEnumerable<TTarget> GetDescendants<TTarget>(TChild node) where TTarget: TChild
    {
        if (node is TTarget target) yield return target;

        if (node is TParent parent)
        {
            foreach (var child in parent.Nodes)
            {
                foreach (var childNodes in GetDescendants<TTarget>(child))
                {
                    yield return childNodes;
                }
            }
        }
    }

    public void Save()
    {
        _nodeRepository.Save();
    }
}