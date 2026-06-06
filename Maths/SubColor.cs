using Raylib_cs;

namespace ColorDrain.Maths;

internal record struct SubColor(int C, int M, int Y) : IEquatable<SubColor>
{
    public static SubColor Mix(params IEnumerable<SubColor> a) => Normalize(a.Sum(e => e.C), a.Sum(e => e.M), a.Sum(e => e.Y));

    private static SubColor Normalize(int C, int M, int Y)
    {
        int gcd = GCD(GCD(C, M), Y);
        // large number prevention !!
        if (((int[])[C, M, Y]).Any(i => i > int.MaxValue / 4))
            gcd++;
        return new(C / gcd, M / gcd, Y / gcd);
    }
    public readonly Color GetRGB()
    {
        float max = ((int[])[C, M, Y, 1]).Max();
        return new Color(
            1f - (C / max),
            1f - (M / max),
            1f - (Y / max)
        );
    }

    private static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);
    public override int GetHashCode() => HashCode.Combine(C, M, Y);
    public static implicit operator SubColor((int C, int M, int Y) sc) => new(sc.C, sc.M, sc.Y);
    public static implicit operator (int C, int M, int Y)(SubColor sc) => (sc.C, sc.M, sc.Y);
    public static implicit operator Color(SubColor sc) => sc.GetRGB();

    public override string ToString() => $"({C}, {M}, {Y})";
}
