namespace ColorDrain.Maths;

internal struct Coord(int x, int y) : IEquatable<Coord>
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public override bool Equals(object? obj) => obj is Coord coord && Equals(coord);
    public bool Equals(Coord other) => X == other.X && Y == other.Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public IEnumerable<Coord> GetNeighbors(int W, int H)
    {
        
        List<Coord> neighbors = [];
        if (X > 0) neighbors.Add((X - 1, Y));
        if (X < W - 1) neighbors.Add((X + 1, Y));
        if (Y > 0) neighbors.Add((X, Y - 1));
        if (Y < H - 1) neighbors.Add((X, Y + 1));
        return neighbors;
    }
    public static bool operator ==(Coord left, Coord right) => left.Equals(right);
    public static bool operator !=(Coord left, Coord right) => !(left == right);
    public static implicit operator Coord((int x, int y) tup) => new(tup.x, tup.y);
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override string ToString() => $"({X}, {Y})";
}
