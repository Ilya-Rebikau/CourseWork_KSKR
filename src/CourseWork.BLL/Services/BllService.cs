using CourseWork.BLL.Models;

namespace CourseWork.BLL.Services
{
    public static class BllService
    {
        public static Mesh GetMesh(List<Node> nodes, List<TriangularFiniteElement> triangularFiniteElements, List<Node> nodesForPin, List<Node> nodesForApplicationOfForce)
        {
            var mesh = new Mesh(nodes, triangularFiniteElements)
            {
                NodesForPin = nodesForPin
            };
            mesh.PinDetal();
            mesh.NodesForApplicationForce = nodesForApplicationOfForce.OrderBy(n => n.Y).ToList();
            mesh.ApplyForce();
            mesh.SolveSlae();
            return mesh;
        }

        public static void SetCoefficients(string youngModule, string poissonRatio, string thickness, string meshStep, string force)
        {
            if (string.IsNullOrWhiteSpace(youngModule) || string.IsNullOrWhiteSpace(poissonRatio) ||
                string.IsNullOrWhiteSpace(thickness) || string.IsNullOrWhiteSpace(meshStep) || string.IsNullOrWhiteSpace(force))
            {
                throw new ArgumentException("Коэффициенты не заданы!");
            }

            var youngModuleParseResult = double.TryParse(youngModule, out var youngModuleValue);
            var poissonRatioParseResult = double.TryParse(poissonRatio, out var poissonRatioValue);
            var thicknessParseResult = double.TryParse(thickness, out var thicknessValue);
            var meshStepParseResult = Enum.TryParse(meshStep, out MeshStep meshStepValue);
            var forceParseResult = double.TryParse(force, out var forceValue);
            if (!youngModuleParseResult || !poissonRatioParseResult || !thicknessParseResult || !meshStepParseResult || !forceParseResult)
            {
                throw new InvalidCastException("Коэффициенты введены неверно!");
            }

            Coefficients.YoungModule = youngModuleValue;
            Coefficients.PoissonRatio = poissonRatioValue;
            Coefficients.Thickness = thicknessValue;
            Coefficients.MeshStep = (int)meshStepValue;
            Coefficients.Force = forceValue * 1000;
        }
    }
}
