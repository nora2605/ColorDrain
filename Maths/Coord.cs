namespace ColorDrain.Maths;

internal record struct Coord(int X, int Y) : IEquatable<Coord>
{
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static implicit operator Coord((int x, int y) tup) => new(tup.x, tup.y);
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override string ToString() => $"({X}, {Y})";
}
