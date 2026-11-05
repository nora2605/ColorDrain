using ColorDrain.Maths;

namespace ColorDrain.Game;

internal struct InitialDroplet(Coord position, SubColor scolor) : Element
{
    public Coord Position { get; } = position;
    public SubColor SColor { get; } = scolor;
    public override string ToString() => $"IDroplet {Position} {SColor}";
}
