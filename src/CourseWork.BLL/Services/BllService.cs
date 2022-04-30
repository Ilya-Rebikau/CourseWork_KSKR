using CourseWork.Bll.Models;
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

        public static void SetCoefficients(List<Material> materials, string materialName, string meshStep, string force)
        {
            if (string.IsNullOrWhiteSpace(materialName) || string.IsNullOrWhiteSpace(meshStep) || string.IsNullOrWhiteSpace(force))
            {
                throw new ArgumentException("Материал или сила или размер сетки не заданы!");
            }

            var material = materials.SingleOrDefault(m => m.Name == materialName);
            var meshStepParseResult = Enum.TryParse(meshStep, out MeshStep meshStepValue);
            var forceParseResult = double.TryParse(force, out var forceValue);
            if (!meshStepParseResult || !forceParseResult)
            {
                throw new InvalidCastException("Коэффициенты введены неверно!");
            }

            Coefficients.YoungModule = material.YoungModule;
            Coefficients.PoissonRatio = material.PoissonRatio;
            Coefficients.Thickness = 1;
            Coefficients.MeshStep = (int)meshStepValue;
            Coefficients.Force = forceValue * 10;
        }
    }
}
