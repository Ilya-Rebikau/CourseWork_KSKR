using CourseWork.BLL.Models;

namespace CourseWork.BLL.Services
{
    public static class NodesService
    {
        public static bool AddUniqueNode(List<Node> nodes, Node newNode)
        {
            if (!nodes.Contains(newNode))
            {
                nodes.Add(newNode);
                return true;
            }

            return false;
        }

        public static Node GetCenterNodeOfLine(Node node1, Node node2)
        {
            double x = node1.X < node2.X ? node1.X + (node2.X - node1.X) / 2 : node2.X + (node1.X - node2.X) / 2;
            double y = node1.Y < node2.Y ? node1.Y + (node2.Y - node1.Y) / 2 : node2.Y + (node1.Y - node2.Y) / 2;
            return new Node { X = x, Y = y };
        }
    }
}
