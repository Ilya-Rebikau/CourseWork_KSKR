namespace CourseWork.BLL.Models
{
    public class Node
    {
        public int Id { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Node node &&
                   X == node.X &&
                   Y == node.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, X, Y);
        }
    }
}
