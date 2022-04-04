namespace CourseWork.BLL.Models
{
    public class MyLine
    {
        public MyLine(List<Node> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("Линия должна состоять из двух узлов");
            }

            Nodes = nodes;
        }

        public List<Node> Nodes { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MyLine lines)
            {
                var nodes1 = new HashSet<Node>(lines.Nodes);
                return nodes1.SetEquals(Nodes);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nodes);
        }
    }
}
