namespace EuroDiffusion.Models
{
    struct Coordinate
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Coordinate c))
                return false;

            return X == c.X & Y == c.Y;
        }

        public override int GetHashCode() => X ^ Y;

        public override string ToString() => $"Coordinate {{X: {X}, Y: {Y}}}";
    }
}
