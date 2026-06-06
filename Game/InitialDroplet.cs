using ColorDrain.Maths;

namespace ColorDrain.Game;

internal struct InitialDroplet(Coord position, SubColor scolor) : Element
{
    public Coord Position { get; } = position;
    public SubColor SColor { get; } = scolor;
}
