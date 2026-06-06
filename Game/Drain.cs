using ColorDrain.Maths;

namespace ColorDrain.Game;

internal record Drain(Coord Position, SubColor SColor, bool Filled) : Element
{
    public override string ToString() => $"Drain {Position} {SColor}";
}
