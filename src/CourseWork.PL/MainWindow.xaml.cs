using CourseWork.BLL.Models;
using CourseWork.BLL.Services;
using CourseWork.PL.Models;
using CourseWork.PL.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Coefficients.Thickness = 1;
            Coefficients.YoungModule = 2e11;
            Coefficients.PoissonRatio = 0.3;
            Drawer.DrawLine(1100, 0, 1100, 2000, 3, Brushes.Black, MainCanvas);
            double h = 20;
            var outsideCircle = Drawer.GetAndDrawCircle(550, 400, 650, MainCanvas);
            var nodes = PresentationService.GetNodesForTriangularFiniteElements(outsideCircle, h);
            var lines = PresentationService.GetLinesForTriangularFiniteElements(outsideCircle, h, nodes);
            var triangularElements = TriangularFiniteElementsService.GetTriangularFiniteElements(lines);
            var mesh = new Mesh(nodes, triangularElements);
            var globalMatrix = mesh.GetGlobalStiffnessMatrix();
            mesh.GlobalMatrix = globalMatrix;
            var rectangles = new List<InternalRectangle>
            {
                Drawer.GetAndDrawRectangle(550, 150, 140, 60, MainCanvas),
                Drawer.GetAndDrawRectangle(550, 650, 140, 60, MainCanvas)
            };
            var nodesForPin = PresentationService.GetNodesForPinModel(nodes, rectangles);
            mesh.NodesForPin = nodesForPin;
            mesh.PinDetal();
            var nodesForApplicationForce = PresentationService.GetNodesForApplicationForce(nodes, rectangles);
            for (int i = 0; i < nodesForApplicationForce.Count; i++)
            {
                Drawer.GetAndDrawCircle(nodesForApplicationForce[i].X, nodesForApplicationForce[i].Y, 10, MainCanvas);
            }
            var internalCircle = Drawer.GetAndDrawCircle(550, 400, 500, MainCanvas);
            Drawer.DrawLines(lines, internalCircle, outsideCircle, rectangles, MainCanvas);
            Drawer.DrawLine(480, 140, 620, 140, 3, Brushes.Black, MainCanvas);
            Drawer.DrawLine(480, 660, 620, 660, 3, Brushes.Black, MainCanvas);
        }
    }
}
