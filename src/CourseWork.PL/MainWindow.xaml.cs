using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using CourseWork.PL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static ComboBox _stressComboBox;
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

            _youngModuleTextBox = Drawer.DrawAndGetTextBox(1100, 50, 100, 16, "YoungModule", MainCanvas, "2e8");
            _poissonRatioTextBox = Drawer.DrawAndGetTextBox(1100, 90, 100, 16, "PoissonRatio", MainCanvas, "0,3");
            _thicknessTextBox = Drawer.DrawAndGetTextBox(1100, 130, 100, 16, "Thickness", MainCanvas, "0,01");
            _forceTextBox = Drawer.DrawAndGetTextBox(1100, 170, 100, 16, "Force", MainCanvas, "20000");

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

            var displacementsButton = Drawer.DrawAndGetButton(855, 430, 600, 16, "Определить перемещения", "Displacements", MainCanvas);
            displacementsButton.Click += new RoutedEventHandler(DisplacemenetsButton_Click);

            var deformationsButton = Drawer.DrawAndGetButton(855, 470, 600, 16, "Определить деформации", "Deformations", MainCanvas);
            deformationsButton.Click += new RoutedEventHandler(DeformationsButton_Click);

            _stressComboBox = Drawer.DrawAndGetComboBox(855, 510, 100, 16, new List<string> { "По X", "По Y", "По XY" }, MainCanvas);
            var stressButton = Drawer.DrawAndGetButton(970, 510, 600, 16, "Определить напряжения", "Stress", MainCanvas);
            stressButton.Click += new RoutedEventHandler(StressButton_Click);
        }

        private void DeformationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Деформации, StressCoords.X);
                RemoveOldElementsAndDrawNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StressButton_Click(object sender, RoutedEventArgs e)
        {
            StressCoords stressCoords = StressCoords.X;
            switch (_stressComboBox.Text)
            {
                case "По X":
                    stressCoords = StressCoords.X;
                    break;
                case "По Y":
                    stressCoords = StressCoords.Y;
                    break;
                case "По XY":
                    stressCoords = StressCoords.XY;
                    break;
            }

            _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Напряжения, stressCoords);
            RemoveOldElementsAndDrawNew();
        }

        private void DisplacemenetsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Перемещения, StressCoords.X);
                RemoveOldElementsAndDrawNew();
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

                DrawForceAndPin();

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

        private void DrawForceAndPin()
        {
            var nodesForApplicationForce = _mesh.NodesForApplicationForce.OrderBy(n => n.Y).ToList();
            var nodesForApplicationForceTop = nodesForApplicationForce.Take(nodesForApplicationForce.Count / 2).ToList();
            var nodesForApplicationForceBot = nodesForApplicationForce.Skip(nodesForApplicationForce.Count / 2).Take(nodesForApplicationForce.Count / 2).ToList();

            var nodesForPin = _mesh.NodesForPin.OrderBy(n => n.Y).ToList();
            var nodesForPinTop = nodesForPin.Take(nodesForPin.Count / 2).ToList();
            var nodesForPinBot = nodesForPin.Skip(nodesForPin.Count / 2).Take(nodesForPin.Count / 2).ToList();

            Drawer.DrawLine(nodesForApplicationForceTop.First().X, nodesForApplicationForceTop.First().Y,
                nodesForApplicationForceTop.Last().X, nodesForApplicationForceTop.Last().Y, Brushes.Crimson, MainCanvas, 6);
            Drawer.DrawLine(nodesForApplicationForceBot.First().X, nodesForApplicationForceBot.First().Y,
                nodesForApplicationForceBot.Last().X, nodesForApplicationForceBot.Last().Y, Brushes.Crimson, MainCanvas, 6);

            Drawer.DrawLine(nodesForPinTop.First().X, nodesForPinTop.First().Y,
                nodesForPinTop.Last().X, nodesForPinTop.Last().Y, Brushes.Aqua, MainCanvas, 6);
            Drawer.DrawLine(nodesForPinBot.First().X, nodesForPinBot.First().Y,
                nodesForPinBot.Last().X, nodesForPinBot.Last().Y, Brushes.Aqua, MainCanvas, 6);
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
                var uiElements = new List<UIElement>();
                if (_lines is not null || _polygons is not null)
                {
                    uiElements.AddRange(_lines);
                    uiElements.AddRange(_polygons);
                }

                Drawer.RemoveUIElements(uiElements, MainCanvas);
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

                _nodes = PresentationService.GetNodesForTriangularFiniteElements(_internalCircle, _rectangles);
                var myLines = PresentationService.GetLinesForTriangularFiniteElements(_internalCircle, _rectangles, _nodes);
                _triangularFiniteElements = TriangularFiniteElementsService.GetTriangularFiniteElements(myLines);
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
        private void RemoveOldElementsAndDrawNew()
        {
            RemoveOldUIElements();

            _polygons = Drawer.FillTriangularFiniteElements(_mesh.TriangularFiniteElements, MainCanvas);
            _lines = Drawer.DrawMyLines(MyLineService.GetMyLines(_triangularFiniteElements), MainCanvas);
        }

        private void RemoveOldUIElements()
        {
            var uiElements = new List<UIElement>
            {
                _arcLeft,
                _arcRight,
                _topLine,
                _botLine,
            };
            if (_outsideCircle is not null && _outsideCircle.Ellipse is not null)
            {
                uiElements.Add(_outsideCircle.Ellipse);
            }

            foreach (var rectangle in _rectangles)
            {
                uiElements.Add(rectangle.Rectangle);
            }

            if (_lines is not null || _polygons is not null)
            {
                uiElements.AddRange(_lines);
                uiElements.AddRange(_polygons);
            }

            Drawer.RemoveUIElements(uiElements, MainCanvas);
        }
    }
}
