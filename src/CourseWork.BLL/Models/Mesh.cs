using CourseWork.BLL.Services;
using System.Diagnostics;

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
        }
        public List<Node> Nodes { get; set; }

        public List<TriangularFiniteElement> TriangularFiniteElements { get; set; }

        public List<Node> NodesForApplicationOfForce { get; set; }

        public List<Node> NodesForPin { get; set; }

        public double[,] GlobalMatrix { get; set; }

        public void ApplyForce()
        {
            if (NodesForApplicationOfForce is null || NodesForApplicationOfForce.Count == 0)
            {
                throw new InvalidOperationException("Узлы для приложения силы не заданы!");
            }


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
    }
}
