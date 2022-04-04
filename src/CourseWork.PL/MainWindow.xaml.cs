using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using CourseWork.PL.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWork.PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double CenterX = 350;
        const double CenterY = 400;
        const double InternalRadius = 250;
        const double OutsideRadius = 325;
        const double MenuLineTerm = 150;
        const double ArcLineTerm = 10;
        const double RectangleWidth = 140;
        const double RectangleHeight = 60;

        private static TextBox _youngModuleTextBox;
        private static TextBox _poissonRatioTextBox;
        private static TextBox _thicknessTextBox;
        private static TextBox _forceTextBox;
        private static ComboBox _meshStepComboBox;
        private static Circle _outsideCircle;
        private static Circle _internalCircle;
        private static List<InternalRectangle> _rectangles;
        private static Path _arcLeft;
        private static Path _arcRight;
        private static Line _topLine;
        private static Line _botLine;
        private List<Node> _nodes;
        private List<TriangularFiniteElement> _triangularFiniteElements;
        private Mesh _mesh;
        private List<Polygon> _polygons;
        private List<Line> _lines;

        public MainWindow()
        {
            InitializeComponent();

            Drawer.DrawLine(CenterX + OutsideRadius + MenuLineTerm, 0, CenterX + OutsideRadius + MenuLineTerm,
                3000, Brushes.Black, MainCanvas, 1.5);
            Drawer.DrawLine(0, 0, 3000, 0, Brushes.Black, MainCanvas);

            Drawer.DrawLabel(850, 50, 16, "Введите модуль Юнга, Па", MainCanvas);
            Drawer.DrawLabel(850, 90, 16, "Введите коэффициент Пуассона", MainCanvas);
            Drawer.DrawLabel(850, 130, 16, "Введите толщину элемента, м", MainCanvas);
            Drawer.DrawLabel(850, 170, 16, "Введите прилагаемую силу, Н", MainCanvas);

            _youngModuleTextBox = Drawer.DrawAndGetTextBox(1100, 50, 100, 16, "YoungModule", MainCanvas);
            _poissonRatioTextBox = Drawer.DrawAndGetTextBox(1100, 90, 100, 16, "PoissonRatio", MainCanvas);
            _thicknessTextBox = Drawer.DrawAndGetTextBox(1100, 130, 100, 16, "Thickness", MainCanvas, "1");
            _forceTextBox = Drawer.DrawAndGetTextBox(1100, 170, 100, 16, "Force", MainCanvas);

            Drawer.DrawLabel(850, 210, 16, "Выберите размер сетки", MainCanvas);
            _meshStepComboBox = Drawer.DrawAndGetComboBox(1100, 210, 200, 16,
                new List<string> { MeshStep.Крупная.ToString(), MeshStep.Мелкая.ToString() }, MainCanvas);

            var coefficientsButton = Drawer.DrawAndGetButton(855, 250, 600, 16, "Принять значения коэффициентов и размера сетки",
                "CoefficientsButton", MainCanvas);
            coefficientsButton.Click += new RoutedEventHandler(CoefficientsButton_Click);

            var drawModelButton = Drawer.DrawAndGetButton(855, 290, 300, 16, "Отрисовать чертёж модели", "DrawModelButton", MainCanvas);
            drawModelButton.Click += new RoutedEventHandler(DrawModelButton_Click);

            var removeModelButton = Drawer.DrawAndGetButton(1155, 290, 600, 16, "Стереть чертёж модели", "RemoveModelButton", MainCanvas);
            removeModelButton.Click += new RoutedEventHandler(RemoveModelButton_Click);

            var getTriangularFiniteElementsButton = Drawer.DrawAndGetButton(855, 330, 600, 16, "Разбить модель на треугольные конечные элементы.\n" +
                "Отрисовать их.",
                "DoMathButton", MainCanvas);
            getTriangularFiniteElementsButton.Click += new RoutedEventHandler(DrawAndGetTriangularFiniteElementsButton_Click);

            var showForceAndPinButton = Drawer.DrawAndGetButton(855, 390, 600, 16, "Закрепить деталь и приложить силу", "ShowForceAndPinButton", MainCanvas);
            showForceAndPinButton.Click += new RoutedEventHandler(ShowForceAndPinButton_Click);

            var deformationsButton = Drawer.DrawAndGetButton(855, 430, 600, 16, "Определить деформации", "Deformations", MainCanvas);
            deformationsButton.Click += new RoutedEventHandler(DeformationsButton_Click);
        }

        private void DeformationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Деформации);
                if (_lines is not null || _polygons is not null)
                {
                    var uiElements = new List<UIElement>
                    {
                        _outsideCircle.Ellipse,
                        _arcLeft,
                        _arcRight,
                        _topLine,
                        _botLine,
                    };
                    foreach (var rectangle in _rectangles)
                    {
                        uiElements.Add(rectangle.Rectangle);
                    }
                    uiElements.AddRange(_lines);
                    uiElements.AddRange(_polygons);
                    Drawer.RemoveUIElements(uiElements, MainCanvas);
                }

                _polygons = Drawer.FillTriangularFiniteElements(_mesh.TriangularFiniteElements, MainCanvas);
                _lines = Drawer.DrawMyLines(MyLineService.GetMyLines(_triangularFiniteElements), MainCanvas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowForceAndPinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mesh = BllService.GetMesh(_nodes, _triangularFiniteElements, PresentationService.GetNodesForPinModel(_nodes, _rectangles),
                    PresentationService.GetNodesForApplicationForce(_nodes, _rectangles));

                Drawer.DrawLine(_rectangles[0].Points[1].X, _rectangles[0].Points[1].Y,
                    _rectangles[0].Points[2].X, _rectangles[0].Points[2].Y, Brushes.Blue, MainCanvas, 6);
                Drawer.DrawLine(_rectangles[1].Points[3].X, _rectangles[1].Points[3].Y,
                    _rectangles[1].Points[0].X, _rectangles[1].Points[0].Y, Brushes.Blue, MainCanvas, 6);

                Drawer.DrawLine(_rectangles[0].Points[3].X, _rectangles[0].Points[3].Y,
                    _rectangles[0].Points[0].X, _rectangles[0].Points[0].Y, Brushes.Crimson, MainCanvas, 6);
                Drawer.DrawLine(_rectangles[1].Points[1].X, _rectangles[1].Points[1].Y,
                    _rectangles[1].Points[2].X, _rectangles[1].Points[2].Y, Brushes.Crimson, MainCanvas, 6);

                Drawer.DrawAndGetArcSegment(new Point(650, 150), new Size(250, 250),
                    new Point(740, 300), Brushes.Black, MainCanvas);
                Drawer.DrawLine(740, 300, 740, 280, Brushes.Black, MainCanvas);
                Drawer.DrawLine(740, 300, 731, 282, Brushes.Black, MainCanvas);
                Drawer.DrawLabel(700, 150, 18, "Момент силы", MainCanvas, 50);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoveModelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uiElements = new List<UIElement>
                {
                    _outsideCircle.Ellipse,
                    _arcLeft,
                    _arcRight,
                    _topLine,
                    _botLine,
                };
                foreach (var rectangle in _rectangles)
                {
                    uiElements.Add(rectangle.Rectangle);
                }

                Drawer.RemoveUIElements(uiElements, MainCanvas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawAndGetTriangularFiniteElementsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_lines is not null || _polygons is not null)
                {
                    var uiElements = new List<UIElement>
                    {
                        _outsideCircle.Ellipse,
                        _arcLeft,
                        _arcRight,
                        _topLine,
                        _botLine,
                    };
                    foreach (var rectangle in _rectangles)
                    {
                        uiElements.Add(rectangle.Rectangle);
                    }
                    uiElements.AddRange(_lines);
                    uiElements.AddRange(_polygons);
                    Drawer.RemoveUIElements(uiElements, MainCanvas);
                }

                if (_outsideCircle is null)
                {
                    _outsideCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, OutsideRadius * 2, Brushes.Black);
                }

                _nodes = PresentationService.GetNodesForTriangularFiniteElements(_outsideCircle);
                var myLines = PresentationService.GetLinesForTriangularFiniteElements(_outsideCircle, _nodes);
                _triangularFiniteElements = TriangularFiniteElementsService.GetTriangularFiniteElements(myLines);
                if (_internalCircle is null)
                {
                    _internalCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, InternalRadius * 2, Brushes.Black);
                }

                if (_rectangles is null || _rectangles.Count == 0)
                {
                    _rectangles = new List<InternalRectangle>
                    {
                        Drawer.GetAndDrawRectangle(CenterX, CenterY - InternalRadius, RectangleWidth, RectangleHeight, Brushes.Black),
                        Drawer.GetAndDrawRectangle(CenterX, CenterY + InternalRadius, RectangleWidth, RectangleHeight, Brushes.Black),
                    };
                }

                _polygons = Drawer.FillTriangularFiniteElements(_triangularFiniteElements, MainCanvas);
                _lines = Drawer.DrawMyLines(myLines, MainCanvas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void CoefficientsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BllService.SetCoefficients(_youngModuleTextBox.Text, _poissonRatioTextBox.Text,
                    _thicknessTextBox.Text, _meshStepComboBox.Text, _forceTextBox.Text);
                MessageBox.Show("Коэффициенты успешно заданы.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawModelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _outsideCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, OutsideRadius * 2, Brushes.Black, MainCanvas);
                _internalCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, InternalRadius * 2, Brushes.Black);
                _rectangles = new List<InternalRectangle>
                {
                    Drawer.GetAndDrawRectangle(CenterX, CenterY - InternalRadius, RectangleWidth, RectangleHeight, Brushes.Black, MainCanvas),
                    Drawer.GetAndDrawRectangle(CenterX, CenterY + InternalRadius, RectangleWidth, RectangleHeight, Brushes.Black, MainCanvas),
                };
                _arcLeft = Drawer.DrawAndGetArcSegment(new Point(CenterX - RectangleWidth / 2, CenterY + InternalRadius - ArcLineTerm),
                    new Size(InternalRadius, InternalRadius), new Point(CenterX - RectangleWidth / 2, CenterY - InternalRadius + ArcLineTerm),
                    Brushes.Black, MainCanvas);
                _arcRight = Drawer.DrawAndGetArcSegment(new Point(CenterX + RectangleWidth / 2, CenterY - InternalRadius + ArcLineTerm),
                    new Size(InternalRadius, InternalRadius), new Point(CenterX + RectangleWidth / 2, CenterY + InternalRadius - ArcLineTerm),
                    Brushes.Black, MainCanvas);
                _topLine = Drawer.DrawLine(CenterX - RectangleWidth / 2, CenterY - InternalRadius - ArcLineTerm, CenterX + RectangleWidth / 2,
                    CenterY - InternalRadius - ArcLineTerm, Brushes.Black, MainCanvas);
                _botLine = Drawer.DrawLine(CenterX - RectangleWidth / 2, CenterY + InternalRadius + ArcLineTerm, CenterX + RectangleWidth / 2,
                    CenterY + InternalRadius + ArcLineTerm, Brushes.Black, MainCanvas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
