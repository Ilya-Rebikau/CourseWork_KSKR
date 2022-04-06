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

        public double Displacement { get; private set; }

        public double Deformation { get; private set; }

        public double Stress { get; private set; }

        public double[,] StressMatrix { get; private set; }

        public double GetDisplacements(double[] allDisplacements)
        {
            double[] currentElementDisplacements = new double[6];
            int i = 0;
            foreach (var node in Nodes)
            {
                currentElementDisplacements[i * 2] = allDisplacements[node.Id * 2];
                node.X += currentElementDisplacements[i * 2];
                currentElementDisplacements[i * 2 + 1] = allDisplacements[node.Id * 2 + 1];
                node.Y += currentElementDisplacements[i * 2 + 1];
                i++;
            }
            double displacement = 0;
            for (i = 0; i < 3; i++)
            {
                displacement += Math.Sqrt(Math.Pow(currentElementDisplacements[i * 2], 2) + Math.Pow(currentElementDisplacements[i * 2 + 1], 2));
            }

            return displacement / 4;
            //OldCoords = new double[,]
            //{
            //    { Nodes[0].X, Nodes[0].Y },
            //    { Nodes[1].X, Nodes[1].Y },
            //    { Nodes[2].X, Nodes[2].Y },
            //};
            //foreach (var node in Nodes)
            //{
            //    node.X += allDisplacements[node.Id * 2];
            //    node.Y += allDisplacements[node.Id * 2 + 1];
            //}

            //double displacement = 0;
            //for (int i = 0; i < 3; i++)
            //{
            //    displacement += Math.Abs(OldCoords[i, 0] - Nodes[i].X + OldCoords[i, 1] - Nodes[i].Y);
            //}

            //Displacement = displacement;
            //return displacement;
        }

        public double GetDeformation(double[] allDisplacements)
        {
            double[] currentElementNodesMoves = new double[6];
            int i = 0;
            foreach (var node in Nodes)
            {
                currentElementNodesMoves[i * 2] = allDisplacements[node.Id * 2];
                currentElementNodesMoves[i * 2 + 1] = allDisplacements[node.Id * 2 + 1];
                i++;
            }
            double[,] IC = A.Inverse();

            double[,] B = new double[3, 6];
            for (i = 0; i < 3; i++)
            {
                B[0, 2 * i + 0] = IC[1, i];
                B[0, 2 * i + 1] = 0.0;
                B[1, 2 * i + 0] = 0.0;
                B[1, 2 * i + 1] = IC[2, i];
                B[2, 2 * i + 0] = IC[2, i];
                B[2, 2 * i + 1] = IC[1, i];
            }
            var stress = B.Dot(currentElementNodesMoves);
            return Math.Sqrt(Math.Pow(stress[0], 2) + Math.Pow(stress[1], 2) - stress[0] * stress[1] + 3 * Math.Pow(stress[2], 2)) / 2;

            if (OldCoords is null || OldCoords.Length == 0)
            {
                throw new InvalidOperationException("Перемещения не найдены.");
            }

            return 0;
        }

        public double GetStress(double[] allDisplacements, StressCoords stressCoords)
        {
            if (allDisplacements is null || allDisplacements.Length == 0)
            {
                throw new InvalidOperationException("Перемещения не расчитаны");
            }

            var stressMatrix = InitMatrixForStress(allDisplacements, stressCoords);
            var intensity = B.Dot(stressMatrix).Transpose().Dot(E);
            return stressCoords switch
            {
                StressCoords.X => Math.Abs(intensity[0, 0]),
                StressCoords.Y => Math.Abs(intensity[0, 1]),
                StressCoords.XY => Math.Abs(intensity[0, 2]),
                _ => 0,
            };
        }

        private double[,] OldCoords { get; set; }

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

        private double[,] InitMatrixForStress(double[] allDisplacements, StressCoords strain)
        {
            if (allDisplacements is null || allDisplacements.Length == 0)
            {
                throw new InvalidOperationException("Узлы или перемещения не расчитаны!");
            }
            StressMatrix = new double[6, 1];
            switch (strain)
            {
                case StressCoords.X:
                    StressMatrix[0, 0] = allDisplacements[Nodes[0].Id * 2];
                    StressMatrix[2, 0] = allDisplacements[Nodes[1].Id * 2];
                    StressMatrix[4, 0] = allDisplacements[Nodes[2].Id * 2];
                    break;
                case StressCoords.Y:
                    StressMatrix[1, 0] = allDisplacements[Nodes[0].Id * 2 + 1];
                    StressMatrix[3, 0] = allDisplacements[Nodes[1].Id * 2 + 1];
                    StressMatrix[5, 0] = allDisplacements[Nodes[2].Id * 2 + 1];
                    break;
                case StressCoords.XY:
                    StressMatrix[0, 0] = allDisplacements[Nodes[0].Id * 2];
                    StressMatrix[1, 0] = allDisplacements[Nodes[0].Id * 2 + 1];
                    StressMatrix[2, 0] = allDisplacements[Nodes[1].Id * 2];
                    StressMatrix[3, 0] = allDisplacements[Nodes[1].Id * 2 + 1];
                    StressMatrix[4, 0] = allDisplacements[Nodes[2].Id * 2];
                    StressMatrix[5, 0] = allDisplacements[Nodes[2].Id * 2 + 1];
                    break;
            }

            return StressMatrix;
        }

        private void InitLocalMatrix()
        {
            if (Coefficients.PoissonRatio == 0 || Coefficients.Thickness == 0 || Coefficients.YoungModule == 0 || Coefficients.MeshStep == 0)
            {
                throw new InvalidOperationException("Коэффициент Пуассона и/или модуль Юнга и/или толщина элемента и/или шаг сетки не заданы");
            }

            LocalMatrix = B.Transpose().Dot(E).Dot(B).Multiply(Coefficients.Thickness * GetSquare());
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
