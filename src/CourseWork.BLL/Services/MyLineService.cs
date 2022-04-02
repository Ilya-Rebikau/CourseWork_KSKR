using CourseWork.BLL.Models;

namespace CourseWork.BLL.Services
{
    public static class MyLineService
    {
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
