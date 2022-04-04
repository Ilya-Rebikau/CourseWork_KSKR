using CourseWork.BLL.Models;

namespace CourseWork.BLL.Services
{
    public static class TriangularFiniteElementsService
    {
        public static List<TriangularFiniteElement> GetTriangularFiniteElements(List<MyLine> lines)
        {
            var triangularFiniteElements = new List<TriangularFiniteElement>();
            foreach (var line1 in lines)
            {
                foreach (var line2 in lines)
                {
                    if (MyLineService.CheckLinesForOneMatchingNode(line1, line2))
                    {
                        foreach (var line3 in lines)
                        {
                            if (MyLineService.CheckLinesForOneMatchingNode(line1, line3) &&
                                MyLineService.CheckLinesForOneMatchingNode(line2, line3))
                            {
                                var triangularFiniteElement = CreateTriangularFiniteElement(line1, line2, line3);
                                if (triangularFiniteElement is not null)
                                {
                                    AddUniqueTriangularFiniteElement(triangularFiniteElements, triangularFiniteElement);
                                }
                            }
                        }
                    }
                }
            }

            return triangularFiniteElements;
        }

        private static TriangularFiniteElement CreateTriangularFiniteElement(MyLine line1, MyLine line2, MyLine line3)
        {
            TriangularFiniteElement triangularFiniteElement = null;
            if (line1.Nodes[0].Equals(line3.Nodes[0]) && line2.Nodes[0].Equals(line3.Nodes[1]) ||
                line1.Nodes[0].Equals(line3.Nodes[1]) && line2.Nodes[0].Equals(line3.Nodes[0]))
            {
                triangularFiniteElement = new TriangularFiniteElement(new List<Node> { line3.Nodes[0], line3.Nodes[1], line1.Nodes[1] });
            }
            else if (line1.Nodes[1].Equals(line3.Nodes[0]) && line2.Nodes[1].Equals(line3.Nodes[1]) ||
                line1.Nodes[1].Equals(line3.Nodes[1]) && line2.Nodes[1].Equals(line3.Nodes[0]))
            {
                triangularFiniteElement = new TriangularFiniteElement(new List<Node> { line3.Nodes[0], line3.Nodes[1], line1.Nodes[0] });
            }

            return triangularFiniteElement;
        }

        private static void AddUniqueTriangularFiniteElement(List<TriangularFiniteElement> triangularFiniteElements, TriangularFiniteElement newTriangularFiniteElement)
        {
            if (!triangularFiniteElements.Contains(newTriangularFiniteElement))
            {
                newTriangularFiniteElement.Id = triangularFiniteElements.Count;
                triangularFiniteElements.Add(newTriangularFiniteElement);
            }
        }
    }
}
