﻿using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;
public interface INodeService<TParent, TChild>
        where TParent : Node, IParentNode<TParent, TChild>, TChild
        where TChild : Node
{
    void Delete(TChild node);
    TChild Get(int id);
    TParent GetRoot(int id);
    void Insert(TChild node);
    bool IsDescendant(TParent ancestor, TChild potentialDescendant);
    void ReAttachNodes(TParent root);
    void Update(TChild node);
}

public class NodeService<TParent, TChild> : INodeService<TParent, TChild>
    where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
    where TChild : Node
{
    IRepository<TChild> _nodeRepository;
    IRepository<NodeClosure> _closureRepository;
    public NodeService(IRepository<TChild> nodeRepository, IRepository<NodeClosure> closureRepository)
    {
        _nodeRepository = nodeRepository;
        _closureRepository = closureRepository;
    }

    public TParent GetRoot(int id)
    {
        var rootNode = _nodeRepository.Get(n => n.NodeId == id).FirstOrDefault();
        var root = (TParent)rootNode;
        if (root == null)
        {
            root = new TParent() { Name = "root" };
            Insert(root);
        }

        LoadNode(root);

        return root;
    }

    private void LoadNode(TChild node)
    {
        if (node is TParent parent)
        {
            var directDescendants = _closureRepository.Get(c => c.AncestorId == node.NodeId && c.Depth == 1);
            parent.Children = directDescendants.Select(c => c.Descendant).Cast<TChild>().ToList();

            foreach (var child in parent.Children)
            {
                LoadNode(child);
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
            var children = parent.Children;
            parent.Children = null;
            _nodeRepository.Insert(parent);
            _nodeRepository.Save();
            Resolve(node);
            foreach (var child in children)
            {
                child.ParentId = parent.NodeId;
                Insert(child);
            }
            parent.Children = children;
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
                Name = $"{node.Name} -> {node.Name}",
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
        _nodeRepository.DetachEntities(originalRoot);
        _nodeRepository.AttachEntities(root);
    }

    private IEnumerable<TChild> GetAllNodes(TChild node)
    {
        yield return node;

        if (node is TParent parent)
        {
            foreach (var child in parent.Children)
            {
                foreach (var childNodes in GetAllNodes(child))
                {
                    yield return childNodes;
                }
            }
        }
    }
}