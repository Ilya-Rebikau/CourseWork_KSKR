using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.Models.BLL;
using CourseWork.PL.Models;
using System;
using System.Collections.Generic;

namespace CourseWork.PL.Services
{
    public static class PresentationService
    {
        public static List<Node> GetNodesForTriangularFiniteElements(Circle outsideCircle, double h)
        {
            var nodeNumber = 0;
            var nodes = new List<Node>();
            for (double k = 0; k < 1100; k += h)
            {
                for (double x = 0, y = k; x < 1100; x += h)
                {
                    if (CheckForAffiliationToCircle(outsideCircle, x, y))
                    {
                        NodesService.AddUniqueNode(nodes, new Node { Id = nodeNumber, X = x, Y = y });
                    }
                }
            }

            return nodes;
        }

        public static List<MyLine> GetLinesForTriangularFiniteElements(Circle outsideCircle, double h, List<Node> nodes)
        {
            var lines = new List<MyLine>();
            foreach (var node1 in nodes)
            {
                foreach (var node2 in nodes)
                {
                    if ((node1.X - node2.X == h && node1.Y - node2.Y == h) || (node1.X == node2.X && node1.Y - node2.Y == h)
                        || (node1.X - node2.X == h && node1.Y == node2.Y) || (node1.X - node2.X == -h && node1.Y - node2.Y == h))
                    {
                        if (CheckForAffiliationToCircle(outsideCircle, node1.X, node1.Y) &&
                            CheckForAffiliationToCircle(outsideCircle, node2.X, node2.Y))
                        {
                            var line = new MyLine(new List<Node> { node1, node2 });
                            MyLineService.AddUniqueLine(lines, line);
                        }
                    }
                }
            }

            return lines;
        }

        public static bool CheckForAffiliationToCircle(Circle circle, double x, double y)
        {
            if (circle is null)
            {
                return false;
            }

            var centerX = circle.CenterCoords.X;
            var centerY = circle.CenterCoords.Y;
            var radius = circle.Ellipse.Width / 2;
            if (Math.Pow(centerX - x, 2) + Math.Pow(centerY - y, 2) <= radius * radius)
            {
                return true;
            }

            return false;
        }

        public static bool CheckForAffiliationToInternalRectangles(List<InternalRectangle> rectangles, double x, double y)
        {
            if (rectangles is null || rectangles.Count == 0)
            {
                return false;
            }

            foreach (var rectangle in rectangles)
            {
                if (x >= rectangle.Points[0].X && y >= rectangle.Points[0].Y && x <= rectangle.Points[1].X && y >= rectangle.Points[1].Y
                    && x <= rectangle.Points[2].X && y <= rectangle.Points[2].Y && x >= rectangle.Points[3].X && y <= rectangle.Points[2].Y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
