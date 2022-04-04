using Accord.Math;
using CourseWork.BLL.Services;
using System.Drawing;

namespace CourseWork.BLL.Models
{
    public class Mesh
    {
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

            int rows = GlobalMatrix.GetUpperBound(0) + 1;
            int columns = GlobalMatrix.Length / rows;
            foreach (var node in NodesForPin)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (node.Id * 2 == i)
                        {
                            GlobalMatrix[j, node.Id * 2] = 0;
                            GlobalMatrix[j, node.Id * 2 + 1] = 0;
                        }

                        if (node.Id * 2 + 1 == i)
                        {
                            GlobalMatrix[j, node.Id * 2] = 0;
                            GlobalMatrix[j, node.Id * 2 + 1] = 0;
                        }

                        if (node.Id * 2 == j)
                        {
                            GlobalMatrix[node.Id * 2, i] = 0;
                            GlobalMatrix[node.Id * 2 + 1, i] = 0;
                        }

                        if (node.Id * 2 + 1 == j)
                        {
                            GlobalMatrix[node.Id * 2, i] = 0;
                            GlobalMatrix[node.Id * 2 + 1, i] = 0;
                        }
                    }
                }
            }

            foreach (var node in NodesForPin)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (node.Id * 2 == i && node.Id * 2 == j)
                        {
                            GlobalMatrix[i, j] = 1;
                        }

                        if (node.Id * 2 + 1 == i && node.Id * 2 + 1 == j)
                        {
                            GlobalMatrix[i, j] = 1;
                        }
                    }
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
        public void GiveColorsToFiniteElemenets(MeshPaintCharacteristicType type)
        {
            if (TriangularFiniteElements is null || TriangularFiniteElements.Count == 0 ||
                Displacements is null || Displacements.Length == 0)
            {
                throw new InvalidOperationException("Не найдены конечные элементы или общие перемещения!");
            }
            double minCharacteristic = double.MaxValue;
            double maxCharacteristic = double.MinValue;
            foreach (var el in TriangularFiniteElements)
            {
                double characteristic = 0;
                switch (type)
                {
                    case MeshPaintCharacteristicType.Деформации:
                        characteristic = el.GetDeformation(Displacements);
                        break;
                    //case MeshPaintCharacteristicType.Напряжения:
                    //    characteristic = el.GetStress(nodesMoves);
                    //    break;
                    //case MeshPaintCharacteristicType.Деформации:
                    //    characteristic = el.GetDeformation(nodesMoves);
                    //    break;
                }
                if (characteristic < minCharacteristic)
                {
                    minCharacteristic = characteristic;
                }

                if (characteristic > maxCharacteristic)
                {
                    maxCharacteristic = characteristic;
                }
            }

            foreach (var el in TriangularFiniteElements)
            {
                double characteristic = 0;
                switch (type)
                {
                    case MeshPaintCharacteristicType.Деформации:
                        characteristic = el.GetDeformation(Displacements);
                        break;
                    //case MeshPaintCharacteristicType.Напряжения:
                    //    characteristic = el.GetStress(nodesMoves);
                    //    break;
                    //case MeshPaintCharacteristicType.Деформации:
                    //    characteristic = el.GetDeformation(nodesMoves);
                    //    break;
                }
                byte[] RGB = GradientMaker.GetRGBGradientValue(minCharacteristic, maxCharacteristic, characteristic);
                el.Color = new SolidBrush(Color.FromArgb(200, RGB[0], RGB[1], RGB[2]));
            }
        }
    }
}
