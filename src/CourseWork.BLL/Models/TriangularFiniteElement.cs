using Accord.Math;
using System.Drawing;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        public SolidBrush Color { get; set; } = new SolidBrush(System.Drawing.Color.FromArgb(0, 0, 255));

        public int Id { get; set; }

        public List<Node> Nodes { get; set; }

        public double[,] LocalMatrix { get; private set; }

        public double[] Displacements { get; private set; }

        public double[] Deformations { get; private set; }

        public double GetDisplacements(double[] allDisplacements)
        {
            double[] currentElementNodesDisplacements = new double[6];
            int i = 0;
            foreach (var node in Nodes)
            {
                currentElementNodesDisplacements[i * 2] = allDisplacements[node.Id * 2];
                currentElementNodesDisplacements[i * 2 + 1] = allDisplacements[node.Id * 2 + 1];
                i++;
            }
            Displacements = currentElementNodesDisplacements;
            double displacement = 0;
            for (i = 0; i < 3; i++)
            {
                displacement += Math.Sqrt(Math.Pow(currentElementNodesDisplacements[i * 2], 2) + Math.Pow(currentElementNodesDisplacements[i * 2 + 1], 2));
            }

            return displacement;
        }

        public double GetDeformation()
        {
            if (Displacements is null || Displacements.Length == 0)
            {
                throw new InvalidOperationException("Перемещения не расчитаны");
            }

            Deformations = B.Dot(Displacements);
            var deformationEq = Math.Sqrt(2) / 3 * Math.Sqrt(Math.Pow(Deformations[0] - Deformations[1], 2) + Math.Pow(Deformations[1], 2)
                + Math.Pow(Deformations[0], 2) + 3 / 2 * Math.Pow(Deformations[2], 2));
            return deformationEq;
        }

        public double GetStress()
        {
            if (Displacements is null || Displacements.Length == 0)
            {
                throw new InvalidOperationException("Перемещения не расчитаны");
            }

            var stress = E.Dot(Deformations);
            return 1 / Math.Sqrt(2) * Math.Sqrt(Math.Pow(stress[0] - stress[1], 2) + Math.Pow(stress[1], 2) + Math.Pow(stress[0], 2) +
                6 * Math.Pow(stress[2], 2));
        }

        /// <summary>
        /// Матрица координат узлов конечного элемента.
        /// </summary>
        private double[,] A => new double[,]
        {
            { 1, Nodes[0].X, Nodes[0].Y },
            { 1, Nodes[1].X, Nodes[1].Y },
            { 1, Nodes[2].X, Nodes[2].Y },
        };

        /// <summary>
        /// Вспомогательная матрица с разностями координат.
        /// </summary>
        private double[,] B
        {
            get
            {
                var node1 = Nodes[0];
                var node2 = Nodes[1];
                var node3 = Nodes[2];
                return new double[,]
                {
                    { node2.Y - node3.Y, 0, node3.Y - node1.Y, 0, node1.Y - node2.Y, 0 },
                    { 0, node3.X - node2.X, 0, node1.X - node3.X, 0, node2.X - node1.X },
                    { node3.X - node2.X, node2.Y - node3.Y, node1.X - node3.X, node3.Y - node1.Y, node2.X - node1.X, node1.Y - node2.Y },
                }.Multiply(1 / (2 * GetSquare()));
            }
        }

        /// <summary>
        /// Матрица с коэффициентом Пуассона и модулем Юнга.
        /// </summary>
        private static double[,] E
        {
            get
            {
                var multiplier = Coefficients.YoungModule / ((1 - 2 * Coefficients.PoissonRatio) * (1 + Coefficients.PoissonRatio));
                return new double[,]
                {
                    { 1 - Coefficients.PoissonRatio, Coefficients.PoissonRatio, 0 },
                    { Coefficients.PoissonRatio, 1 - Coefficients.PoissonRatio, 0 },
                    { 0, 0, (1 - 2 * Coefficients.PoissonRatio) / 2 },
                }.Multiply(multiplier);
            }
        }

        /// <summary>
        /// Площадь конечного элемента.
        /// </summary>
        /// <returns>Площадь.</returns>
        private double GetSquare()
        {
            return 0.5 * Math.Abs(A.Determinant());
        }

        private void InitLocalMatrix()
        {
            if (Coefficients.PoissonRatio == 0 || Coefficients.Thickness == 0 || Coefficients.YoungModule == 0 || Coefficients.MeshStep == 0)
            {
                throw new InvalidOperationException("Коэффициент Пуассона и/или модуль Юнга и/или толщина элемента и/или шаг сетки не заданы");
            }

            LocalMatrix = B.Transpose().Dot(E).Dot(B).Multiply(Coefficients.Thickness).Multiply(GetSquare());
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
