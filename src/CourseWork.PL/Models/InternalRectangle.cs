using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace CourseWork.PL.Models
{
    public class InternalRectangle
    {
        public InternalRectangle(Rectangle rectangle, double centerX, double centerY)
        {
            Rectangle = rectangle;
            CenterX = centerX;
            CenterY = centerY;
            InitPoints();
        }
        public Rectangle Rectangle { get; set; }

        public double CenterX { get; set; }

        public double CenterY { get; set; }

        public List<Point> Points { get; private set; } = new List<Point>();

        private void InitPoints()
        {
            double width = Rectangle.Width;
            double height = Rectangle.Height;
            Points.Add(new Point(CenterX - width / 2, CenterY - height / 2));
            Points.Add(new Point(CenterX + width / 2, CenterY - height / 2));
            Points.Add(new Point(CenterX + width / 2, CenterY + height / 2));
            Points.Add(new Point(CenterX - width / 2, CenterY + height / 2));
        }
    }
}
