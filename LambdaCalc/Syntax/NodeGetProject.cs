namespace LambdaCalc.Syntax;

public static class NodeGetProject
{
    public static Project? GetProject(this INode node)
    {
        INode? cur = node;
        while (cur != null)
        {
            if (cur is Project project)
                return project;
            cur = cur.Parent;
        }
        return null;
    }
}
