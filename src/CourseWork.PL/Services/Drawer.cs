using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CourseWork.BLL.Models;
using CourseWork.PL.Models;

namespace CourseWork.PL.Services
{
    internal class Drawer
    {
        public static Rectangle GetAndDrawGradientScale(double x, double y, double minValue, double maxValue, double height, Canvas cv)
        {
            var myLinearGradientBrush = new LinearGradientBrush();

            double ratio = 0;
            byte B = (byte)Math.Max(0, 255 * (1 - ratio));
            byte R = (byte)Math.Max(0, 255 * (ratio - 1));
            byte G = (byte)(255 - B - R);
            myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(R, G, B), 0));
            ratio = 2 * ((maxValue - minValue) / 2 - minValue) / (maxValue - minValue);
            B = (byte)Math.Max(0, 255 * (1 - ratio));
            R = (byte)Math.Max(0, 255 * (ratio - 1));
            G = (byte)(255 - B - R);
            myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(R, G, B), 0.5));
            ratio = 2;
            B = (byte)Math.Max(0, 255 * (1 - ratio));
            R = (byte)Math.Max(0, 255 * (ratio - 1));
            G = (byte)(255 - B - R);
            myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(R, G, B), 1));

            var rectangle = new Rectangle
            {
                Width = 20,
                Height = height
            };
            myLinearGradientBrush.StartPoint = new Point(0, 1);
            myLinearGradientBrush.EndPoint = new Point(0, 0);
            rectangle.Fill = myLinearGradientBrush;
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            cv.Children.Add(rectangle);
            return rectangle;
        }
        public static List<Polygon> FillTriangularFiniteElements(List<TriangularFiniteElement> triangularFiniteElements,
            Canvas cv, double strokeThickness = 0)
        {
            var polygons = new List<Polygon>();
            foreach (var element in triangularFiniteElements)
            {
                var polygon = new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(element.Nodes[0].X, element.Nodes[0].Y),
                        new Point(element.Nodes[1].X, element.Nodes[1].Y),
                        new Point(element.Nodes[2].X, element.Nodes[2].Y),
                    },
                    Fill = new SolidColorBrush(Color.FromArgb(element.Color.Color.A, element.Color.Color.R, element.Color.Color.G,
                        element.Color.Color.B)),
                    StrokeThickness = strokeThickness,
                };
                cv.Children.Add(polygon);
                polygons.Add(polygon);
            }

            return polygons;
        }

        public static void RemoveUIElements(List<UIElement> uiElements, Canvas cv)
        {
            foreach (var element in uiElements)
            {
                cv.Children.Remove(element);
            }
        }

        public static ComboBox DrawAndGetComboBox(double x, double y, double width, double fontSize, List<string> items, Canvas cv)
        {
            var comboBox = new ComboBox
            {
                FontSize = fontSize,
                Width = width,
                ItemsSource = items,
                Text = items[0],
            };
            Canvas.SetLeft(comboBox, x);
            Canvas.SetTop(comboBox, y);
            cv.Children.Add(comboBox);
            return comboBox;
        }

        public static Path DrawAndGetArcSegment(Point point, Size size, Point startPoint, SolidColorBrush color, Canvas cv,
            bool isClosed = false, double strokeThickness = 3)
        {
            var arcSegment = new ArcSegment
            {
                Point = point,
                Size = size,
            };
            var pathSegmentCollection = new PathSegmentCollection
            {
                arcSegment,
            };
            var pathFigure = new PathFigure
            {
                Segments = pathSegmentCollection,
                IsClosed = isClosed,
                StartPoint = startPoint,
            };
            var pathFigureCollection = new PathFigureCollection
            {
                pathFigure,
            };
            var pathGeomerty = new PathGeometry
            {
                Figures = pathFigureCollection,
            };
            var path = new Path
            {
                StrokeThickness = strokeThickness,
                Data = pathGeomerty,
                Stroke = color,
            };
            cv.Children.Add(path);
            return path;
        }

        public static Label DrawLabel(double x, double y, double fontSize, string content, Canvas cv, double angle = 0)
        {
            var label = new Label()
            {
                FontSize = fontSize,
                Content = content,
            };
            if (angle != 0)
            {
                var rotateTransform = new RotateTransform(angle);
                label.RenderTransform = rotateTransform;
            }
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);
            cv.Children.Add(label);
            return label;
        }

        public static TextBox DrawAndGetTextBox(double x, double y, double width, double fontSize, string name, Canvas cv, string text = null)
        {
            var textBox = new TextBox
            {
                Name = name,
                Width = width,
                FontSize = fontSize,
                Text = text,
            };
            Canvas.SetLeft(textBox, x);
            Canvas.SetTop(textBox, y);
            cv.Children.Add(textBox);
            return textBox;
        }

        public static Button DrawAndGetButton(double x, double y, double width, double fontSize, string content, string name, Canvas cv)
        {
            var button = new Button
            {
                Name = name,
                MaxWidth = width,
                FontSize = fontSize,
                Content = content,
            };
            Canvas.SetLeft(button, x);
            Canvas.SetTop(button, y);
            cv.Children.Add(button);
            return button;
        }

        public static List<Line> DrawMyLines(List<MyLine> myLines, Canvas cv)
        {
            var lines = new List<Line>();
            foreach (var line in myLines)
            {
                var node1 = line.Nodes[0];
                var node2 = line.Nodes[1];
                lines.Add(DrawLine(node1.X, node1.Y, node2.X, node2.Y, Brushes.Black, cv, 1));
            }

            return lines;
        }

        public static Circle GetAndDrawCircle(double x, double y, double diameter, SolidColorBrush color,
            Canvas cv = null, double strokeThickness = 3)
        {
            var ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Stroke = color,
                StrokeThickness = strokeThickness,
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

        public static InternalRectangle GetAndDrawRectangle(double x, double y, double width, double height, SolidColorBrush color,
            Canvas cv = null, double strokeThickness = 3)
        {
            var rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = color,
                StrokeThickness = strokeThickness,
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

        public static Line DrawLine(double x1, double y1, double x2, double y2, SolidColorBrush color, Canvas cv,
            double strokeThickness = 3)
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
