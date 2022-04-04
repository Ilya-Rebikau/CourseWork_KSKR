using CourseWork.BLL.Models;

namespace CourseWork.BLL.Services
{
    public static class MyLineService
    {
        public static List<MyLine> GetMyLines(List<TriangularFiniteElement> triangularFiniteElements)
        {
            var lines = new List<MyLine>();
            foreach (var element in triangularFiniteElements)
            {
                AddUniqueLine(lines, new MyLine(new List<Node> { element.Nodes[0], element.Nodes[1] }));
                AddUniqueLine(lines, new MyLine(new List<Node> { element.Nodes[0], element.Nodes[2] }));
                AddUniqueLine(lines, new MyLine(new List<Node> { element.Nodes[1], element.Nodes[2] }));
            }

            return lines;
        }

        public static bool AddUniqueLine(List<MyLine> lines, MyLine line)
        {
            var sameLines = 0;
            foreach (var myLine in lines)
            {
                var centerNode = NodesService.GetCenterNodeOfLine(myLine.Nodes[0], myLine.Nodes[1]);
                var centerNodeOfMyLine = NodesService.GetCenterNodeOfLine(line.Nodes[0], line.Nodes[1]);
                if (myLine.Equals(lines) || centerNode.Equals(centerNodeOfMyLine))
                {
                    sameLines++;
                }
            }

            if (sameLines == 0)
            {
                lines.Add(line);
                return true;
            }

            return false;
        }

        public static bool CheckLinesForOneMatchingNode(MyLine line1, MyLine line2)
        {
            return (line1.Nodes[0].Equals(line2.Nodes[0]) || line1.Nodes[0].Equals(line2.Nodes[1]) ||
                line1.Nodes[1].Equals(line2.Nodes[1])) && !line1.Equals(line2);
        }
    }
}
