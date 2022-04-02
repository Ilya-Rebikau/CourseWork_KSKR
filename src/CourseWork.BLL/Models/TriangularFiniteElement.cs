using CourseWork.Models.BLL;

namespace CourseWork.BLL.Models
{
    public class TriangularFiniteElement
    {
        public TriangularFiniteElement(List<Node> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("В треугольном конечном элементе должно быть 3 узла!");
            }

            Nodes = nodes;
            InitLocalMatrix();
        }

        public int Id { get; set; }

        public List<Node> Nodes { get; set; }

        public List<MyLine> Lines { get; private set; } = new List<MyLine>();

        public double[,] LocalMatrix { get; private set; }

        private void InitLocalMatrix()
        {
            var node0 = Nodes[0];
            var node1 = Nodes[1];
            var node2 = Nodes[2];
            var square = node1.X * node2.Y + node0.X * node1.Y + node2.X * node0.Y;
            var element00 = 0;
            var element01 = ((node1.Y - node2.Y) * (node2.Y - node0.Y) + (node2.X - node1.X) * (node0.X - node2.X)) / (4 * square);
            var element02 = ((node2.Y - node1.Y) * (node1.Y - node0.Y) + (node1.X - node2.X) * (node0.X - node1.X)) / (4 * square);
            var element10 = ((node0.Y - node2.Y) * (node2.Y - node1.Y) + (node2.X - node0.X) * (node1.X - node2.X)) / (4 * square);
            var element11 = 0;
            var element12 = ((node2.Y - node0.Y) * (node0.Y - node1.Y) + (node0.X - node2.X) * (node1.X - node0.X)) / (4 * square);
            var element20 = ((node0.Y - node1.Y) * (node1.Y - node2.Y) + (node1.X - node0.X) * (node2.X - node1.X)) / (4 * square);
            var element21 = ((node1.Y - node0.Y) * (node0.Y - node2.Y) + (node0.X - node1.X) * (node0.X - node2.X)) / (4 * square);
            var element22 = 0;
            LocalMatrix = new double[,]
            {
                { element00, element01, element02 },
                { element10, element11, element12 },
                { element20, element21, element22 },
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is TriangularFiniteElement triangularFiniteElement)
            {
                var nodes1 = new HashSet<Node>(triangularFiniteElement.Nodes);
                return nodes1.SetEquals(Nodes);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Nodes);
        }
    }
}
