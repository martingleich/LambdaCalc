using LambdaCalc.Diagnostics;

namespace LambdaCalc.Syntax;

public static class NodeEx
{
    public static Location GetLocation(this INode node) => Location.FromOffsetLength(node.Offset, node.Length);
    public static IEnumerable<INode> GetChildrenAtOffset(this INode root, int offset)
    {
        if (offset >= root.Offset && offset <= root.Offset + root.Length)
        {
            yield return root;
            if (root is ISyntax syntax)
                foreach (var t2 in syntax.Children.SelectMany(c => c.GetChildrenAtOffset(offset)))
                    yield return t2;
        }
    }
    public static IEnumerable<T> AllNodesOfType<T>(this INode node) where T : INode
    {
        if (node is T t)
            yield return t;
        if(node is ISyntax syntax)
            foreach (var t2 in syntax.Children.SelectMany(c => c.AllNodesOfType<T>()))
                yield return t2;
    }
    public static INode GetRoot(this INode node)
    {
        while (node.Parent is { } parent)
            node = parent;
        return node;
    }
}
