using Accord.Math;
using CourseWork.BLL.Services;
using System.Drawing;

namespace CourseWork.BLL.Models
{
    public class Mesh
    {
        private bool _wasMoved = false;

        public Mesh(List<Node> nodes, List<TriangularFiniteElement> triangularFiniteElements)
        {
            if (nodes is null || nodes.Count == 0 || triangularFiniteElements is null || triangularFiniteElements.Count == 0)
            {
                throw new ArgumentException("Количество узлов и треугольных конечных элементов не может быть равно 0");
            }

            Nodes = nodes;
            TriangularFiniteElements = triangularFiniteElements;
            GlobalMatrix = GetGlobalStiffnessMatrix();

        }

        public List<Node> Nodes { get; set; }

        public List<TriangularFiniteElement> TriangularFiniteElements { get; set; }

        /// <summary>
        /// Узлы для приложения силы.
        /// </summary>
        public List<Node> NodesForApplicationForce { get; set; }

        /// <summary>
        /// Узлы для закрепления.
        /// </summary>
        public List<Node> NodesForPin { get; set; }

        public double[,] GlobalMatrix { get; private set; }

        /// <summary>
        /// Вектор с приложенной силой на выбранные узлы.
        /// </summary>
        public double[] Answers { get; private set; }

        /// <summary>
        /// Вектор перемещений.
        /// </summary>
        public double[] Displacements { get; private set; }

        public double[] SolveSlae()
        {
            if (GlobalMatrix is null || GlobalMatrix.Length == 0 ||
                Answers is null || Answers.Length == 0)
            {
                throw new InvalidOperationException("Глобальная матрица и/или вектор ответов не заданы!");
            }

            Displacements = GlobalMatrix.Solve(Answers);
            return Displacements;
        }

        public double[] ApplyForce()
        {
            if (GlobalMatrix is null || GlobalMatrix.Length == 0 ||
                NodesForApplicationForce is null || NodesForApplicationForce.Count == 0 ||
                Nodes is null || Nodes.Count == 0)
            {
                throw new InvalidOperationException("Узлы сетки и/или узлы для приложения силы не заданы!");
            }

            var answers = new double[Nodes.Count * 2];
            NodesForApplicationForce = NodesForApplicationForce.OrderBy(n => n.Y).ToList();
            var nodesForApplicationForceTop = NodesForApplicationForce.Take(NodesForApplicationForce.Count / 2);
            var nodesForApplicationForceBot = NodesForApplicationForce.Skip(NodesForApplicationForce.Count / 2).Take(NodesForApplicationForce.Count / 2);
            foreach (var node in nodesForApplicationForceTop)
            {
                for (int i = 0; i < answers.Length; i++)
                {
                    if (node.Id * 2 == i)
                    {
                        answers[i] = Coefficients.Force;
                        //answers[i + 1] = Coefficients.Force; //по Y не прикладываю силу
                    }
                }
            }

            foreach (var node in nodesForApplicationForceBot)
            {
                for (int i = 0; i < answers.Length; i++)
                {
                    if (node.Id * 2 == i)
                    {
                        answers[i] = -Coefficients.Force;
                        //answers[i + 1] = -Coefficients.Force; //по Y не прикладываю силу
                    }
                }
            }

            Answers = answers;
            return Answers;
        }

        public void PinDetal()
        {
            if (GlobalMatrix is null || GlobalMatrix.Length == 0 || NodesForPin is null || NodesForPin.Count == 0)
            {
                throw new InvalidOperationException("Глобальная матрица и/или узлы для закрепления не заданы!");
            }

            var globalMatrix = GlobalMatrix;
            var nodesCount = Nodes.Count;
            foreach (var node in Nodes)
            {
                if (NodesForPin.Contains(node))
                {
                    for (int j = 0; j < nodesCount * 2; j++)
                    {
                        globalMatrix[2 * node.Id, j] = 0;
                        globalMatrix[2 * node.Id + 1, j] = 0;
                        globalMatrix[j, 2 * node.Id] = 0;
                        globalMatrix[j, 2 * node.Id + 1] = 0;
                    }

                    globalMatrix[2 * node.Id, 2 * node.Id] = 1;
                    globalMatrix[2 * node.Id + 1, 2 * node.Id + 1] = 1;
                }
            }
        }

        public double[,] GetGlobalStiffnessMatrix()
        {
            double[,] stiffnessMatrix = new double[Nodes.Count * 2, Nodes.Count * 2];
            foreach (var element in TriangularFiniteElements)
            {
                for (int i = 0; i < element.Nodes.Count; i++)
                {
                    for (int j = 0; j < element.Nodes.Count; j++)
                    {
                        stiffnessMatrix[2 * element.Nodes[i].Id, 2 * element.Nodes[j].Id] += element.LocalMatrix[2 * i, 2 * j];
                        stiffnessMatrix[2 * element.Nodes[i].Id, 2 * element.Nodes[j].Id + 1] += element.LocalMatrix[2 * i, 2 * j + 1];
                        stiffnessMatrix[2 * element.Nodes[i].Id + 1, 2 * element.Nodes[j].Id] += element.LocalMatrix[2 * i + 1, 2 * j];
                        stiffnessMatrix[2 * element.Nodes[i].Id + 1, 2 * element.Nodes[j].Id + 1] += element.LocalMatrix[2 * i + 1, 2 * j + 1];
                    }
                }
            }

            return stiffnessMatrix;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        public (double, double) GiveColorsToFiniteElemenets(MeshPaintCharacteristicType type)
        {
            if (TriangularFiniteElements is null || TriangularFiniteElements.Count == 0 ||
                Displacements is null || Displacements.Length == 0)
            {
                throw new InvalidOperationException("Не найдены конечные элементы или общие перемещения!");
            }

            if (_wasMoved is false)
            {
                foreach (var node in Nodes)
                {
                    node.X += Displacements[node.Id * 2];
                    node.Y += Displacements[node.Id * 2 + 1];
                }
                _wasMoved = true;
            }

            var characteristics = new List<double>();
            foreach (var el in TriangularFiniteElements)
            {
                double characteristic = 0;
                switch (type)
                {
                    case MeshPaintCharacteristicType.Перемещения:
                        characteristic = el.GetDisplacements(Displacements);
                        break;
                    case MeshPaintCharacteristicType.Деформации:
                        characteristic = el.GetDeformation();
                        break;
                    case MeshPaintCharacteristicType.Напряжения:
                        characteristic = el.GetStress();
                        break;
                }
                characteristics.Add(characteristic);
            }
            var minCharacteristic = characteristics.Min();
            var maxCharacteristic = characteristics.Max();
            foreach (var el in TriangularFiniteElements)
            {
                double characteristic = 0;
                switch (type)
                {
                    case MeshPaintCharacteristicType.Перемещения:
                        characteristic = el.GetDisplacements(Displacements);
                        break;
                    case MeshPaintCharacteristicType.Деформации:
                        characteristic = el.GetDeformation();
                        break;
                    case MeshPaintCharacteristicType.Напряжения:
                        characteristic = el.GetStress();
                        break;
                }
                byte[] RGB = GradientMaker.GetRGBGradientValue(minCharacteristic, maxCharacteristic, characteristic);
                el.Color = new SolidBrush(Color.FromArgb(200, RGB[0], RGB[1], RGB[2]));
            }

            return (minCharacteristic, maxCharacteristic);
        }
    }
}
