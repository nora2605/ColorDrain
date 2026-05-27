using ColorDrain.Maths;

namespace ColorDrain.Objects;

internal struct Droplet(Coord position, SubColor scolor) : Element
{
    public Coord Position { get; } = position;
    public SubColor SColor { get; } = scolor;

    public override string ToString() => $"Droplet {Position} {SColor}";
}