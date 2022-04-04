using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseWork.PL.Services
{
    public static class PresentationService
    {
        public static List<Node> GetNodesForApplicationForce(List<Node> nodes, List<InternalRectangle> rectangles)
        {
            if (rectangles is null || rectangles.Count != 2)
            {
                throw new ArgumentException("Прямоугольников должно быть 2!");
            }

            if (rectangles[0].CenterY > rectangles[1].CenterY)
            {
                var tmp = rectangles[0];
                rectangles[0] = rectangles[1];
                rectangles[1] = tmp;
            }

            var nodesForApplicationOfForce = new List<Node>();
            foreach (var node in nodes)
            {
                if (rectangles[0].CenterX - rectangles[0].Rectangle.Width / 2 == node.X &&
                    rectangles[0].CenterY - rectangles[0].Rectangle.Height / 2 <= node.Y &&
                    rectangles[0].CenterY + rectangles[0].Rectangle.Height / 2 >= node.Y)
                {
                    nodesForApplicationOfForce.Add(node);
                }

                if (rectangles[1].CenterX + rectangles[1].Rectangle.Width / 2 == node.X &&
                    rectangles[1].CenterY - rectangles[1].Rectangle.Height / 2 <= node.Y &&
                    rectangles[1].CenterY + rectangles[1].Rectangle.Height / 2 >= node.Y)
                {
                    nodesForApplicationOfForce.Add(node);
                }
            }

            return nodesForApplicationOfForce;
        }

        public static List<Node> GetNodesForPinModel(List<Node> nodes, List<InternalRectangle> rectangles)
        {
            if (rectangles is null || rectangles.Count != 2)
            {
                throw new ArgumentException("Прямоугольников должно быть 2!");
            }

            if (rectangles[0].CenterY > rectangles[1].CenterY)
            {
                var tmp = rectangles[0];
                rectangles[0] = rectangles[1];
                rectangles[1] = tmp;
            }

            var nodesForPin = new List<Node>();
            foreach (var node in nodes)
            {
                if (rectangles[0].CenterX + rectangles[0].Rectangle.Width / 2 == node.X &&
                    rectangles[0].CenterY - rectangles[0].Rectangle.Height / 2 <= node.Y &&
                    rectangles[0].CenterY + rectangles[0].Rectangle.Height / 2 >= node.Y)
                {
                    nodesForPin.Add(node);
                }

                if (rectangles[1].CenterX - rectangles[1].Rectangle.Width / 2 == node.X &&
                    rectangles[1].CenterY - rectangles[1].Rectangle.Height / 2 <= node.Y &&
                    rectangles[1].CenterY + rectangles[1].Rectangle.Height / 2 >= node.Y)
                {
                    nodesForPin.Add(node);
                }
            }

            return nodesForPin;
        }

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
                        nodeNumber++;
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
