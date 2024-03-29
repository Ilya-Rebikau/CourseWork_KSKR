﻿using CourseWork.Bll.Models;
using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using CourseWork.PL.Services;
using MeshImport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const double CenterX = 360;
        private const double CenterY = 420;
        private const double MenuLineTerm = 500;
        private const double TermAfterMenuLine = 10;
        private const double TermAfterLeftElement = 250;
        private double _internalRadius = 240;
        private double _rectangleWidth = 180;
        private double _rectangleHeight = 120;
        private static ComboBox _materialsComboBox;
        private static TextBox _forceTextBox;
        private static ComboBox _meshStepComboBox;
        private static Circle _internalCircle;
        private static List<InternalRectangle> _rectangles;
        private static Line _forceTopLine;
        private static Line _forceBotLine;
        private static Line _pinTopLine;
        private static Line _pinBotLine;
        private static Rectangle _scale;
        private static Label _minValue;
        private static Label _midValue;
        private static Label _maxValue;
        private static Line _minLine;
        private static Line _midLine;
        private static Line _maxLine;
        private static Button _findSizeButton;
        private List<Node> _nodes;
        private List<TriangularFiniteElement> _triangularFiniteElements;
        private Mesh _mesh;
        private List<Polygon> _polygons;
        private List<Line> _lines;
        private List<Material> _materials;

        public MainWindow()
        {
            InitializeComponent();

            Drawer.DrawLine(CenterX + MenuLineTerm, 0, CenterX + MenuLineTerm,
                3000, Brushes.Black, MainCanvas, 1.5);
            Drawer.DrawLine(0, 0, 3000, 0, Brushes.Black, MainCanvas);

            Drawer.DrawLabel(CenterX + MenuLineTerm + TermAfterMenuLine, 50, 16, "Выберите материал", MainCanvas);
            _materials = PresentationService.GetMaterials();
            _materialsComboBox = Drawer.DrawAndGetComboBox(CenterX + MenuLineTerm + TermAfterMenuLine + TermAfterLeftElement, 50, 150, 16, _materials.Select(m => m.Name).ToList(), MainCanvas);
            
            Drawer.DrawLabel(CenterX + MenuLineTerm + TermAfterMenuLine, 90, 16, "Введите прилагаемую силу, Н", MainCanvas);
            _forceTextBox = Drawer.DrawAndGetTextBox(CenterX + MenuLineTerm + TermAfterMenuLine + TermAfterLeftElement, 90, 100, 16, "Force", MainCanvas, "10000");

            Drawer.DrawLabel(CenterX + MenuLineTerm + TermAfterMenuLine, 130, 16, "Выберите размер сетки", MainCanvas);
            _meshStepComboBox = Drawer.DrawAndGetComboBox(CenterX + MenuLineTerm + TermAfterMenuLine + TermAfterLeftElement, 130, 200, 16,
                new List<string> { MeshStep.Крупная.ToString(), MeshStep.Средняя.ToString(), MeshStep.Мелкая.ToString() }, MainCanvas);

            var getTriangularFiniteElementsButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine, 200, 600, 16, "Разбить модель на треугольные конечные\nэлементы. " +
                "Отрисовать их. Закрепить\nдеталь и приложить силу.",
                "DoMathButton", MainCanvas);
            getTriangularFiniteElementsButton.Click += new RoutedEventHandler(DrawAndGetTriangularFiniteElementsPinAndForceDetailButton_Click);
            
            var getTriangularFiniteElementsFromFileButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine + 350, 200, 600, 16, "Получить сетку из файла.\nЗакрепить деталь и приложить силу.",
                "FileButton", MainCanvas);
            getTriangularFiniteElementsFromFileButton.Click += new RoutedEventHandler(GetTriangularFiniteElementsFromFileButton_Click);

            var displacementsButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine, 300, 600, 16, "Определить перемещения, мм", "Displacements", MainCanvas);
            displacementsButton.Click += new RoutedEventHandler(DisplacemenetsButton_Click);

            var deformationsButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine, 340, 600, 16, "Определить деформации, мм/мм", "Deformations", MainCanvas);
            deformationsButton.Click += new RoutedEventHandler(DeformationsButton_Click);

            var stressButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine, 380, 600, 16, "Определить напряжения, Н/мм^2", "Stress", MainCanvas);
            stressButton.Click += new RoutedEventHandler(StressButton_Click);

            _findSizeButton = Drawer.DrawAndGetButton(CenterX + MenuLineTerm + TermAfterMenuLine, 420, 600, 16, "Определить размеры детали", "Sizes", MainCanvas);
            _findSizeButton.Click += new RoutedEventHandler(FindSizeButton_Click);
        }

        private void FindSizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var material = _materials.SingleOrDefault(m => m.Name == _materialsComboBox.Text);
                var currentStress = double.Parse(_maxValue.Content.ToString());
                if (material.MaxStress * 1e-6 <= currentStress)
                {
                    MessageBox.Show("Текущее напряжение превышает допустимое напряжение для данного материала.\n"
                                    + "Нажмите \"ОК\", чтобы подобрать размеры.");
                    _internalCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, _internalRadius * 2, Brushes.Black);
                    _rectangles = new List<InternalRectangle>
                    {
                        Drawer.GetAndDrawRectangle(CenterX, CenterY - _internalRadius, _rectangleWidth, _rectangleHeight, Brushes.Black),
                        Drawer.GetAndDrawRectangle(CenterX, CenterY + _internalRadius, _rectangleWidth, _rectangleHeight, Brushes.Black),
                    };

                    _nodes = PresentationService.GetNodesForTriangularFiniteElements(_internalCircle, _rectangles);
                    var myLines = PresentationService.GetLinesForTriangularFiniteElements(_internalCircle, _rectangles, _nodes);
                    _triangularFiniteElements = TriangularFiniteElementsService.GetTriangularFiniteElements(myLines);
                    var newElementSquare = Coefficients.Force / (material.MaxStress * 1e-6);
                    var oldElementSquare = _triangularFiniteElements.First().GetSquare();
                    var difference = newElementSquare / oldElementSquare + 1;
                    var oldCircleSquare = Math.PI * _internalRadius * _internalRadius;
                    var totalElementsInOldSquare = oldCircleSquare / oldElementSquare;
                    var newDiameter = Math.Sqrt(2 * difference * totalElementsInOldSquare * Coefficients.MeshStep * Coefficients.MeshStep / Math.PI);
                    var newWidth = _rectangleWidth * Math.Sqrt(difference);
                    var newHeight = _rectangleHeight * Math.Sqrt(difference);
                    MessageBox.Show("Подобранные размеры при заданной силе:\n"
                                    + $"Диаметр вала: {newDiameter} мм\n"
                                    + $"Ширина шпонки: {newWidth} мм\n"
                                    + $"Высота шпонки: {newHeight} мм\n");
                }
                else
                {
                    MessageBox.Show("Текущее напряжение не превышает допустимое для данного материала.\n"
                                    + "Текущие размеры детали:\n"
                                    + $"Диаметр вала: {2 * _internalRadius} мм\n"
                                    + $"Ширина шпонки: {_rectangleWidth} мм\n"
                                    + $"Высота шпонки: {_rectangleHeight} мм\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeformationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveScale();
                var minmax = _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Деформации);
                minmax.Item1 = minmax.Item1 * 2 * 10e-3;
                minmax.Item2 = minmax.Item2 * 2 * 10e-3;
                DrawScale(minmax);
                RemoveOldElementsAndDrawNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StressButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveScale();
                var minmax = _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Напряжения);
                minmax.Item1 *= 10e-3;
                minmax.Item2 *= 10e-3;
                DrawScale(minmax);
                RemoveOldElementsAndDrawNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DisplacemenetsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveScale();
                var minmax = _mesh.GiveColorsToFiniteElemenets(MeshPaintCharacteristicType.Перемещения);
                minmax.Item1 *= 1e-2;
                minmax.Item2 *= 1e-2;
                DrawScale(minmax);
                RemoveOldElementsAndDrawNew();
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

            _forceTopLine = Drawer.DrawLine(nodesForApplicationForceTop.First().X, nodesForApplicationForceTop.First().Y,
                nodesForApplicationForceTop.Last().X, nodesForApplicationForceTop.Last().Y, Brushes.Crimson, MainCanvas, 6);
            _forceBotLine = Drawer.DrawLine(nodesForApplicationForceBot.First().X, nodesForApplicationForceBot.First().Y,
                nodesForApplicationForceBot.Last().X, nodesForApplicationForceBot.Last().Y, Brushes.Crimson, MainCanvas, 6);

            _pinTopLine = Drawer.DrawLine(nodesForPinTop.First().X, nodesForPinTop.First().Y,
                nodesForPinTop.Last().X, nodesForPinTop.Last().Y, Brushes.Aqua, MainCanvas, 6);
            _pinBotLine = Drawer.DrawLine(nodesForPinBot.First().X, nodesForPinBot.First().Y,
                nodesForPinBot.Last().X, nodesForPinBot.Last().Y, Brushes.Aqua, MainCanvas, 6);
        }

        private void GetTriangularFiniteElementsFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _findSizeButton.IsEnabled = false;
                RemoveScale();
                RemovePolygonsLinesForceAndPinLines();
                BllService.SetCoefficients(_materials, _materialsComboBox.Text, MeshStep.Мелкая.ToString(), _forceTextBox.Text);
                var meshImporter = new MeshImporter("nodes.xlsx", "elements.xlsx");
                _mesh = meshImporter.GetFigureMesh();
                _triangularFiniteElements = _mesh.TriangularFiniteElements;
                _nodes = _mesh.Nodes;
                _polygons = Drawer.FillTriangularFiniteElements(_mesh.TriangularFiniteElements, MainCanvas);
                DrawForceAndPin();
                DrawForceArc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawAndGetTriangularFiniteElementsPinAndForceDetailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _findSizeButton.IsEnabled = true;
                RemoveScale();
                BllService.SetCoefficients(_materials, _materialsComboBox.Text, _meshStepComboBox.Text, _forceTextBox.Text);
                SetRectangleWidth();
                RemovePolygonsLinesForceAndPinLines();

                _internalCircle = Drawer.GetAndDrawCircle(CenterX, CenterY, _internalRadius * 2, Brushes.Black);
                _rectangles = new List<InternalRectangle>
                {
                    Drawer.GetAndDrawRectangle(CenterX, CenterY - _internalRadius, _rectangleWidth, _rectangleHeight, Brushes.Black),
                    Drawer.GetAndDrawRectangle(CenterX, CenterY + _internalRadius, _rectangleWidth, _rectangleHeight, Brushes.Black),
                };

                _nodes = PresentationService.GetNodesForTriangularFiniteElements(_internalCircle, _rectangles);
                var myLines = PresentationService.GetLinesForTriangularFiniteElements(_internalCircle, _rectangles, _nodes);
                _triangularFiniteElements = TriangularFiniteElementsService.GetTriangularFiniteElements(myLines);
                _polygons = Drawer.FillTriangularFiniteElements(_triangularFiniteElements, MainCanvas);
                _lines = Drawer.DrawMyLines(myLines, MainCanvas);

                _mesh = BllService.GetMesh(_nodes, _triangularFiniteElements, PresentationService.GetNodesForPinModel(_nodes, _rectangles),
                    PresentationService.GetNodesForApplicationForce(_nodes, _rectangles));
                DrawForceAndPin();
                DrawForceArc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawForceArc()
        {
            Drawer.DrawAndGetArcSegment(new Point(650, 150), new Size(250, 250),
                new Point(740, 300), Brushes.Black, MainCanvas);
            Drawer.DrawLine(740, 300, 740, 280, Brushes.Black, MainCanvas);
            Drawer.DrawLine(740, 300, 731, 282, Brushes.Black, MainCanvas);
            Drawer.DrawLabel(700, 150, 18, "Момент силы", MainCanvas, 50);
        }

        private void RemoveOldElementsAndDrawNew()
        {
            RemoveOldUIElements();

            _polygons = Drawer.FillTriangularFiniteElements(_mesh.TriangularFiniteElements, MainCanvas);
            _lines = Drawer.DrawMyLines(MyLineService.GetMyLines(_triangularFiniteElements), MainCanvas);
        }

        private void RemoveOldUIElements()
        {
            var uiElements = new List<UIElement>();
            if (_rectangles is not null && _rectangles.Count != 0)
            {
                foreach (var rectangle in _rectangles)
                {
                    uiElements.Add(rectangle.Rectangle);
                }
            }

            if (_lines is not null)
            {
                uiElements.AddRange(_lines);
            }

            if (_polygons is not null)
            {
                uiElements.AddRange(_polygons);
            }

            if (_forceBotLine is not null || _forceTopLine is not null)
            {
                uiElements.Add(_forceBotLine);
                uiElements.Add(_forceTopLine);
            }

            Drawer.RemoveUIElements(uiElements, MainCanvas);
        }

        private void SetRectangleWidth()
        {
            RemoveOldUIElements();
            try
            {
                if (Coefficients.MeshStep == (int)MeshStep.Средняя)
                {
                    _rectangleWidth = 8 * (int)MeshStep.Средняя;
                }
                if (Coefficients.MeshStep == (int)MeshStep.Крупная)
                {
                    _rectangleWidth = 6 * (int)MeshStep.Крупная;
                }
                if (Coefficients.MeshStep == (int)MeshStep.Мелкая)
                {
                    _rectangleWidth = 18 * (int)MeshStep.Мелкая;
                }
            }
            catch
            {
                throw new InvalidOperationException("Шаг сетки не задан!");
            }
        }

        private void RemovePolygonsLinesForceAndPinLines()
        {
            var uiElements = new List<UIElement>();
            if (_polygons is not null)
            {
                uiElements.AddRange(_polygons);
            }

            if (_lines is not null)
            {
                uiElements.AddRange(_lines);
            }

            if (_pinBotLine is not null || _pinTopLine is not null || _forceBotLine is not null || _forceTopLine is not null)
            {
                uiElements.Add(_pinBotLine);
                uiElements.Add(_pinTopLine);
                uiElements.Add(_forceBotLine);
                uiElements.Add(_forceTopLine);
            }

            Drawer.RemoveUIElements(uiElements, MainCanvas);
            _rectangles = null;
        }

        private void RemoveScale()
        {
            if (_scale is not null)
            {
                var uiElements = new List<UIElement>
                {
                    _scale,
                    _minLine,
                    _midLine,
                    _maxLine,
                    _minValue,
                    _midValue,
                    _maxValue,
                };

                Drawer.RemoveUIElements(uiElements, MainCanvas);
            }
        }

        private void DrawScale((double, double) minmax)
        {
            _scale = Drawer.GetAndDrawGradientScale(0, 50, minmax.Item1, minmax.Item2, 200, MainCanvas);
            _maxLine = Drawer.DrawLine(0, 250, 180, 250, Brushes.Black, MainCanvas, 3);
            _maxValue = Drawer.DrawLabel(21, 25, 14, minmax.Item2.ToString(), MainCanvas);
            _midLine = Drawer.DrawLine(0, 150, 180, 150, Brushes.Black, MainCanvas, 3);
            _midValue = Drawer.DrawLabel(21, 125, 14, ((minmax.Item2 - minmax.Item1) / 2).ToString(), MainCanvas);
            _minLine = Drawer.DrawLine(0, 50, 180, 50, Brushes.Black, MainCanvas, 3);
            _minValue = Drawer.DrawLabel(21, 225, 14, minmax.Item1.ToString(), MainCanvas);
        }
    }
}
