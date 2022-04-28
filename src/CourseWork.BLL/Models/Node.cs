namespace CourseWork.BLL.Models
{
    public class Node
    {
        public Node() 
        { }
        public Node(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }
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
