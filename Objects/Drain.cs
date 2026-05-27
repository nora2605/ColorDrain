using ColorDrain.Maths;

namespace ColorDrain.Objects;

internal struct Drain(Coord position, SubColor scolor, bool filled) : Element
{
    public Coord Position { get; } = position;
    public SubColor SColor { get; } = scolor;
    public bool Filled { get; set; } = filled;

    public override string ToString() => $"Drain {Position} {SColor}";
}
