using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using CourseWork.PL.Services;

namespace CourseWork.PL
{
    internal class Drawer
    {
        public static void DrawLines(List<MyLine> lines, Circle internalCircle, Circle outsideCircle, List<InternalRectangle> rectangles, Canvas cv)
        {
            foreach (var line in lines)
            {
                var node1 = line.Nodes[0];
                var node2 = line.Nodes[1];
                var centerNode = NodesService.GetCenterNodeOfLine(node1, node2);
                if ((PresentationService.CheckForAffiliationToCircle(internalCircle, node1.X, node2.Y) ||
                    PresentationService.CheckForAffiliationToCircle(internalCircle, node1.X, node2.Y))
                    && !PresentationService.CheckForAffiliationToInternalRectangles(rectangles, centerNode.X, centerNode.Y))
                {
                    DrawLine(node1.X, node1.Y, node2.X, node2.Y, 1, Brushes.Green, cv);
                }
                else if (PresentationService.CheckForAffiliationToCircle(outsideCircle, node1.X, node1.Y) &&
                    PresentationService.CheckForAffiliationToCircle(outsideCircle, node2.X, node2.Y) &&
                    !PresentationService.CheckForAffiliationToInternalRectangles(rectangles, centerNode.X, centerNode.Y))
                {
                    DrawLine(node1.X, node1.Y, node2.X, node2.Y, 1, Brushes.Crimson, cv);
                }
                else if (PresentationService.CheckForAffiliationToInternalRectangles(rectangles, node1.X, node1.Y))
                {
                    DrawLine(node1.X, node1.Y, node2.X, node2.Y, 1, Brushes.Black, cv);
                }
            }
        }

        public static Circle GetAndDrawCircle(double x, double y, double diameter, Canvas cv = null)
        {
            var ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Stroke = Brushes.Black,
                StrokeThickness = 3,
            };
            if (cv is not null)
            {
                cv.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, x - diameter / 2);
                Canvas.SetTop(ellipse, y - diameter / 2);
            }
            var circle = new Circle
            {
                Ellipse = ellipse,
                CenterCoords = new Point(x, y),
            };
            return circle;
        }

        public static InternalRectangle GetAndDrawRectangle(double x, double y, double width, double height, Canvas cv = null)
        {
            var rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Black,
                StrokeThickness = 3,
            };
            if (cv is not null)
            {
                cv.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, x - width / 2);
                Canvas.SetTop(rectangle, y - height / 2);
            }
            var internalRectangle = new InternalRectangle(rectangle, x, y);
            return internalRectangle;
        }

        public static Line DrawLine(double x1, double y1, double x2, double y2, int strokeThickness, SolidColorBrush color, Canvas cv)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = color,
                StrokeThickness = strokeThickness,
            };
            cv.Children.Add(line);
            return line;
        }
    }
}
