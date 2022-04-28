using CourseWork.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeshImport
{
	public class MeshImporter
	{
		public const string StartPath = @"..\..\..\..\..\data\";
        private Mesh _mesh;
		private readonly string _fileNameNodes;
		private readonly string _fileNameElements;

		public MeshImporter(string fileNameNodes, string fileNameElements)
		{
			_fileNameNodes = StartPath + fileNameNodes;
			_fileNameElements = StartPath + fileNameElements;
		}

		public Mesh GetFigureMesh()
		{
			var nodes = ExcelReaderFE.GetNodes(_fileNameNodes);
			var timeFiniteElements = ExcelReaderFE.GetFiniteElements(_fileNameElements);

			var finiteElements = new List<TriangularFiniteElement>();

			foreach (var timeFiniteElement in timeFiniteElements)
			{
				var nodeNumbers = new List<int> { timeFiniteElement.Nodes[0].Id, timeFiniteElement.Nodes[1].Id, timeFiniteElement.Nodes[2].Id };
				var currentNodes = nodes.Where(x => nodeNumbers.Contains(x.Id)).ToList();
				if (currentNodes.Count == 3)
                {
					var finiteElement = new TriangularFiniteElement(currentNodes);
					finiteElement.Id = timeFiniteElement.Id;
					finiteElements.Add(finiteElement);
				}
				else
                {
					throw new ArgumentException($"В файле что-то не так с количеством узлов у элемента №{timeFiniteElement.Id}!");
                }
			}
            _mesh = new Mesh(nodes, finiteElements);
            SetPinAndForceNodes(nodes);
            _mesh.ApplyForce();
            _mesh.PinDetal();
            _mesh.SolveSlae();
            return _mesh;
		}

        private void SetPinAndForceNodes(List<Node> allNodes)
        {
            double maxY = allNodes.Max(n => n.Y);
            var nodes = allNodes.Where(n => n.Y == maxY);
            var nodesForPinAndForce1 = new List<Node>();
            var nodesForPinAndForce2 = new List<Node>();
            foreach (var node in allNodes)
            {
                if (node.X == nodes.First().X)
                {
                    nodesForPinAndForce1.Add(node);
                }
            }

            foreach (var node in allNodes)
            {
                if (node.X == nodes.Last().X)
                {
                    nodesForPinAndForce2.Add(node);
                }
            }
            nodesForPinAndForce1 = nodesForPinAndForce1.OrderBy(n => n.Y).ToList();
            nodesForPinAndForce2 = nodesForPinAndForce2.OrderBy(n => n.Y).ToList();
            var forceNodes = new List<Node>();
            forceNodes.AddRange(nodesForPinAndForce1.Take(nodesForPinAndForce1.Count / 2));
            forceNodes.AddRange(nodesForPinAndForce2.Skip(nodesForPinAndForce2.Count / 2).Take(nodesForPinAndForce2.Count / 2));
            _mesh.NodesForApplicationForce = forceNodes;
            var pinNodes = new List<Node>();
            pinNodes.AddRange(nodesForPinAndForce2.Take(nodesForPinAndForce2.Count / 2));
            pinNodes.AddRange(nodesForPinAndForce1.Skip(nodesForPinAndForce1.Count / 2).Take(nodesForPinAndForce1.Count / 2));
            _mesh.NodesForPin = pinNodes;
        }
    }
}
