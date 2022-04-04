using System.Drawing;
using System.Drawing.Drawing2D;

namespace CourseWork.BLL.Models
{
    /// <summary>
    /// Градиентная шкала
    /// </summary>
    public class GradientScale
    {
        /// <summary>
        /// Создание градиентной шкалы
        /// </summary>
        /// <param name="minValue">Минимальное значение шкалы</param>
        /// <param name="maxValue">Максимальное значение шкалы</param>
        public GradientScale(double minValue, double maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        public void Draw(Graphics g, int bottomLeftAngleX, int bottomLeftAngleY, int width, int height)
        {
            FirstBrush = new LinearGradientBrush(new Point(bottomLeftAngleX, bottomLeftAngleY - height / 10), new Point(bottomLeftAngleX + width / 2, bottomLeftAngleY - height / 10), Color.Red, Color.Green);
            SecondBrush = new LinearGradientBrush(new Point(bottomLeftAngleX + width / 2, bottomLeftAngleY - height / 10), new Point(width, bottomLeftAngleY - height / 10), Color.Green, Color.Blue);

            g.FillRectangle(FirstBrush, bottomLeftAngleX, bottomLeftAngleY - height / 10, width / 2, 200);
            g.FillRectangle(SecondBrush, bottomLeftAngleX + width / 2, bottomLeftAngleY - height / 10, width / 2, 200);


            double currentValue = MaxValue;
            double currentPos = bottomLeftAngleX;

            double stepValue = (MaxValue - MinValue) / 10;
            double stepPos = (width - bottomLeftAngleX) / 10;
            float y = bottomLeftAngleY - height / 10;
            while (MinValue - currentValue < 1e-6)
            {
                g.DrawString(currentValue.ToString("e3"), SystemFonts.DefaultFont, Brushes.Black, new PointF((float)currentPos, y));

                currentPos += stepPos;
                currentValue -= stepValue;
            }
        }

        public LinearGradientBrush FirstBrush { get; set; }
        public LinearGradientBrush SecondBrush { get; set; }

        public double MaxValue { get; set; }
        public double MinValue { get; set; }

        public string MaxValueText
        {
            get
            {
                return MaxValue.ToString("e3");
            }
        }
        public string MinValueText
        {
            get
            {
                return MinValue.ToString("e3");
            }
        }
    }
}
